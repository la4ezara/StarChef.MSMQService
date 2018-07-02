#region usings

using Aspose.Pdf.Exceptions;
using Aspose.Pdf.Text;

#endregion

namespace Fourth.Starchef.Packages.Pdf
{
    public static class FontUtil
    {
        public static Font GetFont(string fontName)
        {
            try
            {
                return FontRepository.FindFont(fontName);
            }
            catch (FontNotFoundException)
            {
                return FontRepository.FindFont("Arial");
            }
        }
    }
}