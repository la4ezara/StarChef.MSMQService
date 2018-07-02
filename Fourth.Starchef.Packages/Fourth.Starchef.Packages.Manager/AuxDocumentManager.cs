#region usings

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Fourth.Starchef.Packages.DataService;
using Fourth.Starchef.Packages.Excel;
using Fourth.Starchef.Packages.Model;
using Fourth.Starchef.Packages.Pdf;
using Fourth.Starchef.Packages.Word;

#endregion

namespace Fourth.Starchef.Packages.Manager
{
    public class AuxDocumentManager
    {
        public void Process(Config config, Model.Package package, string documentsFolderPath)
        {
            ICollection<AuxDocument> convertedDocuments = new Collection<AuxDocument>();
            package.LogProgress(package.LogId, "Processing documents");
            foreach (Section section in package.Sections)
            {
                package.LogProgress(package.LogId, "Processing documents in section \"" + section.Name + "\"");
                foreach (Item item in section.Items.Where(i => i.ItemType == ItemType.AuxiliaryDocument))
                {
                    if (!item.AuxiliaryDocument.ConversionRequired)
                        continue;

                    if (!File.Exists(documentsFolderPath + item.AuxiliaryDocument.SourceFile))
                        continue;

                    package.LogProgress(package.LogId, "Converting document for \"" + item.Name + "\"");
                    switch (item.AuxiliaryDocument.DocumentType)
                    {
                        case DocumentType.Word:
                            new WordConvert().ToPdf(item.AuxiliaryDocument, documentsFolderPath, convertedDocuments, config);

                            break;
                        case DocumentType.Excel:
                            new ExcelConvert().ToPdf(item.AuxiliaryDocument, documentsFolderPath, convertedDocuments, config);
                            break;

                        case DocumentType.Pdf:
                            new PdfConvert().ToPdf(item.AuxiliaryDocument, documentsFolderPath, convertedDocuments);

                            break;

                        default:
                            continue;
                    }
                }
                package.LogProgress(package.LogId, "Completed processing documents in section \"" + section.Name + "\"");
            }
            package.LogProgress(package.LogId, "Completed documents conversion");
            PackageItemService packageItemService = new PackageItemService();
            packageItemService.UpdateConverted(convertedDocuments, config);
        }
    }
}