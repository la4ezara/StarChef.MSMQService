using System;
using StarChef.Data;

namespace StarChef.MSMQService
{
    [Serializable]
    public class UpdateMessage
    {
        private DateTime _arrivedTime = DateTime.MinValue;
        private int _databaseId;
        private string _dbDsn = string.Empty;
        private int _entityTypeId;
        private string _externalId = string.Empty;
        private int _groupId;
        private int _messageType;
        private int _productId;
        private int _subMessageType;
        private Guid _uniqueIdent = Guid.Empty;

        public UpdateMessage()
        {
        }

        public UpdateMessage(int productId, string dbDsn, int action, int databaseId)
        {
            _productId = productId;
            _groupId = 0;
            _dbDsn = dbDsn;
            _messageType = action;
            _databaseId = databaseId;
        }

        public UpdateMessage(int productId, string dbDsn, int action, int databaseId, int entityTypeId = 0)
        {
            _productId = productId;
            _groupId = 0;
            _dbDsn = dbDsn;
            _messageType = action;
            _databaseId = databaseId;
            _entityTypeId = entityTypeId;
        }

        public UpdateMessage(int productId, int groupId, string dbDsn, int action, int databaseId)
        {
            _productId = productId;
            _groupId = groupId;
            _dbDsn = dbDsn;
            _messageType = action;
            _databaseId = databaseId;
        }

        public UpdateMessage(int productId, int groupId, string dbDsn, int action, int databaseId, int entityTypeId) : this(productId, groupId, dbDsn, action, databaseId)
        {
            _entityTypeId = entityTypeId;
        }

        public int SubAction
        {
            get { return _subMessageType; }
            set { _subMessageType = value; }
        }

        public int Action
        {
            get { return _messageType; }
            set { _messageType = value; }
        }

        public int ProductID
        {
            get { return _productId; }
            set { _productId = value; }
        }

        public Guid UniqueIdent
        {
            get { return _uniqueIdent; }
            set { _uniqueIdent = value; }
        }

        public int GroupID
        {
            get { return _groupId; }
            set { _groupId = value; }
        }

        public string DSN
        {
            get { return _dbDsn; }
            set { _dbDsn = value; }
        }

        public DateTime ArrivedTime
        {
            get { return _arrivedTime; }
            set { _arrivedTime = value; }
        }

        public int DatabaseID
        {
            get { return _databaseId; }
            set { _databaseId = value; }
        }

        public int EntityTypeId
        {
            get { return _entityTypeId; }
            set { _entityTypeId = value; }
        }

        public string ExternalId
        {
            get { return _externalId; }
            set { _externalId = value; }
        }

        public override string ToString()
        {
            return "database_id: " + _databaseId + ", product_id: " + _productId + ", group_id: " + _groupId + ", action: " + ((Constants.MessageActionType) _messageType) + ", sub action: " + _subMessageType + ", entityTypeId: " + _entityTypeId;
        }
    }
}