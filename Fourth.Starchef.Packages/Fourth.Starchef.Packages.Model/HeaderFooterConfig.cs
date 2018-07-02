namespace Fourth.Starchef.Packages.Model
{
    public class HeaderFooterConfig  /**/
    {
      public HeaderFooterConfig()
        {
            HeaderMargin = new Margin();
            FooterMargin = new Margin();
        }

        public Margin HeaderMargin { get; private set; }
        public Margin FooterMargin { get; private set; }

        public double MarginTopOffset { get; set; }
        public double MarginBottomOffset { get; set; }

        public double MarginTopOffsetPoints { get { return MarginTopOffset * Margin.OneCmInPoints; } }
        public double MarginBottomOffsetPoints { get { return MarginBottomOffset * Margin.OneCmInPoints; } }
    }
}