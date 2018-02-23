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
        }

        public async Task<bool> ExecuteAsync()
        {
            while (CanProcess)
            {
                try
                {
                    this._userDatabases = _databaseManager.GetUserDatabases(this._appConfiguration.UserDSN);
                    foreach (var userDatabase in _userDatabases)
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

                    Parallel.ForEach(this._userDatabases, Process);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    Thread.Sleep(60000);
                }
            }

            return await Task.FromResult<bool>(true);
        }

        public void Process(UserDatabase userDatabase)
        {
            Logger.Info($"Process started for db {userDatabase.DatabaseId}");

            var count = 1;
            while (count > 0)
            {
                HashSet<CalculateUpdateMessage> messages = null;
                try
                {
                    messages = new HashSet<CalculateUpdateMessage>();
                    var sqlParam = new SqlParameter("@count", System.Data.SqlDbType.Int) { Value = _appConfiguration.MessagesCount };
                    var reader = _databaseManager.ExecuteReaderMultiResultset(userDatabase.ConnectionString, "sc_calculation_dequeue", sqlParam);

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

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            count = reader.GetValue<int>("Count");
                        }
                    }
                    else
                    {
                        count = 0;
                    }

                    if (messages.Any())
                    {
                        Send(userDatabase, messages);
                    }
                }
                catch (Exception e)
                {
                    //Put back to queue
                    if (messages != null && messages.Any())
                    {
                        Parallel.ForEach(messages, m => Enqueue(userDatabase.ConnectionString, m.ProductID, m.EntityTypeId, m.StatusId, m.RetryCount, m.ArrivedTime));
                    }
                    Logger.Error(e);
                }
            }

            Logger.Info($"Process finished for db {userDatabase.DatabaseId}");
        }

        public void Send(UserDatabase userDatabase, HashSet<CalculateUpdateMessage> messages)
        {
            var grouppedMessages = messages.GroupBy(c => c.EntityTypeId);
            foreach (var groupedMessage in grouppedMessages)
            {
                var isPublishEnabled = userDatabase.OrchestrationLookups.FirstOrDefault(c => c.EntityTypeId == groupedMessage.Key);
                if (isPublishEnabled != null && isPublishEnabled.CanPublish)
                {
                    foreach (var message in groupedMessage)
                    {
                        EnumHelper.EntityTypeWrapper? entityTypeWrapper = null;

                        switch (message.EntityTypeId)
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
                        var result = _messageSender.Send(entityTypeWrapper.Value, message.DSN, message.EntityTypeId, message.ProductID, message.ExternalId, message.DatabaseID, message.ArrivedTime);
                        if (!result)
                        {
                            Enqueue(userDatabase.ConnectionString, message.ProductID, message.EntityTypeId, message.StatusId, message.RetryCount, message.ArrivedTime);
                            Logger.Error($"Processing failed: DatabaseId {message.DatabaseID} : EntityId {message.ProductID} : EntityTypeId {message.EntityTypeId}");
                        }
                    }
                }
            }

            Logger.Info($"Process completed for DB {userDatabase.DatabaseId}");
        }

        private void Enqueue(string connectionString, int entityId, int entityTypeId, int statusId, int retryCount, DateTime dateCreated)
        {
            var retryCountUpdated = retryCount + 1;
            var statusIdUpdated = retryCountUpdated == 20 ? 3 : statusId;

            _databaseManager.Execute(connectionString,
                "sc_calculation_enqueue",
                new SqlParameter("@EntityId", entityId),
                new SqlParameter("@EntityTypeId", entityTypeId),
                new SqlParameter("@RetryCount", retryCountUpdated),
                new SqlParameter("@StatusId", statusIdUpdated),
                new SqlParameter("@DateCreated", dateCreated));
        }

        public bool CanProcess { get; set; }
    }
}
