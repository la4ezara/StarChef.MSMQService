namespace Fourth.Starchef.Packages.Model
{
    public class AuxDocument
    {
        public int Id { get; set; }
        public DocumentType DocumentType { get; set; }
        public string SourceFile { get; set; }
        public string ConvertedFile { get; set; }
        public bool ConversionRequired { get; set; }
        public int PageCount { get; set; }
    }
}