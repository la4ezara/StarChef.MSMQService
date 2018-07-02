namespace Fourth.Starchef.Packages.Model
{
    public class Margin      /**/
    {

        public const double OneCmInPoints = 28.346456;

        public Margin()
        {
        }

        public Margin(Margin sourceMargin)
        {
            this.Left = sourceMargin.Left;
            this.Right = sourceMargin.Right;
            this.Top = sourceMargin.Top;
            this.Bottom = sourceMargin.Bottom;
        }

        public double Left { get; set; }
        public double Right { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }

        public double LeftPoints
        {
            get { return Left * Margin.OneCmInPoints; }
        }

        public double RightPoints
        {
            get { return Right * Margin.OneCmInPoints; }
        }

        public double TopPoints
        {
            get { return Top * Margin.OneCmInPoints; }
        }

        public double BottomPoints
        {
            get { return Bottom * Margin.OneCmInPoints; }
        }
    }
}