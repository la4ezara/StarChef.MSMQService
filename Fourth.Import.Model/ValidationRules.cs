namespace Fourth.Import.Model
{
    public class ValidationRules
    {
        public bool Mandatory { get; set; }
        public decimal MinimumValue { get; set; }
        public decimal MaximumValue { get; set; }
        public string RegEx { get; set; }
        public int StringLength { get; set; }
    }
}