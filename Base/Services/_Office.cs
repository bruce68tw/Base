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
    /// <summary>舊版 NPOI 工作簿輸出工具，現行專案設定排除此類別。</summary>
    public class _Office
    {

        /// <summary>將工作簿寫入檔案，覆寫同名檔。成功時回傳 true，寫入例外由呼叫端處理。</summary>
        /// <param name="docx">要寫入目的檔的 NPOI 工作簿</param>
        /// <param name="filePath">要寫入工作簿的檔案路徑，同名檔會被覆寫。</param>
        public static bool DocxToFile(IWorkbook docx, string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            docx.Write(fs);
            return true;
        }
    } //class
}
