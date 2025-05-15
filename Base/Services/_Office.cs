using Base.Models;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Base.Services
{
    //for word, excel file
    public class _Office
    {

        public static bool DocxToFile(IWorkbook docx, string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            docx.Write(fs);
            return true;
        }
    } //class
}
