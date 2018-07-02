using System.IO;

namespace Fourth.Starchef.Packages.Model
{
    public class GroupLogo
    {
        private double _resizedWidth;
        public string Path { get; set; }
        public Stream ImageStream { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public double ResizedWidth
        {
            get { return _resizedWidth; }
            set
            {   
                _resizedWidth = value;
                if (Width > 0 && Height > 0 && _resizedWidth>0)
                    CalculatedHeight= (Height / Width) * ResizedWidth;
            }
        }
        public double CalculatedHeight { get; private set; }
        
    }
}