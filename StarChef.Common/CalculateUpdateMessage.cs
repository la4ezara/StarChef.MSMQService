

namespace StarChef.Common
{
    using System;
    using Fourth.StarChef.Invariables;

    [Serializable]
    public class CalculateUpdateMessage : UpdateMessage
    {
        public int Id { get; set; }
        public bool Processed { get; set; }
        public int RetryCount { get; set; }
        public int StatusId { get; set; }

        public CalculateUpdateMessage(int productId, string dbDsn, int action, int databaseId, int entityTypeId, int id, int retryCount, int statusId) : base(productId, dbDsn, action, databaseId, entityTypeId)
        {
            this.Id = id;
            this.RetryCount = retryCount;
            this.StatusId = statusId;
        }
    }
}
