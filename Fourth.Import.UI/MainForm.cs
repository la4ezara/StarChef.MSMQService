using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Fourth.Import.ExcelService;
using Fourth.Import.Process;

namespace Fourth.Import.UI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            string uploadPath = ConfigurationManager.AppSettings["ImportFilePath"].ToString();
            string suppUploadPath = ConfigurationManager.AppSettings["SupplierImportFilePath"].ToString();

            IngredientImport ingredientImport = new IngredientImport();

            if (Directory.GetFiles(suppUploadPath).Any())
            {
                CopySupplierImportFiles();
            }

            if (!Directory.GetFiles(uploadPath).Any())
            {
                MessageBox.Show("Import folder is empty");
            }
            else
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(uploadPath);
                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.xls*"))
                {
                    ingredientImport.Process(fileInfo, "StarChefLogin");
                }
            }
        }

        private void CopySupplierImportFiles()
        {
            string importFileFolder = ConfigurationManager.AppSettings["ImportFilePath"].ToString();
            int startHour = Convert.ToInt32(ConfigurationManager.AppSettings["SupplierImportStartTime"]);
            int endHour = Convert.ToInt32(ConfigurationManager.AppSettings["SupplierImportEndTime"]);
            string supplierImportFolder = ConfigurationManager.AppSettings["SupplierImportFilePath"];
            DateTime currentTime = DateTime.Now;
            TimeSpan startTime = new TimeSpan(startHour, 0, 0);
            TimeSpan endTime = new TimeSpan(endHour, 0, 0);
            if (currentTime.TimeOfDay > startTime && currentTime.TimeOfDay < endTime)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(supplierImportFolder);
                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.xls*"))
                {
                    FileService fileService = new FileService();
                    fileService.MoveFile(fileInfo.FullName, importFileFolder + fileInfo.Name);
                }

            }

        }
    }
}
