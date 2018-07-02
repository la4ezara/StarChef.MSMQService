#region usings

using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.Pdf
{
    public static class PageSetting    /**/
    {
        public static void SetPageSize(Page page, Paper paper)
        {
            var pageSize = (paper == Paper.Letter) ? PageSize.PageLetter : PageSize.A4;
            page.SetPageSize(pageSize.Width, pageSize.Height);
        }

        public static void AddMargins(string packagedFile, Package package, Config config)
        {
            PdfFileEditor fileEditor = new PdfFileEditor();
            var pageMargin = PageSetting.GetBodyMargin(package, config);
            fileEditor.AddMargins(packagedFile, packagedFile, null, pageMargin.LeftPoints, pageMargin.RightPoints, pageMargin.TopPoints, pageMargin.BottomPoints);
        }
        
        // returns margin for body of the page
        public static Margin GetBodyMargin(Package package, Config config)
        {
            var resMargin = new Margin(package.PageSetting.Margin);

            // if header exists we need to add offset to top margin
            if (!package.Header.SuppressHeaderFooter)
            {
                resMargin.Top += config.HeaderFooterConfig.MarginTopOffset;
            }

            // if footer exists we need to add offset to bottom margin
            if (!package.Footer.SuppressHeaderFooter)
            {
                resMargin.Bottom += config.HeaderFooterConfig.MarginBottomOffset;
            }

            return resMargin;
        }
    }
}