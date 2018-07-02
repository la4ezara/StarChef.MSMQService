#region usings

using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.DataService
{
    public static class MapDocumentType
    {
        public static DocumentType Of(string scDocumentType)
        {
            switch (scDocumentType.ToUpperInvariant())
            {
                case "XLS":
                    return DocumentType.Excel;
                case "PDF":
                    return DocumentType.Pdf;
                case "DOC":
                    return DocumentType.Word;
                default:
                    return DocumentType.None;
            }
        }
    }
}