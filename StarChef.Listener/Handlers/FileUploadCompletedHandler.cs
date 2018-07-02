namespace StarChef.Listener.Handlers
{
    using AutoMapper;
    using Fourth.Import.Process;
    using Fourth.Orchestration.Messaging;
    using log4net;
    using StarChef.Listener.Commands;
    using StarChef.Listener.Exceptions;
    using StarChef.Listener.Extensions;
    using StarChef.Orchestrate.Models.TransferObjects;
    using System.Reflection;
    using System.Threading.Tasks;
    using static Fourth.Orchestration.Model.StarChef.Events;

    public class FileUploadCompletedHandler : ListenerEventHandler, IMessageHandler<FileUploadCompleted>
    {
        private readonly ILog _logger;
        private ImportFileService importService = new ImportFileService();
        public FileUploadCompletedHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
               
        public async Task<MessageHandlerResult> HandleAsync(FileUploadCompleted payload, string trackingId)
        {
            ThreadContext.Properties[INTERNAL_ID] = payload.InternalCustomerId;
            try
            {
                if (Validator.IsAllowedEvent(payload))
                {
                    _logger.EventReceived(trackingId, payload);

                    if (Validator.IsValidPayload(payload))
                    {
                        FileUploadCompletedTransferObject fileUpload = null;
                        try
                        {
                            fileUpload = Mapper.Map<FileUploadCompletedTransferObject>(payload);
                            importService.ImportFileUploaded(fileUpload.FilePath, fileUpload.SourceLogin.ToString());
                                                       
                            await MessagingLogger.MessageProcessedSuccessfully(payload, trackingId);
                            _logger.Processed(trackingId, payload);
                        }
                        catch (ListenerException ex)
                        {
                            _logger.ListenerException(ex, trackingId, fileUpload);
                            return MessageHandlerResult.Fatal;
                        }
                    }
                    else
                    {
                        var errors = Validator.GetErrors();
                        _logger.InvalidModel(trackingId, payload, errors);
                        await MessagingLogger.ReceivedInvalidModel(trackingId, payload, errors);
                        return MessageHandlerResult.Fatal;
                    }
                }
            }
            catch (System.Exception e)
            {
                _logger.Error(e);
                return MessageHandlerResult.Fatal;
            }
            finally
            {
                ThreadContext.Properties.Remove(INTERNAL_ID);
            }
            return MessageHandlerResult.Success;
        }
    }
}
