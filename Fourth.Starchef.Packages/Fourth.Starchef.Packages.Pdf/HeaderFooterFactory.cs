#region usings

using System;
using System.Globalization;
using Aspose.Pdf;
using Fourth.Starchef.Packages.Model;
using Fourth.Starchef.Util;

#endregion

namespace Fourth.Starchef.Packages.Pdf
{
    public static class HeaderFooterFactory
    {
        public static void Create(Package package, string packagedFile, Config config)
        {
            if (!string.IsNullOrWhiteSpace(package.GroupLogo.Path))
            {
                package.GroupLogo.ImageStream = ImageHelper.GetGroupLogo(config.ImageBaseUrl, package.GroupLogo.Path);
            }

            if (package.GroupLogo.Width > config.HeaderFooterLogoResizedWidth)
            {
                package.GroupLogo.ResizedWidth = config.HeaderFooterLogoResizedWidth;
            }

            package.LineSeparator.ImageStream = ImageHelper.GetGroupLogo(config.ImageBaseUrl, "images/black_line.jpg");

            Document doc = new Document(packagedFile);

            foreach (Page page in doc.Pages)
            {
                if (package.Header != null)
                    SetHeader(config, package, page);

                if (package.Footer != null)
                    SetFooter(config, package, page);
            }
            doc.Save(packagedFile);
        }


        private static void SetHeader(Config config, Package package, Page page)
        {
            int pageNumber = package.PageNumber(page);

            SetHeaderFooter(package.Header.Left.HeaderFooterType, page, HorizontalAlignment.Left, VerticalAlignment.Top,
                config.HeaderFooterConfig, package.Header.Left.Text, pageNumber, package, true);
            SetHeaderFooter(package.Header.Middle.HeaderFooterType, page, HorizontalAlignment.Center, VerticalAlignment.Top,
                config.HeaderFooterConfig, package.Header.Middle.Text, pageNumber, package, true);
            SetHeaderFooter(package.Header.Right.HeaderFooterType, page, HorizontalAlignment.Right, VerticalAlignment.Top,
                config.HeaderFooterConfig, package.Header.Right.Text, pageNumber, package, true);

            AddLineSeparator(page, HorizontalAlignment.Center, VerticalAlignment.Top, config, package);
        }

        private static void SetFooter(Config config, Package package, Page page)
        {
            int pageNumber = package.PageNumber(page);

            AddLineSeparator(page, HorizontalAlignment.Center, VerticalAlignment.Bottom, config, package);

            SetHeaderFooter(package.Footer.Left.HeaderFooterType, page, HorizontalAlignment.Left, VerticalAlignment.Bottom,
                config.HeaderFooterConfig, package.Footer.Left.Text, pageNumber, package, false);
            SetHeaderFooter(package.Footer.Middle.HeaderFooterType, page, HorizontalAlignment.Center, VerticalAlignment.Bottom,
                config.HeaderFooterConfig, package.Footer.Middle.Text, pageNumber, package, false);
            SetHeaderFooter(package.Footer.Right.HeaderFooterType, page, HorizontalAlignment.Right, VerticalAlignment.Bottom,
                config.HeaderFooterConfig, package.Footer.Right.Text, pageNumber, package, false);
        }

        private static void SetHeaderFooter(HeaderFooterType headerFootertype, Page page, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, HeaderFooterConfig headerFooterConfig, string text, int pageNumber, Package package, bool isHeader)
        {
            switch (headerFootertype)
            {
                case HeaderFooterType.Company:
                    AddText(page, horizontalAlignment, verticalAlignment, headerFooterConfig, package.CompanyName, isHeader, package);
                    break;
                case HeaderFooterType.Copyright:
                    AddText(page, horizontalAlignment, verticalAlignment, headerFooterConfig, package.CopyrightText, isHeader, package);
                    break;
                case HeaderFooterType.CustomText:
                    AddText(page, horizontalAlignment, verticalAlignment, headerFooterConfig, text, isHeader, package);
                    break;

                case HeaderFooterType.Date:
                    AddText(page, horizontalAlignment, verticalAlignment, headerFooterConfig, DateTime.Now.ToShortDateString(), isHeader, package);
                    break;
                case HeaderFooterType.PageNumber:
                    if (pageNumber > 0)
                        AddText(page, horizontalAlignment, verticalAlignment, headerFooterConfig, (pageNumber).ToString(CultureInfo.InvariantCulture), isHeader, package);
                    break;

                case HeaderFooterType.Logo:
                    AddImage(page, horizontalAlignment, verticalAlignment, headerFooterConfig, package.GroupLogo, isHeader, package);
                    break;

                default:
                    return;
            }
        }

        private static void AddLineSeparator(Page page, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, Config config, Package package)
        {
            if (package.LineSeparator.ImageStream == null)
            {
                return;
            }

            // for drawing line separator between headerfooter and body, we need to add header/footer offsets to page margins 
            var bodyMargin = PageSetting.GetBodyMargin(package, config);

            ImageStamp imageStamp = new ImageStamp(package.LineSeparator.ImageStream)
            {
                TopMargin = bodyMargin.TopPoints,
                BottomMargin = bodyMargin.BottomPoints,
                LeftMargin = bodyMargin.LeftPoints,
                RightMargin = bodyMargin.RightPoints,
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = verticalAlignment,
                Width = page.PageInfo.Width - bodyMargin.LeftPoints - bodyMargin.RightPoints,
                Height = 1
            };

            page.AddStamp(imageStamp);
        }

        private static void AddImage(Page page, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, HeaderFooterConfig headerFooterConfig, GroupLogo groupLogo, bool isHeader, Package package)
        {
            if (groupLogo.ImageStream == null)
                return;
            
            var pageMargin = GetHeaderFooterMargin(horizontalAlignment, headerFooterConfig, isHeader, package);

            ImageStamp imageStamp = new ImageStamp(groupLogo.ImageStream)
            {
                TopMargin = pageMargin.TopPoints,
                BottomMargin = pageMargin.BottomPoints,
                LeftMargin = pageMargin.LeftPoints,
                RightMargin = pageMargin.RightPoints,
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = verticalAlignment,                
                Width = groupLogo.ResizedWidth,
                Height = groupLogo.CalculatedHeight > 0 ? groupLogo.CalculatedHeight : groupLogo.Height
            };

            page.AddStamp(imageStamp);
        }


        private static void AddText(Page page, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, HeaderFooterConfig headerFooterConfig, string text, bool isHeader, Package package)
        {            
            var pageMargin = GetHeaderFooterMargin(horizontalAlignment, headerFooterConfig, isHeader, package);

            TextStamp textStamp = new TextStamp(text)
            {
                TopMargin = pageMargin.TopPoints,
                BottomMargin = pageMargin.BottomPoints,
                LeftMargin = pageMargin.LeftPoints,
                RightMargin = pageMargin.RightPoints,
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = verticalAlignment
            };

            page.AddStamp(textStamp);             
        }

        private static Margin GetHeaderFooterMargin(HorizontalAlignment horizontalAlignment, HeaderFooterConfig headerFooterConfig, bool isHeader, Package package)
        {
            var resMargin = new Margin(package.PageSetting.Margin);

            if (horizontalAlignment != HorizontalAlignment.Left)
            {
                resMargin.Left = 0;
            }

            if (horizontalAlignment != HorizontalAlignment.Right)
            {
                resMargin.Right = 0;
            }

            return resMargin;
        }
    }
}