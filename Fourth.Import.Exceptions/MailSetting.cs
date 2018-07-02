using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fourth.Import.Exceptions
{
    public class MailSetting
    {
        public string FromEmail { get; set; }
        public string Subject { get; set; }
        public string Alias { get; set; }
        public bool HighImportance { get; set; }
        public IList<string> ToEmail { get; set; }  
    }
}
