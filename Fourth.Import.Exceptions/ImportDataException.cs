namespace Fourth.Import.Exceptions
{
    public class ImportDataException 
    {
        public ExceptionType ExceptionType { get; set; }
        public string TemplateColumnName { get; set; }
        public string TemplateMappingColumn { get; set; }
        public string MessageParam1 { get; set; }
        public string MessageParam2 { get; set; }
        public bool IsValid { get; set; }
    }
}