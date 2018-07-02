#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.Pdf
{
    public class PdfConvert
    {
        public void ToPdf(AuxDocument auxDocument, string documentsFolder, ICollection<AuxDocument> convertedDocuments)
        {
            AuxDocument convertedDocumentDetails = convertedDocuments.FirstOrDefault(d => d.Id == auxDocument.Id);
            if (convertedDocumentDetails != null)
            {
                auxDocument.ConversionRequired = false;
                auxDocument.ConvertedFile = convertedDocumentDetails.ConvertedFile;
                auxDocument.PageCount = TotalPages.GetCount(auxDocument.ConvertedFile);
                return;
            }

            string copiedFileName = string.Format("pdf_{0}.pdf",Guid.NewGuid());
            File.Copy(documentsFolder + auxDocument.SourceFile, documentsFolder + copiedFileName);
            auxDocument.ConvertedFile = copiedFileName;
            convertedDocuments.Add(auxDocument);
        }
    }
}