using Messaging.MSMQ.Interface;
using System;

namespace Messaging.MSMQ
{
    public class UpdateMessage : IMessage
    {
        private int _product_id = 0;
        private int _message_type = 0;
        private int _sub_message_type = 0;
        private int _group_id = 0;
        private string _dbDSN = string.Empty;
        private DateTime _arrived_time = DateTime.MinValue;
        private int _database_id = 0;
        private int _userId = 0;
        private int _ugroupId = 0;
        private Guid _uniqueIdent = Guid.Empty;
        private int _entity_type_id = 0;

        public UpdateMessage()
        {
        }

        /// <summary>
        /// Using this method for Orchestration as well
        /// </summary>
        /// <param name="productId">Entity id (Ingredient/Recipe/Menu/Meal period/Group/User/User group) Id </param>
        /// <param name="dbDSN">Connection String</param>
        /// <param name="action">Msmq Action type id </param>
        /// <param name="databaseId">Database Id</param>
        /// <param name="entityTypeId">Entity type id (</param>
        public UpdateMessage(int productId, string dbDSN, int action, int databaseId, int entityTypeId = 0)
        {
            _product_id = productId;
            _group_id = 0;
            _dbDSN = dbDSN;
            _message_type = action;
            _database_id = databaseId;
            _entity_type_id = entityTypeId;
        }

        public UpdateMessage(int productId, int groupId, string dbDSN, int action, int databaseId)
        {
            _product_id = productId;
            _group_id = groupId;
            _dbDSN = dbDSN;
            _message_type = action;
            _database_id = databaseId;
        }

        public UpdateMessage(int productId, int groupId, int userId, int ugroupId, int action, int databaseId, string dbDSN)
        {
            _product_id = productId;
            _group_id = groupId;
            _dbDSN = dbDSN;
            _message_type = action;
            _database_id = databaseId;
            _userId = userId;
            _ugroupId = ugroupId;
        }


        public int SubAction
        {
            get { return _sub_message_type; }
            set { _sub_message_type = value; }
        }

        public int Action
        {
            get { return _message_type; }
            set { _message_type = value; }
        }

        public int ProductID
        {
            get { return _product_id; }
            set { _product_id = value; }
        }

        public int GroupID
        {
            get { return _group_id; }
            set { _group_id = value; }
        }

        public string DSN
        {
            get { return _dbDSN; }
            set { _dbDSN = value; }
        }

        public DateTime ArrivedTime
        {
            get { return _arrived_time; }
            set { _arrived_time = value; }
        }

        public int DatabaseID
        {
            get { return _database_id; }
            set { _database_id = value; }
        }

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public int UGroupId
        {
            get { return _ugroupId; }
            set { _ugroupId = value; }
        }

        public Guid UniqueIdent
        {
            get { return this._uniqueIdent; }
            set { this._uniqueIdent = value; }
        }


        public int EntityTypeId
        {
            get { return _entity_type_id; }
            set { _entity_type_id = value; }
        }

        public override string ToString()
        {
            if (_message_type == (int)Constants.MessageActionType.CreatePackage)
            {
                return string.Format("database_id:{0},entity_id:{1},group_id:{2},user_id:{3},ugroup_id:{4},unique_ident:{5}",
                    this._database_id.ToString(),
                    this._product_id.ToString(),
                    this._group_id.ToString(),
                    this._userId.ToString(),
                    this._ugroupId.ToString(),
                    this._uniqueIdent.ToString());
            }
            if (_message_type == (int)Constants.MessageActionType.StarChefEventsUpdated)
            {
                return string.Format("database_id: {0}, entity_id: {1}, group_id: {2}, action: {3}, sub action: {4}, entity_type_id: {5}",
                    this._database_id.ToString(),
                    this._product_id.ToString(),
                    this._group_id.ToString(),
                    ((Constants.MessageActionType)_message_type).ToString(),
                    this._sub_message_type.ToString(),
                    this._entity_type_id.ToString());
            }

            return "database_id: " + _database_id.ToString() + ", product_id: " + _product_id.ToString() + ", group_id: " + _group_id.ToString() + ", action: " + ((Constants.MessageActionType)_message_type).ToString() + ", sub action: " + _sub_message_type.ToString();
        }
    }
}
