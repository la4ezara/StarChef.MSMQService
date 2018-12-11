using System.Data.SqlClient;
using log4net;

namespace StarChef.SqlQueue.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fourth.StarChef.Invariables;
    using StarChef.Common;
    using StarChef.Orchestrate;
    using StarChef.SqlQueue.Service.Interface;

    public class Listener : IListener
    {
        private readonly IStarChefMessageSender _messageSender;
        private readonly IDatabaseManager _databaseManager;
        private readonly IAppConfiguration _appConfiguration;
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private HashSet<UserDatabase> _userDatabases;

        public Listener(IAppConfiguration appConfiguration, IDatabaseManager databaseManager, IStarChefMessageSender messageSender)
        {
            this._databaseManager = databaseManager;
            _messageSender = messageSender;
            this._appConfiguration = appConfiguration;
            this.ActiveThreads = new List<Thread>();
        }

        public async Task<bool> ExecuteAsync()
        {
            int updateCacheCounter = 0;

            while (CanProcess)
            {
                int dbId = default(int);
                var dbForDeletion = new List<int>();

                try
                {
                    var timeSpan = TimeSpan.FromMinutes(_appConfiguration.SleepMinutes);
                    Thread.Sleep(timeSpan);

                    if (updateCacheCounter == 0)
                    {
                        Logger.Info($"Load databases and orchestration settings");
                        this._userDatabases = _databaseManager.GetUserDatabases(this._appConfiguration.UserDSN);
                        foreach (var userDatabase in _userDatabases)
                        {
                            LoadOrchestrationSettings(dbForDeletion, userDatabase);
                        }

                        if (dbForDeletion.Any())
                        {
                            foreach (var db in dbForDeletion)
                            {
                                this._userDatabases.RemoveWhere(c => c.DatabaseId == db);
                            }
                        }
                    }

                    updateCacheCounter++;
                    if (updateCacheCounter == _appConfiguration.CacheInterval)
                    {
                        updateCacheCounter = 0;
                    }

                    for (int i = this.ActiveThreads.Count - 1; i >= 0; i--)
                    {
                        if (!this.ActiveThreads[i].IsAlive)
                        {
                            this.ActiveThreads.RemoveAt(i);
                        }
                    }

                   Parallel.ForEach(this._userDatabases, StartProcess);
                }
                catch (Exception e)
                {
                    var message = $"Database ID: { dbId }";
                    Logger.Error(message, e);
                }
            }

            return await Task.FromResult<bool>(true);
        }

        private void LoadOrchestrationSettings(List<int> dbForDeletion, UserDatabase userDatabase)
        {
            try
            {
                var reader = _databaseManager.ExecuteReader(userDatabase.ConnectionString, "sc_get_orchestration_lookups");
                while (reader.Read())
                {
                    var entityTypeId = reader.GetValue<int>("entity_type_id");
                    var canPublish = reader.GetValue<bool>("can_publish");
                    var lookup = new OrchestrationLookup(entityTypeId, canPublish);
                    userDatabase.OrchestrationLookups.Add(lookup);
                }
            }
            catch (Exception ex)
            {
                dbForDeletion.Add(userDatabase.DatabaseId);
                var message = $"Database ID: { userDatabase.DatabaseId }";
                Logger.Error(message, ex);
            }
        }

        private void StartProcess(UserDatabase userDatabase)
        {
            var threadName = $"ThreadDbId{userDatabase.DatabaseId}";
            var count = _databaseManager.ExecuteScalar(userDatabase.ConnectionString, "sc_calculation_queue_count");
            Common.Repository.IPricingRepository repo = new Common.Repository.PricingRepository(userDatabase.ConnectionString, Constants.TIMEOUT_MSMQ_EXEC_STOREDPROC);
            Common.Engine.IPriceEngine engine = new Common.Engine.PriceEngine(repo);

            if (count > this._appConfiguration.NewThreadMessages)
            {
                if (ActiveThreads.Count <= _appConfiguration.MaxThreadCount)
                {
                    if (this.ActiveThreads.All(t => t.Name != threadName))
                    {
                        var t = new Thread(() => Process(engine, userDatabase, count)) { Name = threadName };
                        t.Start();
                        ActiveThreads.Add(t);
                    }
                }
            }
            else
            {
                Process(engine, userDatabase, count);
            }
        }

        public virtual HashSet<CalculateUpdateMessage> GetDatabaseMessages(UserDatabase userDatabase) {
            HashSet<CalculateUpdateMessage> messages = new HashSet<CalculateUpdateMessage>();
            var sqlParam = new SqlParameter("@count", System.Data.SqlDbType.Int) { Value = _appConfiguration.MessagesCount };
            var reader = _databaseManager.ExecuteReader(userDatabase.ConnectionString, "sc_calculation_dequeue", sqlParam);

            while (reader.Read())
            {
                var id = reader.GetValue<int>("Id");
                var entityId = reader.GetValue<int>("EntityId");
                var entityTypeId = reader.GetValue<int>("EntityTypeId");
                var statusId = reader.GetValue<int>("StatusId");
                var retryCount = reader.GetValue<int>("RetryCount");
                var dateCreaded = reader.GetValue<DateTime>("DateCreated");
                var externalId = reader.GetValue<string>("ExternalId");

                var messageActionType = Constants.MessageActionType.NoMessage;

                if (reader.IsDBNull("MessageActionTypeId"))
                {
                    messageActionType = Constants.MessageActionType.StarChefEventsUpdated;
                }
                else
                {
                    var messageActionTypeId = reader.GetValueOrDefault<int>("MessageActionTypeId");
                    messageActionType = (Constants.MessageActionType)messageActionTypeId;
                }

                if (messageActionType == Constants.MessageActionType.NoMessage)
                {
                    messageActionType = Constants.MessageActionType.StarChefEventsUpdated;
                }
                var message = new CalculateUpdateMessage(entityId, userDatabase.ConnectionString, (int)messageActionType, userDatabase.DatabaseId, entityTypeId, id, retryCount, statusId)

                { ArrivedTime = dateCreaded, ExternalId = userDatabase.ExternalId };
                message.ExternalId = externalId;

                if (!messages.Any(c => c.EntityTypeId == entityTypeId && c.ProductID == entityId))
                {
                    messages.Add(message);
                }
            }

            return messages;
        }


        public void Process(Common.Engine.IPriceEngine pEngine, UserDatabase userDatabase, int count)
        {
            Logger.Info($"Process started for db {userDatabase.DatabaseId}");

            var connectionString = $"{userDatabase.ConnectionString}; Connection Timeout=10";

            while (count > 0 && this.CanProcess)
            {
                HashSet<CalculateUpdateMessage> messages = null;
                try
                {
                    messages = GetDatabaseMessages(userDatabase);
                    if (messages.Any())
                    {
                        var grouppedMessages = messages.GroupBy(c => c.EntityTypeId);
                        foreach (var groupedMessage in grouppedMessages)
                        {
                            var entityTypeId = groupedMessage.Key;
                            var entityMessages = groupedMessage.ToList();

                            var isPublishEnabled = userDatabase.OrchestrationLookups.FirstOrDefault(c => c.EntityTypeId == entityTypeId);
                            if ((isPublishEnabled != null && isPublishEnabled.CanPublish) || groupedMessage.Key == 0)
                            {
                                EnumHelper.EntityTypeWrapper? entityTypeWrapper = null;

                                switch (entityTypeId)
                                {
                                    case (int)Constants.EntityType.UserGroup:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.UserGroup;
                                        break;
                                    case (int)Constants.EntityType.Ingredient:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.Ingredient;
                                        break;
                                    case (int)Constants.EntityType.Dish:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.Recipe;
                                        break;
                                    case (int)Constants.EntityType.Menu:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.Menu;
                                        break;
                                    case (int)Constants.EntityType.MealPeriodManagement:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.MealPeriod;
                                        break;
                                    case (int)Constants.EntityType.Group:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.Group;
                                        break;
                                    case (int)Constants.EntityType.Supplier:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.SendSupplierUpdatedEvent;
                                        break;
                                    case (int)Constants.EntityType.ProductSet:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.ProductSet;
                                        break;
                                    case (int)Constants.EntityType.User:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.User;
                                        break;
                                }

                                foreach (var entityMessage in entityMessages)
                                {
                                    //create different messages for different MessageActionType
                                    switch ((Constants.MessageActionType)entityMessage.Action)
                                    {
                                        case Constants.MessageActionType.UserDeActivated:
                                            Console.WriteLine($"Publish command message start Entity ID {entityMessage.ProductID} {entityMessage.EntityTypeId} DataBase {userDatabase.DatabaseId}");
                                            SendCommand(entityMessage, userDatabase);
                                            Console.WriteLine("Message was sent");
                                            break;
                                        case Constants.MessageActionType.EntityDeleted:
                                            Console.WriteLine($"Publish delete message start Entity ID {entityMessage.ProductID} {entityMessage.EntityTypeId} DataBase {userDatabase.DatabaseId}");
                                            SendDeleteCommand(entityMessage, userDatabase);
                                            Console.WriteLine("Message was sent");
                                            break;
                                        //case Constants.MessageActionType.EntityUpdated:
                                        //    _messageSender.PublishUpdateEvent(entityMessage);
                                        //    break;
                                        case Constants.MessageActionType.StarChefEventsUpdated:
                                            if (entityTypeWrapper.HasValue)
                                            {
                                                Console.WriteLine($"Message sending start Entity ID {entityMessage.ProductID} {entityMessage.EntityTypeId} DataBase {userDatabase.DatabaseId}");
                                                Send(entityTypeWrapper.Value, connectionString, entityMessage, userDatabase.DatabaseId);
                                                Console.WriteLine("Message was sent");
                                            }
                                            else
                                            {
                                                Enqueue(connectionString, entityMessage.ProductID, entityMessage.EntityTypeId, entityMessage.StatusId, entityMessage.RetryCount, entityMessage.ArrivedTime, userDatabase.DatabaseId, entityMessage.ExternalId, entityMessage.Action);
                                                Logger.Error($"Not supported entity type: DatabaseId {entityMessage.DatabaseID} : EntityId {entityMessage.ProductID} : EntityTypeId {entityMessage.EntityTypeId}");
                                            }
                                            break;
                                        case Constants.MessageActionType.UserCreated:
                                        case Constants.MessageActionType.UserActivated:
                                        case Constants.MessageActionType.SalesForceUserCreated:
                                            if ((Constants.EntityType)entityTypeId == Constants.EntityType.User)
                                            {
                                                //set specific status for object type user
                                                switch ((Constants.MessageActionType)entityMessage.Action)
                                                {
                                                    case Constants.MessageActionType.UserActivated:
                                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.UserActivated;
                                                        break;
                                                    case Constants.MessageActionType.SalesForceUserCreated:
                                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.SendUserUpdatedEvent;
                                                        break;
                                                }
                                            }
                                            else {
                                                entityTypeWrapper = null;
                                            }

                                            if (entityTypeWrapper.HasValue)
                                            {
                                                Console.WriteLine($"Message sending start Entity ID {entityMessage.ProductID} {entityMessage.EntityTypeId} DataBase {userDatabase.DatabaseId}");
                                                Send(entityTypeWrapper.Value, connectionString, entityMessage, userDatabase.DatabaseId);
                                                Console.WriteLine("Message was sent");
                                            }
                                            else
                                            {
                                                Enqueue(connectionString, entityMessage.ProductID, entityMessage.EntityTypeId, entityMessage.StatusId, entityMessage.RetryCount, entityMessage.ArrivedTime, userDatabase.DatabaseId, entityMessage.ExternalId, entityMessage.Action);
                                                Logger.Error($"Not supported entity type: DatabaseId {entityMessage.DatabaseID} : EntityId {entityMessage.ProductID} : EntityTypeId {entityMessage.EntityTypeId}");
                                            }
                                            break;
                                        case Constants.MessageActionType.UpdatedProductCost:
                                            var enabled = pEngine.IsEngineEnabled().Result;
                                            Logger.Info($"Processing UpdatedProductCost DataBase {userDatabase.DatabaseId}");
                                            if (enabled)
                                            {
                                                IEnumerable<Common.Model.DbPrice> result;
                                                if (entityMessage.ProductID > 0)
                                                {
                                                    Logger.Info($"Run product price recalculation DataBase {userDatabase.DatabaseId}");
                                                    result = pEngine.Recalculation(entityMessage.ProductID, true, DateTime.UtcNow).Result;
                                                }
                                                else
                                                {
                                                    Logger.Info($"Run global price recalculation DataBase {userDatabase.DatabaseId}");
                                                    result = pEngine.GlobalRecalculation(true, DateTime.UtcNow).Result;
                                                }

                                                Logger.Info($"Generated {result.Count()} prices");
                                            }
                                            else {
                                                Enqueue(connectionString, entityMessage.ProductID, entityMessage.EntityTypeId, entityMessage.StatusId, entityMessage.RetryCount, entityMessage.ArrivedTime, userDatabase.DatabaseId, entityMessage.ExternalId, entityMessage.Action);
                                                Logger.Error($"Global price recalculation is not switch on DatabaseId {entityMessage.DatabaseID}");
                                            }
                                            break;
                                        default:
                                            if ((Constants.EntityType)entityTypeId == Constants.EntityType.User)
                                            {
                                                entityTypeWrapper = EnumHelper.EntityTypeWrapper.SendUserUpdatedEventAndCommand;
                                                Console.WriteLine($"Message sending start Entity ID {entityMessage.ProductID} {entityMessage.EntityTypeId} DataBase {userDatabase.DatabaseId}");
                                                Send(entityTypeWrapper.Value, connectionString, entityMessage, userDatabase.DatabaseId);
                                                Console.WriteLine("Message was sent");
                                            }
                                            else
                                            {
                                                Enqueue(connectionString, entityMessage.ProductID, entityMessage.EntityTypeId, entityMessage.StatusId, entityMessage.RetryCount, entityMessage.ArrivedTime, userDatabase.DatabaseId, entityMessage.ExternalId, entityMessage.Action);
                                                Logger.Error($"Not supported entity type: DatabaseId {entityMessage.DatabaseID} : EntityId {entityMessage.ProductID} : EntityTypeId {entityMessage.EntityTypeId}");
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        count = 0;
                    }

                    count = count - messages.Count();
                    Console.WriteLine($"Pending count {count} Database ID {userDatabase.DatabaseId}");
                }
                catch (Exception e)
                {
                    //Put back to queue
                    if (messages != null && messages.Any())
                    {
                        Parallel.ForEach(messages, m => Enqueue(userDatabase.ConnectionString, m.ProductID, m.EntityTypeId, m.StatusId, m.RetryCount, m.ArrivedTime, userDatabase.DatabaseId, m.ExternalId, m.Action));
                    }
                    var message = $"Database ID: { userDatabase.DatabaseId }";
                    Logger.Error(message, e);
                }
            }
            Logger.Info($"Process finished for db {userDatabase.DatabaseId}");
        }

        public virtual void Send(EnumHelper.EntityTypeWrapper? entityTypeWrapper, string connectionString, CalculateUpdateMessage message, int databaseId)
        {
            var result = _messageSender.Send(entityTypeWrapper.Value, connectionString, message.EntityTypeId, message.ProductID, message.ExternalId, message.DatabaseID, message.ArrivedTime);
            if (!result)
            {
                Enqueue(connectionString, message.ProductID, message.EntityTypeId, message.StatusId, message.RetryCount, message.ArrivedTime, databaseId, message.ExternalId, message.Action);
                Logger.Error($"Processing failed: DatabaseId {message.DatabaseID} : EntityId {message.ProductID} : EntityTypeId {message.EntityTypeId}");
            }
        }

        public virtual void SendCommand(CalculateUpdateMessage message, UserDatabase database) {
            var result = _messageSender.PublishCommand(message);
            if (!result)
            {
                Enqueue(database.ConnectionString, message.ProductID, message.EntityTypeId, message.StatusId, message.RetryCount, message.ArrivedTime, database.DatabaseId, message.ExternalId, message.Action);
                Logger.Error($"Processing failed: DatabaseId {message.DatabaseID} : EntityId {message.ProductID} : EntityTypeId {message.EntityTypeId}");
            }
        }

        public virtual void SendDeleteCommand(CalculateUpdateMessage message, UserDatabase database)
        {
            var result = _messageSender.PublishDeleteEvent(message);
            if (!result)
            {
                Enqueue(database.ConnectionString, message.ProductID, message.EntityTypeId, message.StatusId, message.RetryCount, message.ArrivedTime, database.DatabaseId, message.ExternalId, message.Action);
                Logger.Error($"Processing failed: DatabaseId {message.DatabaseID} : EntityId {message.ProductID} : EntityTypeId {message.EntityTypeId}");
            }
        }

        public virtual void Enqueue(string connectionString, int entityId, int entityTypeId, int statusId, int retryCount, DateTime dateCreated, int databaseId, string externalId,int messageActionTypeId)
        {
            try
            {
                var retryCountUpdated = retryCount + 1;
                var statusIdUpdated = retryCountUpdated >= _appConfiguration.RetryCount ? 3 : statusId;

                _databaseManager.Execute(connectionString,
                    "sc_calculation_enqueue",
                    new SqlParameter("@EntityId", entityId),
                    new SqlParameter("@EntityTypeId", entityTypeId),
                    new SqlParameter("@RetryCount", retryCountUpdated),
                    new SqlParameter("@StatusId", statusIdUpdated),
                    new SqlParameter("@DateCreated", dateCreated),
                    new SqlParameter("@ExternalId", externalId),
                    new SqlParameter("@MessageActionTypeId", messageActionTypeId));
            }
            catch (Exception e)
            {
                Logger.Error($"Saving failed: DatabaseId {databaseId} : EntityId {entityId} : EntityTypeId {entityTypeId}");
                Logger.Error(e);
            }
        }

        public bool CanProcess { get; set; }
        public List<Thread> ActiveThreads { get; private set; }
    }
}
