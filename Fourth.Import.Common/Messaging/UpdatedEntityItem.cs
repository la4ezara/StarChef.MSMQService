using Fourth.StarChef.Invariables;

namespace Fourth.Import.Common.Messaging
{
    public class UpdatedEntityItem
    {
        public int EntityId { get; set; }
        public Constants.EntityType EntityTypeId { get; set; }
        public Constants.MessageActionType MessageActionType { get; set; }
    }
}
