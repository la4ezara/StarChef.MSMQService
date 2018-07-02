#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.Cells;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.Excel
{
    public class ExcelConvert
    {
        public void ToPdf(AuxDocument auxDocument, string documentsFolder, ICollection<AuxDocument> convertedDocuments, Config config)
        {
            License license = new License();
            license.SetLicense(config.AsposeLicenseFile);

            AuxDocument convertedDocumentDetails = convertedDocuments.FirstOrDefault(d => d.Id == auxDocument.Id);
            if (convertedDocumentDetails != null)
            {
                auxDocument.ConversionRequired = false;
                auxDocument.ConvertedFile = convertedDocumentDetails.ConvertedFile;
                return;
            }

            Workbook workbook = new Workbook(documentsFolder+auxDocument.SourceFile)
            {
                FileFormat = FileFormatType.Pdf
            };

            auxDocument.ConvertedFile = string.Format("excel_{0}.pdf", Guid.NewGuid());

            workbook.Save(documentsFolder+auxDocument.ConvertedFile, new PdfSaveOptions
            {
                OnePagePerSheet = config.ConvertExcelToOnePagePerSheet
            });
            convertedDocuments.Add(auxDocument);
        }
    }
}