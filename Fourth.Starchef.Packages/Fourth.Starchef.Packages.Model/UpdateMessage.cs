#region usings

using System;

#endregion

namespace Fourth.Starchef.Packages.Model
{
    [Serializable]
    public class UpdateMessage
    {
        public string DSN { get; set; }
        public int SubAction { get; set; }
        public int Action { get; set; }
        public int ProductID { get; set; }
        public int GroupID { get; set; }
        public int UGroupId { get; set; }
        public int UserId { get; set; }
    }
}