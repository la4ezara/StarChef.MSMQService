#region usings

using System;
using System.IO;
using Aspose.Pdf;

#endregion

namespace Fourth.Starchef.Packages.Pdf
{
    public static class TotalPages
    {
        public static int GetCount(string filePath)
        {
            try
            {
                Document pdfDocument = new Document(filePath);
                return pdfDocument.Pages.Count;
            }
            catch (FileNotFoundException e)
            {
                return 0;
            }
            
          
        }
    }
}