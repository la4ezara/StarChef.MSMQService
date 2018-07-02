using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fourth.Orchestration.Model.StarChef.Events;

namespace StarChef.Orchestrate.Models.TransferObjects
{
    public class FileUploadCompletedTransferObject
    {
        public string CustomerCanonicalId { get; set; }
        public int InternalCustomerId { get; set; }
        public string FilePath { get; set; }
        public SourceLoginDb SourceLogin { get; set; }

    }
}
