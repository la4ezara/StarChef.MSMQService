using static Fourth.Orchestration.Model.StarChef.Events;

namespace StarChef.Orchestrate.Models.TransferObjects
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class FileUploadCompletedTransferObject
    {
        public string CustomerCanonicalId { get; set; }
        public int InternalCustomerId { get; set; }
        public string FilePath { get; set; }
        public SourceLoginDb SourceLogin { get; set; }

    }
}
