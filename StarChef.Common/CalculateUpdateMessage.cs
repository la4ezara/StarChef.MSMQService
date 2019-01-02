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
        public OrchestrationQueueStatus Status { get; set; }

        public CalculateUpdateMessage(int productId, string dbDsn, int action, int databaseId, int entityTypeId, int id, int retryCount, OrchestrationQueueStatus status) : base(productId, dbDsn, action, databaseId, entityTypeId)
        {
            this.Id = id;
            this.RetryCount = retryCount;
            this.Status = status;
        }
    }
}
