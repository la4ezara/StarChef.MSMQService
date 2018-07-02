#region usings

using System;
using System.Linq;
using Aspose.Pdf;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.Pdf
{
    public class Files
    {
        public string MergeDocs(Config config, Package package)
        {
            License license = new License();
            license.SetLicense(config.AsposeLicenseFile);
            Aspose.Pdf.Generator.Pdf pdf = new Aspose.Pdf.Generator.Pdf();

            Document sourceDocument = new Document(pdf);
            return AppendFiles(config, package, sourceDocument);
        }

        public string MergeDocs(Config config, Package package, string tocFilePath)
        {
            package.LogProgress(package.LogId, "Started merging documents");
            License license = new License();
            license.SetLicense(config.AsposeLicenseFile);
            Aspose.Pdf.Generator.Pdf pdf = new Aspose.Pdf.Generator.Pdf();
            Document sourceDocument = new Document(pdf)
            {
                PageInfo = new PageInfo
                {
                    Width = PageSize.PageLetter.Width,
                    Height = PageSize.PageLetter.Height
                }
            };

            Document document = new Document(tocFilePath);
            foreach (Page page in document.Pages)
            {
                sourceDocument.Pages.Add(page);
            }

            return AppendFiles(config, package, sourceDocument);
        }

        private string AppendFiles(Config config, Package package, Document sourceDocument)
        {
            License license = new License();
            license.SetLicense(config.AsposeLicenseFile);

            foreach (Section section in package.Sections.OrderBy(s => s.Order))
            {
                package.LogProgress(package.LogId, "Creating package section \"" + section.Name + "\"");
                foreach (Item item in section.Items.OrderBy(i => i.Order))
                {
                    Document document = null;
                    switch (item.ItemType)
                    {
                        case ItemType.AuxiliaryDocument:
                            package.LogProgress(package.LogId, "Appending document \"" + item.Name);
                            if (item.AuxiliaryDocument.PageCount <= 0) continue;
                            document = new Document(config.DocumentsConvertedFolder+item.AuxiliaryDocument.ConvertedFile);
                            break;
                        case ItemType.Report:
                            package.LogProgress(package.LogId, "Appending report \"" + item.Name);
                            if (item.ReportFilter.PageCount <= 0) continue;
                            document = new Document(config.ReportOutputPath + item.ReportFilter.ReportPath);
                            break;
                    }

                    if (document == null) continue;
                    int startPageNumber = item.StartPageNumber;

                    foreach (Page page in document.Pages)
                    {
                        if (item.ItemType == ItemType.AuxiliaryDocument && (item.AuxiliaryDocument.DocumentType == DocumentType.Excel || item.AuxiliaryDocument.DocumentType == DocumentType.Pdf))
                            PageSetting.SetPageSize(page, package.PageSetting.Paper);

                        sourceDocument.Pages.Add(page);
                        package.PdfPackagePageNumbers.Add(new PdfPackagePageNumber
                        {
                            PdfPageNumber = sourceDocument.Pages.Count - 1,
                            ContentPageNumber = startPageNumber
                        });
                        startPageNumber++;
                    }
                }
                package.LogProgress(package.LogId, "Completed appending section \"" + section.Name + "\"");
            }

            string fileName = string.Format("{0}_{1}.pdf", package.Name.Replace(" ","_"), DateTime.Now.ToString("yyyyMMddHHmmss"));
            string packagedFile = config.DocumentsConvertedFolder + @"\" + fileName;

            if (sourceDocument.Pages.Count > 0)
                sourceDocument.Pages.Delete(1);

            sourceDocument.Save(packagedFile);

            PageSetting.AddMargins(packagedFile, package, config);
            HeaderFooterFactory.Create(package, packagedFile, config);
            return fileName;
        }
    }
}