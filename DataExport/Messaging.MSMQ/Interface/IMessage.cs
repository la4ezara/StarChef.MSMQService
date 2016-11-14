using System;

namespace Messaging.MSMQ.Interface
{
    public interface IMessage
    {
        int Action { get; set; }
        DateTime ArrivedTime { get; set; }
        int DatabaseID { get; set; }
        string DSN { get; set; }
        int EntityTypeId { get; set; }
        int GroupID { get; set; }
        int ProductID { get; set; }
        int SubAction { get; set; }
        int UGroupId { get; set; }
        Guid UniqueIdent { get; set; }
        int UserId { get; set; }
        string ToString();
    }
}
