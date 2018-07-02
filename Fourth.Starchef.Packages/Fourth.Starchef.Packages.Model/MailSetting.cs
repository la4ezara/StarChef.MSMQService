#region usings

using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Fourth.Starchef.Packages.Model
{
    public class MailSetting
    {
        public MailSetting()
        {
            ToEmail = new Collection<string>();
        }

        public string FromEmail { get; set; }
        public string Subject { get; set; }
        public string Alias { get; set; }
        public string Body { get; set; }
        public bool IsHighImportance { get; set; }
        public ICollection<string> ToEmail { get; private set; }
    }
}