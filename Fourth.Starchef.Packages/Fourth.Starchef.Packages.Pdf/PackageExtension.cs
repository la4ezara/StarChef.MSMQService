#region usings

using System.Linq;
using Aspose.Pdf;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.Pdf
{
    public static class PackageExtension
    {
        public static int PageNumber(this Package package, Page page)
        {
            int pageNumber =
                package.PdfPackagePageNumbers.Where(p => p.PdfPageNumber == page.Number)
                    .Select(i => i.ContentPageNumber)
                    .FirstOrDefault();
            return pageNumber;
        }
    }
}