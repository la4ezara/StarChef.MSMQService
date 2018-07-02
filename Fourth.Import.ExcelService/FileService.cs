using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fourth.Import.ExcelService
{
    public class FileService
    {
        public void MoveFile(string source, string destination)
        {
            if(File.Exists(source))
                File.Move(source, destination);
        }
    }
}
