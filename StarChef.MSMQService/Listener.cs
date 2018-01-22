using Fourth.StarChef.Invariables;
using log4net;
using StarChef.Common;
using StarChef.Common.Extensions;
using StarChef.Data.Extensions;
using StarChef.MSMQService.Configuration;
using StarChef.Orchestrate;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Messaging;
using System.Net.Mail;
using System.Threading.Tasks;

namespace StarChef.MSMQService
{
    /// <summary>
    /// Summary description for Listener.
    /// </summary>
    public class Listener : IListener
    {
        private readonly IAppConfiguration _appConfiguration;
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IStarChefMessageSender _messageSender;
        private readonly IDatabaseManager _databaseManager;
        private readonly IMessageManager _messageManager;
        private readonly XmlMessageFormatter _messageFormat;

        public event EventHandler<MessageProcessEventArgs> MessageProcessing;
        public event EventHandler<MessageProcessEventArgs> MessageProcessed;
        public event EventHandler<MessageProcessEventArgs> MessageNotProcessing;

        public bool CanProcess { get; set; }

        public bool IsProcessing { get; private set; }

        public Listener(IAppConfiguration appConfiguration, IStarChefMessageSender messageSender, IDatabaseManager databaseManager)
        {
            _appConfiguration = appConfiguration;
            _messageSender = messageSender;
            _databaseManager = databaseManager;
            _messageManager = new MsmqManager(_appConfiguration.NormalQueueName, _appConfiguration.PoisonQueueName);
            _messageFormat = new XmlMessageFormatter(new[] { typeof(UpdateMessage) });
            this.CanProcess = true;
        }

        public Listener(IAppConfiguration appConfiguration, IStarChefMessageSender messageSender, IDatabaseManager databaseManager, IMessageManager messageManager)
        {
            _appConfiguration = appConfiguration;
            _messageSender = messageSender;
            _databaseManager = databaseManager;
            _messageManager = messageManager;
            _messageFormat = new XmlMessageFormatter(new[] { typeof(UpdateMessage) });
            this.CanProcess = true;
        }

        public Task ExecuteAsync(Hashtable activeDatabases, Hashtable globalUpdateTimeStamps)
        {
            Message msg = null;
           UpdateMessage updmsg = null;
            
            try
            {
                TimeSpan timeout = TimeSpan.FromSeconds(10);
                
                while (CanProcess && !IsProcessing)
                {
                    this.IsProcessing = true;
                    msg = _messageManager.mqPeek(timeout);
                    IsProcessing = true;
                    if (msg != null)
                    {
                        var messageId = msg.Id;
                        //Receive message and exclude from queue.
                        //Remove only when data is processed
                        msg = _messageManager.mqReceive(msg.Id, timeout);
                        if (msg != null)
                        {
                            msg.Formatter = _messageFormat;
                            updmsg = (UpdateMessage)msg.Body;

                            if (!activeDatabases.Contains(updmsg.DatabaseID) && updmsg != null)
                            {
                                DateTime arrivalTime = DateTime.UtcNow;
                                if (messageId != "00000000-0000-0000-0000-000000000000\\0")
                                {
                                    arrivalTime = msg.ArrivedTime;
                                }

                                updmsg.ArrivedTime = arrivalTime;
                                int databaseId = updmsg.DatabaseID;
                                ThreadContext.Properties["OrganisationId"] = databaseId;

                                activeDatabases.Add(databaseId, arrivalTime);

                                if (updmsg.Action == (int)Constants.MessageActionType.GlobalUpdate)
                                {
                                    if (globalUpdateTimeStamps.Contains(databaseId))
                                    {
                                        if (TimeSpan.FromMinutes(DateTime.UtcNow.Subtract((DateTime)globalUpdateTimeStamps[databaseId]).Minutes) > TimeSpan.FromMinutes(_appConfiguration.GlobalUpdateWaitTime))
                                        {
                                            globalUpdateTimeStamps[databaseId] = DateTime.UtcNow;
                                        }
                                        else
                                        {
                                            //do not process message - message is skipped and lost
                                            updmsg = null;
                                        }
                                    }
                                    else
                                    {
                                        globalUpdateTimeStamps.Add(databaseId, DateTime.UtcNow);
                                    }
                                }

                                if (updmsg != null)
                                {
                                    OnMessageProcessing(new MessageProcessEventArgs(updmsg, MessageProcessStatus.Processing));
                                    ProcessMessage(updmsg);
                                    OnMessageProcessed(new MessageProcessEventArgs(updmsg, MessageProcessStatus.Success));
                                }

                                activeDatabases.Remove(databaseId);
                            }
                            else
                            {
                                OnMessageNotProcessing(new MessageProcessEventArgs(updmsg, MessageProcessStatus.ParallelDatabaseId));
                            }
                        }
                        else {
                            OnMessageNotProcessing(new MessageProcessEventArgs(MessageProcessStatus.NoMessage));
                        }
                    }
                    else {
                        OnMessageNotProcessing(new MessageProcessEventArgs(MessageProcessStatus.NoMessage));
                    }
                    IsProcessing = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                this.SendPoisonMessage(msg, _messageFormat, _messageManager, activeDatabases);
            }
            finally
            {
                IsProcessing = false;
                _messageManager.mqDisconnect();
                ThreadContext.Properties.Remove("OrganisationId");

                if (updmsg != null) {
                    activeDatabases.Remove(updmsg.DatabaseID);
                }

            }
            return Task.CompletedTask;
        }

        protected virtual void OnMessageProcessing(MessageProcessEventArgs e)
        {
            EventHandler<MessageProcessEventArgs> handler = MessageProcessing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMessageProcessed(MessageProcessEventArgs e)
        {
            EventHandler<MessageProcessEventArgs> handler = MessageProcessed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMessageNotProcessing(MessageProcessEventArgs e)
        {
            EventHandler<MessageProcessEventArgs> handler = MessageNotProcessing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void SendPoisonMessage(Message msg, IMessageFormatter format, IMessageManager mqManager, Hashtable activeDatabases)
        {
            if (msg != null)
            {
                msg.Formatter = format;
                var updmsg = (UpdateMessage)msg.Body;
                if (updmsg != null)
                {
                    _logger.Error(new Exception("StarChef MQ Service: SENDING MESSAGE TO THE POISON QUEUE"));
                    mqManager.mqSendToPoisonQueue(updmsg, msg.Priority);
                    if (_appConfiguration.SendPoisonMessageNotification)
                    {
                        _logger.Error(new Exception("StarChef MQ Service: SENDING POISON MESSAGE TO THE MAIL"));
                        SendPoisonMessageMail(updmsg);
                    }
                }
            }
        }

        private void SendPoisonMessageMail(UpdateMessage message)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(_appConfiguration.FromAddress, _appConfiguration.Alias)
                };
                mail.To.Add(_appConfiguration.ToAddress);
                mail.IsBodyHtml = true;
                mail.Subject = _appConfiguration.Subject;
                mail.Body = message.ToString();
                mail.Priority = MailPriority.High;
                var smtp = new SmtpClient();
                smtp.Send(mail);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
        }

        public void ProcessMessage(UpdateMessage msg)
        {
            if (msg != null)
            {
                _logger.InfoFormat("Received MSMQ message: {0}", msg.ToJson());

                switch (msg.Action)
                {
                    case (int)Constants.MessageActionType.UpdatedUserDefinedUnit:
                        ProcessUduUpdate(msg);
                        break;
                    case (int)Constants.MessageActionType.UpdatedProductSet:
                        ProcessProductSetUpdate(msg);
                        break;
                    case (int)Constants.MessageActionType.UpdatedPriceBand:
                        ProcessPriceBandUpdate(msg);
                        break;
                    case (int)Constants.MessageActionType.UpdatedGroup:
                        ProcessGroupUpdate(msg);
                        break;
                    case (int)Constants.MessageActionType.UpdatedProductCost:
                        ProcessProductCostUpdate(msg);
                        break;
                    case (int)Constants.MessageActionType.GlobalUpdate:
                        ProcessGlobalUpdate(msg);
                        break;
                    case (int)Constants.MessageActionType.UpdatedProductNutrient:
                        ProcessProductNutrientUpdate(msg);
                        break;
                    case (int)Constants.MessageActionType.UpdatedProductIntolerance:
                        ProcessProductIntoleranceUpdate(msg);
                        break;
                    case (int)Constants.MessageActionType.UpdatedProductNutrientInclusive:
                        ProcessProductNutrientInclusiveUpdate(msg);
                        break;
                    case (int)Constants.MessageActionType.GlobalUpdateBudgeted:
                        ProcessGlobalUpdateBudgeted(msg);
                        break;
                    case (int)Constants.MessageActionType.UpdateAlternateIngredients:
                        ProcessAlternateIngredientUpdate(msg);
                        break;
                    // All Events are populating under StarChefEventsUpdated Action - Additional action added for 
                    // User because of multiple different actions
                    case (int)Constants.MessageActionType.StarChefEventsUpdated:
                    // Starchef to Salesforce - later Salesforce notify to Starchef the user created notification
                    case (int)Constants.MessageActionType.UserCreated:
                    case (int)Constants.MessageActionType.UserUpdated:
                    case (int)Constants.MessageActionType.UserActivated:
                    // Once user created in Salesforce, SF will notified and to SC and SC store the external id on DB
                    case (int)Constants.MessageActionType.SalesForceUserCreated:
                        ProcessStarChefEventsUpdated(msg);
                        break;
                    case (int)Constants.MessageActionType.UserDeActivated:
                        _messageSender.PublishCommand(msg);
                        break;
                    case (int)Constants.MessageActionType.EntityDeleted:
                        _messageSender.PublishDeleteEvent(msg);
                        break;
                    case (int)Constants.MessageActionType.EntityUpdated:
                        _messageSender.PublishUpdateEvent(msg);
                        break;
                    case (int)Constants.MessageActionType.EntityImported:
                        PostProcessingPerSubAction(msg);
                        break;
                }
            }
        }

        private void PostProcessingPerSubAction(UpdateMessage msg)
        {
            var importSettings = _databaseManager.GetImportSettings(msg.DSN, msg.DatabaseID);
            switch (msg.SubAction)
            {
                case (int) Constants.MessageSubActionType.ImportedIngredient:

                    #region Ingredient

                {
                    var importTypeSettings = importSettings.Ingredient();
                    if (importTypeSettings.AutoCalculateCost)
                            _databaseManager.Execute(msg.DSN, "sc_calculate_dish_pricing", new SqlParameter("@product_id", msg.ProductID));
                }

                    #endregion

                    break;
                case (int) Constants.MessageSubActionType.ImportedIngredientIntolerance:

                    #region IngredientIntolerance

                {
                    var importTypeSettings = importSettings.IngredientIntolerance();
                    if (importTypeSettings.AutoCalculateIntolerance)
                    {
                        _databaseManager.Execute(msg.DSN, "sc_audit_log_nutrition_intolerance",
                            new SqlParameter("@product_id", msg.ProductID),
                            new SqlParameter("@db_entity_id", 20), // hardcoded
                            new SqlParameter("@user_id", 1), // hardcoded
                            new SqlParameter("@audit_type_id", 6) // hardcoded
                            );

                        _databaseManager.Execute(msg.DSN, "utils_recalc_parent_intol_by_product",
                            new SqlParameter("@ProductID", msg.ProductID));

                        _databaseManager.Execute(msg.DSN, "sc_batch_product_labelling_update",
                            new SqlParameter("@product_id", msg.ProductID),
                            new SqlParameter("@disable_msmq_log", 1) // hardcoded
                            );
                    }
                }

                    #endregion

                    break;
                case (int) Constants.MessageSubActionType.ImportedIngredientNutrient:

                    #region IngredientNutrient

                {
                    var importTypeSettings = importSettings.IngredientNutrient();
                    if (importTypeSettings.AutoCalculateIntolerance)
                    {

                        _databaseManager.Execute(msg.DSN, "sc_audit_log_nutrition_intolerance",
                            new SqlParameter("@product_id", msg.ProductID),
                            new SqlParameter("@db_entity_id", 20), // hardcoded
                            new SqlParameter("@user_id", 1), // hardcoded
                            new SqlParameter("@audit_type_id", 6) // hardcoded
                            );

                        // first try to recalculate fibre if need
                        string fiberFlag;
                        var properties = msg.ExtendedProperties.Pairs();
                        if (properties.TryGetValue("FIBER_RECALC_REQUIRED", out fiberFlag))
                        {
                            if (Convert.ToBoolean(fiberFlag))
                            {
                                _databaseManager.Execute(msg.DSN, "_sc_recalculate_nutrient_fibre",
                                    new SqlParameter("@product_id", msg.ProductID));
                            }
                        }

                        //calculate summary for ingredient
                        _databaseManager.Execute(msg.DSN, "_sc_update_calorie_details",
                            new SqlParameter("@product_id", msg.ProductID));

                        //calculate nutrition data for related recipes
                        _databaseManager.Execute(msg.DSN, "_sc_update_dish_yield",
                            new SqlParameter("@product_id", msg.ProductID),
                            new SqlParameter("@include_self", false) // hardcoded
                            );
                    }
                }

                    #endregion

                    break;
                case (int) Constants.MessageSubActionType.ImportedIngredientPriceBand:

                    #region IngredientPriceBand

                {
                    var importTypeSettings = importSettings.IngredientPriceBand();
                    if (importTypeSettings.AutoCalculateCost)
                    {
                        string priceBands;
                        var properties = msg.ExtendedProperties.Pairs();
                        if (properties.TryGetValue("PRICE_BANDS", out priceBands))
                        {
                            var priceBandsArray = priceBands.Split(',').Cast<int>().ToArray();
                            for (var i = 0; i < priceBandsArray.Count(); i++)
                            {
                                _databaseManager.Execute(msg.DSN, "_sc_recalculate_nutrient_fibre",
                                    new SqlParameter("@product_id", msg.ProductID),
                                    new SqlParameter("@pband_id", priceBandsArray[i])
                                    );
                            }
                        }
                    }
                }

                    #endregion

                    break;
                case (int) Constants.MessageSubActionType.ImportedIngredientConversion:

                    #region IngredientConversion

                {
                    _databaseManager.Execute(msg.DSN, "sc_audit_history_single_log",
                        new SqlParameter("@entity_id", msg.ProductID),
                        new SqlParameter("@modified_columns", "Pack Size"),
                        new SqlParameter("@db_entity_id", 20)); // hardcoded
                }

                    #endregion

                    break;
                case (int) Constants.MessageSubActionType.ImportedUsers:
                case (int) Constants.MessageSubActionType.ImportedIngredientCategory:
                default:
                    // do nothing
                    break;
            }
        }

        /// <summary>
        /// Resend message to use another processing algorithm
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="sendUpdateMessageFor">Array of message types which should be forwarded</param>
        //private static void ForwardUpdateMessage(UpdateMessage msg, int[] sendUpdateMessageFor = null)
        //{
        //    sendUpdateMessageFor = sendUpdateMessageFor ?? new[]
        //    {
        //        (int) Constants.MessageSubActionType.ImportedIngredient,
        //        (int) Constants.MessageSubActionType.ImportedIngredientCategory,
        //        (int) Constants.MessageSubActionType.ImportedIngredientIntolerance,
        //        (int) Constants.MessageSubActionType.ImportedIngredientNutrient,
        //    };
        //    if (sendUpdateMessageFor.Contains(msg.SubAction))
        //    {
        //        var forwardedMessage = new UpdateMessage(
        //            msg.ProductID,
        //            entityTypeId: msg.EntityTypeId,
        //            action: (int) Constants.MessageActionType.SalesForceUserCreated,
        //            dbDsn: msg.DSN,
        //            databaseId: msg.DatabaseID);

        //        var queueName = ConfigurationManager.AppSettings["StarChef.MSMQ.Queue"];
        //        MSMQHelper.Send(forwardedMessage, queueName);
        //    }
        //}

        private void ProcessUduUpdate(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_calculate_dish_pricing",
                new SqlParameter("@group_id", 0),
                new SqlParameter("@product_id", 0),
                new SqlParameter("@pset_id", 0),
                new SqlParameter("@pband_id", 0),
                new SqlParameter("@unit_id", msg.ProductID),
                new SqlParameter("@message_arrived_time", msg.ArrivedTime));
        }

        private void ProcessProductSetUpdate(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_calculate_dish_pricing",
                new SqlParameter("@group_id", 0),
                new SqlParameter("@product_id", 0),
                new SqlParameter("@pset_id", msg.ProductID),
                new SqlParameter("@pband_id", 0),
                new SqlParameter("@unit_id", 0),
                new SqlParameter("@message_arrived_time", msg.ArrivedTime));
        }

        private void ProcessPriceBandUpdate(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_calculate_dish_pricing",
                new SqlParameter("@group_id", 0),
                new SqlParameter("@product_id", 0),
                new SqlParameter("@pset_id", 0),
                new SqlParameter("@pband_id", msg.ProductID),
                new SqlParameter("@unit_id", 0),
                new SqlParameter("@message_arrived_time", msg.ArrivedTime));
        }

        private void ProcessGroupUpdate(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_calculate_dish_pricing",
                new SqlParameter("@group_id", msg.GroupID),
                new SqlParameter("@product_id", 0),
                new SqlParameter("@pset_id", 0),
                new SqlParameter("@pband_id", 0),
                new SqlParameter("@unit_id", 0),
                new SqlParameter("@message_arrived_time", msg.ArrivedTime));
        }

        private void ProcessProductCostUpdate(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_calculate_dish_pricing",
                new SqlParameter("@group_id", 0),
                new SqlParameter("@product_id", msg.ProductID),
                new SqlParameter("@pset_id", 0),
                new SqlParameter("@pband_id", 0),
                new SqlParameter("@unit_id", 0),
                new SqlParameter("@message_arrived_time", msg.ArrivedTime));
        }

        private void ProcessProductNutrientUpdate(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_batch_product_nutrition_update",
                new SqlParameter("@product_id", msg.ProductID));
        }

        private void ProcessProductNutrientInclusiveUpdate(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_batch_product_nutrition_update_inc",
                new SqlParameter("@product_id", msg.ProductID));
        }

        private void ProcessProductIntoleranceUpdate(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_batch_product_labelling_update",
                new SqlParameter("@product_id", msg.ProductID));
        }

        private void ProcessGlobalUpdate(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_calculate_dish_pricing",
                new SqlParameter("@group_id", msg.GroupID),
                new SqlParameter("@product_id", 0),
                new SqlParameter("@pset_id", 0),
                new SqlParameter("@pband_id", 0),
                new SqlParameter("@unit_id", 0),
                new SqlParameter("@message_arrived_time", msg.ArrivedTime));
        }


        private void ProcessGlobalUpdateBudgeted(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_calculate_dish_pricing_budgeted");
        }

        private void ProcessAlternateIngredientUpdate(UpdateMessage msg)
        {
            ExecuteStoredProc(msg.DSN,
                "sc_alternate_ingredient_update",
                new SqlParameter("@product_id", msg.ProductID));
        }

        private void ProcessStarChefEventsUpdated(UpdateMessage msg)
        {
            var entityTypeId = 0;
            var entityId = 0;
            var arrivedTime = msg.ArrivedTime;

            EnumHelper.EntityTypeWrapper? entityTypeWrapper = null;
            switch (msg.EntityTypeId)
            {
                case (int) Constants.EntityType.User:
                    entityTypeId = (int) Constants.EntityType.User;
                    entityId = msg.ProductID;
                    switch (msg.Action)
                    {
                        case (int) Constants.MessageActionType.UserCreated:
                        case (int) Constants.MessageActionType.StarChefEventsUpdated:
                            entityTypeWrapper = EnumHelper.EntityTypeWrapper.User;
                            break;
                        case (int) Constants.MessageActionType.UserActivated:
                            entityTypeWrapper = EnumHelper.EntityTypeWrapper.UserActivated;
                            break;
                        case (int) Constants.MessageActionType.UserDeActivated:
                            entityTypeWrapper = EnumHelper.EntityTypeWrapper.UserDeactivated;
                            break;
                        case (int)Constants.MessageActionType.SalesForceUserCreated:
                            entityTypeWrapper = EnumHelper.EntityTypeWrapper.SendUserUpdatedEvent;
                            break;
                        default:
                            entityTypeWrapper = EnumHelper.EntityTypeWrapper.SendUserUpdatedEventAndCommand;
                            break;
                    }
                    break;
                case (int)Constants.EntityType.UserGroup:
                    entityTypeId = (int)Constants.EntityType.UserGroup;
                    entityId = msg.ProductID;
                    entityTypeWrapper = EnumHelper.EntityTypeWrapper.UserGroup;
                    break;
                case (int) Constants.EntityType.Ingredient:
                    entityTypeId = (int) Constants.EntityType.Ingredient;
                    entityId = msg.ProductID;
                    entityTypeWrapper = EnumHelper.EntityTypeWrapper.Ingredient;
                    break;
                case (int) Constants.EntityType.Dish:
                    entityTypeId = (int) Constants.EntityType.Dish;
                    entityTypeWrapper = EnumHelper.EntityTypeWrapper.Recipe;
                    entityId = msg.ProductID;
                    break;
                case (int) Constants.EntityType.Menu:
                    entityTypeId = (int) Constants.EntityType.Menu;
                    entityTypeWrapper = EnumHelper.EntityTypeWrapper.Menu;
                    entityId = msg.ProductID;
                    break;
                case (int) Constants.EntityType.MealPeriodManagement:
                    entityTypeId = (int) Constants.EntityType.MealPeriodManagement;
                    entityTypeWrapper = EnumHelper.EntityTypeWrapper.MealPeriod;
                    entityId = msg.ProductID;
                    break;
                case (int)Constants.EntityType.Group:
                    entityTypeId = (int)Constants.EntityType.Group;
                    entityTypeWrapper = EnumHelper.EntityTypeWrapper.Group;
                    entityId = msg.ProductID;
                    break;
                case (int)Constants.EntityType.Supplier:
                    entityTypeId = (int)Constants.EntityType.Supplier;
                    entityTypeWrapper = EnumHelper.EntityTypeWrapper.SendSupplierUpdatedEvent;
                    entityId = msg.ProductID;
                    break;
                case (int)Constants.EntityType.ProductSet:
                    entityTypeId = (int)Constants.EntityType.ProductSet;
                    entityTypeWrapper = EnumHelper.EntityTypeWrapper.ProductSet;
                    entityId = msg.ProductID;
                    break;
            }

            if (entityTypeWrapper.HasValue)
            {
                _messageSender.Send(entityTypeWrapper.Value,
                                    msg.DSN,
                                    entityTypeId,
                                    entityId,
                                    msg.ExternalId,
                                    msg.DatabaseID,
                                    arrivedTime);
            }
        }

        private int ExecuteStoredProc(string connectionString, string spName, params SqlParameter[] parameterValues)
        {
            var result = _databaseManager.Execute(connectionString, spName, Constants.TIMEOUT_MSMQ_EXEC_STOREDPROC, parameterValues);
            return result;
        }
    }
}