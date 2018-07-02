using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fourth.Import.Model
{
    public class ImportType
    {
        public ImportOperation Operation { get; set; }
        public string PrerequisiteQuery { get; set; }
    }
}
