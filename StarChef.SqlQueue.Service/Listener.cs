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
            while (CanProcess)
            {
                int dbId = default(int);
                var dbForDeletion = new List<int>();

                try
                {
                    var timeSpan = TimeSpan.FromMinutes(_appConfiguration.SleepMinutes);
                    Thread.Sleep(timeSpan);

                    this._userDatabases = _databaseManager.GetUserDatabases(this._appConfiguration.UserDSN);
                    foreach (var userDatabase in _userDatabases)
                    {
                        try
                        {
                            dbId = userDatabase.DatabaseId;
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

                    if (dbForDeletion.Any())
                    {
                        foreach (var db  in dbForDeletion)
                        {
                            this._userDatabases.RemoveWhere(c => c.DatabaseId == db);
                        }
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

        private void StartProcess(UserDatabase userDatabase)
        {
            var count = _databaseManager.ExecuteScalar(userDatabase.ConnectionString, "sc_calculation_queue_count");
            var threadName = $"ThreadDbId{userDatabase.DatabaseId}";
            if (count > this._appConfiguration.NewThreadMessages)
            {
                if (ActiveThreads.Count <= _appConfiguration.MaxThreadCount)
                {
                    if (this.ActiveThreads.All(t => t.Name != threadName))
                    {
                        var t = new Thread(() => Process(userDatabase, count)) { Name = threadName };
                        t.Start();
                        ActiveThreads.Add(t);
                    }
                }
            }
            else
            {
                Process(userDatabase, count);
            }
        }


        private void Process(UserDatabase userDatabase, int count)
        {


            Logger.Info($"Process started for db {userDatabase.DatabaseId}");

            var connectionString = $"{userDatabase.ConnectionString}; Connection Timeout=10";

            while (count > 0 && this.CanProcess)
            {
                HashSet<CalculateUpdateMessage> messages = null;
                try
                {
                    messages = new HashSet<CalculateUpdateMessage>();
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
                        var message = new CalculateUpdateMessage(entityId, userDatabase.ConnectionString, (int)Constants.MessageActionType.StarChefEventsUpdated, userDatabase.DatabaseId, entityTypeId, id, retryCount, statusId)
                        { ArrivedTime = dateCreaded, ExternalId = userDatabase.ExternalId };

                        if (!messages.Any(c => c.EntityTypeId == entityTypeId && c.ProductID == entityId))
                        {
                            messages.Add(message);
                        }
                    }

                    if (messages.Any())
                    {
                        var grouppedMessages = messages.GroupBy(c => c.EntityTypeId);
                        foreach (var groupedMessage in grouppedMessages)
                        {
                            var entityTypeId = groupedMessage.Key;
                            var entityMessages = groupedMessage.ToList();

                            var isPublishEnabled =
                                userDatabase.OrchestrationLookups.FirstOrDefault(c => c.EntityTypeId == entityTypeId);
                            if (isPublishEnabled != null && isPublishEnabled.CanPublish)
                            {
                                EnumHelper.EntityTypeWrapper? entityTypeWrapper = null;

                                switch (entityTypeId)
                                {
                                    case (int)Constants.EntityType.Ingredient:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.Ingredient;
                                        break;
                                    case (int)Constants.EntityType.Dish:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.Recipe;
                                        break;
                                    case (int)Constants.EntityType.Menu:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.Menu;
                                        break;
                                    case (int)Constants.EntityType.MenuCycle:
                                        entityTypeWrapper = EnumHelper.EntityTypeWrapper.MealPeriod;
                                        break;
                                }

                                foreach (var entityMessage in entityMessages)
                                {
                                    Console.WriteLine(
                                        $"Message sending start Entity ID {entityMessage.ProductID} {entityMessage.EntityTypeId} DataBase {userDatabase.DatabaseId}");
                                    Send(entityTypeWrapper.Value, connectionString, entityMessage,
                                        userDatabase.DatabaseId);
                                    Console.WriteLine("Message was sent");
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
                        Parallel.ForEach(messages, m => Enqueue(userDatabase.ConnectionString, m.ProductID, m.EntityTypeId, m.StatusId, m.RetryCount, m.ArrivedTime, userDatabase.DatabaseId));
                    }
                    var message = $"Database ID: { userDatabase.DatabaseId }";
                    Logger.Error(message, e);
                }
            }
            Logger.Info($"Process finished for db {userDatabase.DatabaseId}");
        }

        public void Send(EnumHelper.EntityTypeWrapper? entityTypeWrapper, string connectionString, CalculateUpdateMessage message, int databaseId)
        {
            var result = _messageSender.Send(entityTypeWrapper.Value, connectionString, message.EntityTypeId, message.ProductID, message.ExternalId, message.DatabaseID, message.ArrivedTime);
            if (!result)
            {
                Enqueue(connectionString, message.ProductID, message.EntityTypeId, message.StatusId, message.RetryCount, message.ArrivedTime, databaseId);
                Logger.Error($"Processing failed: DatabaseId {message.DatabaseID} : EntityId {message.ProductID} : EntityTypeId {message.EntityTypeId}");
            }
        }

        private void Enqueue(string connectionString, int entityId, int entityTypeId, int statusId, int retryCount, DateTime dateCreated, int databaseId)
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
                    new SqlParameter("@DateCreated", dateCreated));
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
