using System;
using System.Messaging;
using System.Data;
using System.Data.SqlClient;
using StarChef.Data;
using System.Diagnostics;
using System.Threading;
using System.Configuration;
using System.Net.Mail;
using StarChef.Orchestrate;
using Autofac;
using log4net;
using log4net.Config;
using StarChef.Common;
using StarChef.MSMQService.Configuration;
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

        public Listener(IAppConfiguration appConfiguration, IStarChefMessageSender messageSender)
        {
            _appConfiguration = appConfiguration;
            _messageSender = messageSender;
        }

        public Task ExecuteAsync()
        {
            Message msg = null;
            MSMQManager mqm = new MSMQManager {MQName = _appConfiguration .QueuePath};
            UpdateMessage updmsg = null;
            UpdateMessage u = new UpdateMessage();
            XmlMessageFormatter format = new XmlMessageFormatter(new Type[] { u.GetType() });
            try
            {
                using (Cursor cursor = mqm.mqCreateCursor())
                {
                    msg = mqm.mqPeek(cursor, PeekAction.Current);
                    if (msg != null)
                    {
                        msg.Formatter = format;
                        string messageId = msg.Id;
                        updmsg = (UpdateMessage) msg.Body;

                        if (updmsg != null)
                        {
                            ThreadContext.Properties["OrganisationId"] = updmsg.DatabaseID;
                        }

                        step1:
                        if (!ListenerService.ActiveTaskDatabaseIDs.Contains(updmsg.DatabaseID))
                        {
                            if (updmsg.Action == (int) Constants.MessageActionType.GlobalUpdate && ListenerService.GlobalUpdateTimeStamps.Contains(updmsg.DatabaseID))
                            {
                                if (TimeSpan.FromMinutes(DateTime.Now.Subtract((DateTime) ListenerService.GlobalUpdateTimeStamps[updmsg.DatabaseID]).Minutes) > TimeSpan.FromMinutes(Double.Parse(ConfigurationSettings.AppSettings.Get("GlobalUpdateWaitTime"))))
                                {
                                    ListenerService.GlobalUpdateTimeStamps[updmsg.DatabaseID] = DateTime.Now;
                                    ListenerService.ActiveTaskDatabaseIDs[updmsg.DatabaseID] = msg.ArrivedTime;
                                    msg = mqm.mqReceive(messageId);
                                }
                                else
                                {
                                    msg = null;
                                    msg = mqm.mqPeek(cursor, PeekAction.Next);
                                    if (msg != null)
                                    {
                                        msg.Formatter = format;
                                        messageId = msg.Id;
                                        updmsg = (UpdateMessage) msg.Body;
                                        goto step1;
                                    }
                                }
                            }
                            else
                            {
                                if (updmsg.Action == (int) Constants.MessageActionType.GlobalUpdate)
                                {
                                    ListenerService.GlobalUpdateTimeStamps[updmsg.DatabaseID] = DateTime.Now;
                                }
                                ListenerService.ActiveTaskDatabaseIDs[updmsg.DatabaseID] = msg.ArrivedTime;
                                msg = mqm.mqReceive(messageId);
                            }

                            if (msg != null)
                            {
                                // Added to handle messages from the Web Service (which have the AppSpecific property set
                                // MTB - 2005-07-28
                                if (msg.AppSpecific == 99)
                                {
                                    WebServMsgHandler.HandleWebServiceQueryMessage(msg);
                                }
                                else if (msg.AppSpecific == 98)
                                {
                                    WebServMsgHandler.HandleWebServiceCostUpdateMessage(msg);
                                }
                                else if (msg.AppSpecific == 89)
                                {
                                    //ReportingMsgHandler.HandleReportingMessage(msg);
                                }
                                else
                                {
                                    msg.Formatter = format;
                                    updmsg = (UpdateMessage) msg.Body;
                                    updmsg.ArrivedTime = msg.ArrivedTime;
                                    ProcessMessage(updmsg);
                                    ListenerService.ActiveTaskDatabaseIDs.Remove(updmsg.DatabaseID);
                                }
                            }
                        }
                        else
                        {
                            msg = null;
                            msg = mqm.mqPeek(cursor, PeekAction.Next);
                            if (msg != null)
                            {
                                msg.Formatter = format;
                                messageId = msg.Id;
                                updmsg = (UpdateMessage) msg.Body;
                                goto step1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                    
                if (msg != null)
                {
                    msg.Formatter = format;
                    updmsg = (UpdateMessage)msg.Body;
                    SendMail(updmsg);
                    _logger.Error(new Exception("StarChef MQ Service: SENDING MESSAGE TO THE POISON QUEUE"));
                    mqm.mqSendToPoisonQueue(updmsg, msg.Priority);
                    ListenerService.ActiveTaskDatabaseIDs.Remove(updmsg.DatabaseID);
                }
            }
            finally
            {
                mqm.mqDisconnect();
                ThreadContext.Properties.Remove("OrganisationId");
            }
            return Task.CompletedTask;
        }

        private void SendMail(UpdateMessage message)
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

        private void ProcessMessage(UpdateMessage msg)
        {
            if (msg != null)
            {
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
                        // Construct event and notify subscribers about deletion of an entity
                        _messageSender.PublishDeleteEvent(msg);
                        break;
                    case (int)Constants.MessageActionType.EntityUpdated:
                        // Construct event and notify subscribers about deletion of an entity
                        _messageSender.PublishUpdateEvent(msg);
                        break;
                }
            }
        }

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
            //create & open a SqlConnection, and dispose of it after we are done.
            using (var cn = new SqlConnection(connectionString))
            {
                cn.Open();

                // need a command with sensible timeout value (10 minutes), as some 
                // of these procs may take several minutes to complete
                var cmd = new SqlCommand(spName, cn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = Constants.TIMEOUT_MSMQ_EXEC_STOREDPROC
                };
                //600

                // add params
                if (parameterValues != null)
                    foreach (var param in parameterValues)
                        cmd.Parameters.Add(param);

                // run proc
                var retval = cmd.ExecuteNonQuery();
                return retval;
            }
        }
	}
}