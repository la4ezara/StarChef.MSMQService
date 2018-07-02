using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Fourth.Import.ExcelService
{
    public class ExceptionReportService
    {
        
        public void Export(DataTable exceptionReport, string exceptionFileName, IList<int> dataRows)
        {
            HSSFWorkbook hssfworkbook;
            FileStream file = new FileStream(ConfigurationManager.AppSettings["TemplateFilePath"] + "ExceptionReportTemplate.xls", FileMode.Open, FileAccess.Read);

            hssfworkbook = new HSSFWorkbook(file);

            ISheet sheet1 = hssfworkbook.GetSheetAt(0);
            ICellStyle dataRowcs = hssfworkbook.CreateCellStyle();
            dataRowcs.FillForegroundColor = new NPOI.HSSF.Util.HSSFColor.PaleBlue().Indexed;
            dataRowcs.FillPattern = FillPattern.SolidForeground;

            ICellStyle warningcs = hssfworkbook.CreateCellStyle();
            warningcs.FillForegroundColor = new NPOI.HSSF.Util.HSSFColor.Grey80Percent().Indexed;
            warningcs.FillPattern = FillPattern.SolidForeground;

            for (int rowcnt = 0; rowcnt < exceptionReport.Rows.Count;rowcnt++)
            {
                if (sheet1.GetRow(rowcnt) == null) sheet1.CreateRow(rowcnt);

                for(int cellcnt = 0;cellcnt < exceptionReport.Columns.Count;cellcnt++)
                {
                    if (sheet1.GetRow(rowcnt).GetCell(cellcnt) == null)
                    {
                        sheet1.GetRow(rowcnt).CreateCell(cellcnt);
                    }
                    sheet1.GetRow(rowcnt).GetCell(cellcnt).SetCellValue(exceptionReport.Rows[rowcnt][cellcnt] != null
                                                                            ? exceptionReport.Rows[rowcnt][cellcnt].
                                                                                  ToString()
                                                                            : string.Empty);

                    if (dataRows.Contains(rowcnt))
                        sheet1.GetRow(rowcnt).GetCell(cellcnt).CellStyle = dataRowcs;
                }
                
            }

            //Write the stream data of workbook to the root directory
            FileStream stream = new FileStream(exceptionFileName, FileMode.Create);
            hssfworkbook.Write(stream);
            stream.Close();
        }


        public void Export(DataTable exceptionReport, string exceptionFileName)
        {
            HSSFWorkbook hssfworkbook;
            FileStream file = new FileStream(ConfigurationManager.AppSettings["TemplateFilePath"] + "ExceptionReportTemplate2.xls", FileMode.Open, FileAccess.Read);

            hssfworkbook = new HSSFWorkbook(file);

            ISheet sheet1 = hssfworkbook.GetSheetAt(0);
            ICellStyle dataRowcs = hssfworkbook.CreateCellStyle();
            dataRowcs.FillForegroundColor = new NPOI.HSSF.Util.HSSFColor.PaleBlue().Indexed;
            dataRowcs.FillPattern = FillPattern.SolidForeground;

            ICellStyle warningcs = hssfworkbook.CreateCellStyle();
            warningcs.FillForegroundColor = new NPOI.HSSF.Util.HSSFColor.Grey80Percent().Indexed;
            warningcs.FillPattern = FillPattern.SolidForeground;

            for (int rowcnt = 0; rowcnt < exceptionReport.Rows.Count; rowcnt++)
            {
                if (sheet1.GetRow(rowcnt) == null) sheet1.CreateRow(rowcnt);

                for (int cellcnt = 0; cellcnt < exceptionReport.Columns.Count; cellcnt++)
                {
                    if (sheet1.GetRow(rowcnt).GetCell(cellcnt) == null)
                    {
                        sheet1.GetRow(rowcnt).CreateCell(cellcnt);
                    }
                    sheet1.GetRow(rowcnt).GetCell(cellcnt).SetCellValue(exceptionReport.Rows[rowcnt][cellcnt] != null
                                                                            ? exceptionReport.Rows[rowcnt][cellcnt].
                                                                                  ToString()
                                                                            : string.Empty);

                    
                }

            }

            //Write the stream data of workbook to the root directory
            FileStream stream = new FileStream(exceptionFileName, FileMode.Create);
            hssfworkbook.Write(stream);
            stream.Close();
        }
    }
}