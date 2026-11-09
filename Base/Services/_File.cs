using Base.Enums;
using Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 檔案讀寫、壓縮、路徑與副檔名處理；未捕捉的檔案系統錯誤仍會拋出。
    /// </summary>
    public class _File
    {
        /// <summary>
        /// 讀取 JSON 提問檔；讀取或反序列化失敗時記錄並回傳 null。
        /// </summary>
        public static LlmAskDto? ReadLlmAsk(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<LlmAskDto>(json);
            }
            catch (Exception ex)
            {
                _Log.Error("_File.cs ReadLlmAsk() failed: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 將路徑副檔名改成上傳用變形名稱，不移動或加密檔案內容。
        /// </summary>
        public static string PathExtToUp(string path, string oldExt = "")
        {
            if (string.IsNullOrEmpty(oldExt))
                oldExt = GetFileExt(path);
            return Path.ChangeExtension(path, UpExtRename(oldExt));
        }

        /// <summary>
        /// 對副檔名部分字元作既有位移轉換；輸入須非空，並非加密。
        /// </summary>
        public static string UpExtRename(string ext)
        {
            var chars = ext.ToCharArray();
            int start = ext[0] == '.' ? 2 : 1;
            for (int i = start; i < chars.Length; i++)
            {
                int offset = i - start + 1;              
                chars[i] = (char)('a' + (chars[i] - 'a' + offset) % 26);    //小寫 a-z 循環
            }
            return new string(chars);
        }

        /// <summary>
        /// 依既有位移規則還原副檔名，輸入須非空。
        /// </summary>
        public static string UpExtRestore(string ext)
        {
            var chars = ext.ToCharArray();
            int start = ext[0] == '.' ? 2 : 1;
            for (int i = start; i < chars.Length; i++)
            {
                int offset = i - start + 1;
                chars[i] = (char)('a' + (chars[i] - 'a' - offset + 26 * 10) % 26);  // 反向循環
            }
            return new string(chars);
        }

        /// <summary>
        /// 建立 ZIP 並先刪除既有目的檔；名稱數量不符或來源缺失回傳 false，可能留下部分結果。
        /// </summary>
        public static bool ZipFiles(List<string> fromPaths, string toPath, List<string>? entryNames = null)
        {
            if (entryNames != null && entryNames.Count != fromPaths.Count)
            {
                _Log.Error($"_File.cs ZipFiles() Error: 參數 fromPaths 與 entryNames 陣列長度不同。");
                return false;
            }

            if (File.Exists(toPath))
                File.Delete(toPath); // 或改用 Update 模式

            var hasEntry = (entryNames != null);
            using var zip = ZipFile.Open(toPath, ZipArchiveMode.Create);
            for (int i = 0; i < fromPaths.Count; i++)
            {
                var file = fromPaths[i];
                if (!File.Exists(file))
                {
                    //continue; // 或 log 起來
                    _Log.Error($"_File.cs ZipFiles() Error: 來源檔案不存在: {file}");
                    return false;
                }

                var entryName = hasEntry ? entryNames![i] : Path.GetFileName(file);
                zip.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
            }

            //case ok
            return true;
        }

        /// <summary>
        /// 建立位置歸零的 ZIP MemoryStream，由呼叫端釋放；名稱不符或來源缺失回傳 null。
        /// </summary>
        public static MemoryStream? ZipToStream(List<string> fromPaths, List<string>? entryNames = null)
        {
            if (entryNames != null && entryNames.Count != fromPaths.Count)
            {
                _Log.Error($"_File.cs ZipToStream() Error: 參數 fromPaths 與 entryNames 陣列長度不同。");
                return null;
            }

            var hasEntry = (entryNames != null);
            var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                for (int i = 0; i < fromPaths.Count; i++)
                {
                    var file = fromPaths[i];
                    if (!File.Exists(file))
                    {
                        _Log.Error($"ZipToStream Error: 檔案不存在: {file}");
                        return null;
                    }

                    var entryName = hasEntry ? entryNames![i] : Path.GetFileName(file);
                    var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);

                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(file);

                    fileStream.CopyTo(entryStream);
                }
            }

            //很重要：reset position
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// 列出目錄直屬且尾碼符合 ext 的檔名，忽略大小寫；目錄缺失回傳 null。
        /// </summary>
        public static List<string>? GetFiles(string dir, string ext)
        {
            dir = _Str.RemoveRightSlash(dir);
            if (!Directory.Exists(dir))
            {
                _Log.Error($"No Directory: {dir}");
                return null;
            }

            // 取得檔案清單（不含子目錄）
            return Directory.GetFiles(dir)
                .Where(a => a.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                .Select(a => Path.GetFileName(a))
                .ToList();
        }

        /// <summary>
        /// 計算目錄直屬且尾碼符合 ext 的檔案數，目錄缺失回傳 0。
        /// </summary>
        public static int GetFileCount(string dir, string ext)
        {
            dir = _Str.RemoveRightSlash(dir);
            if (!Directory.Exists(dir))
            {
                _Log.Error($"No Directory: {dir}");
                return 0;
            }

            // 取得檔案清單（不含子目錄）
            return Directory.GetFiles(dir)
                .Where(file => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                .Count();
        }

        /// <summary>
        /// 目錄不存在時建立，目錄字串須非空。
        /// </summary>
        public static void MakeDir(string dir)
        {
            dir = _Str.RemoveRightSlash(dir);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// 取得路徑最後的檔名，含副檔名。
        /// </summary>
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// 取得小寫且不含句點的副檔名。
        /// </summary>
        public static string GetFileExt(string path)
        {
            //return Path.GetExtension(path).Replace(".", "").ToLower();
            var ext = Path.GetExtension(path).ToLower();
            return ext.StartsWith('.') ? ext[1..] : ext;
        }

        /// <summary>
        /// 尋找固定變形圖片尾碼；目錄缺失回傳空字串，無符合檔案回傳 null。
        /// </summary>
        public static string? DirHasUpImage(string dir, string fileName)
        {
            if (!Directory.Exists(dir))
                return "";

            //string[] imageExts = [".jpg", ".jpeg", ".png", ".gif", ".bmp"];
            string[] imageExts = [".jqi", ".jqsii", ".poi", ".gjh", ".bnr"];
            foreach (var ext in imageExts)
            {
                var fileName2 = fileName + ext;
                var filePath = Path.Combine(dir, fileName2);
                if (File.Exists(filePath))
                    return fileName2;
            }

            return null;
        }

        /// <summary>
        /// 在副檔名前插入指定尾碼，不改動實體檔案。
        /// </summary>
        public static string FnameAddTail(string fname, string tail)
        {
            var ext = Path.GetExtension(fname);  //has dot(.)
            return fname.Substring(0, fname.Length - ext.Length) + tail + ext;
        }

        /*
        //rename file, 檔名後面加上日期&時分
        public static bool Rename(string path)
        {
            var ext = Path.GetExtension(path);  //has dot(.)
            var newPath = path.Substring(0, path.Length - ext.Length - 1) + "_" + _Date.NowSecStr() + ext;
            File.Copy(path, newPath);
            File.Delete(path);
            return true;
        }
        */

        /// <summary>
        /// read utf8 text file to string, synchronous for small file size
        /// </summary>
        /// <param name="path">file path</param>
        /// <returns>file string, return null if no file</returns>
        /*
        public static async Task<string> ToStrAsync(string path)
        {
            if (!File.Exists(path))
                return null;

            //utf8 file only !!
            using (var file = new StreamReader(path, Encoding.UTF8))
            {
                return await file.ReadToEndAsync();
            }           
            //return result.Replace("\"", "");
        }
        */

        /// <summary>
        /// 以 UTF-8 讀取檔案，檔案不存在回傳 null。
        /// </summary>
        public static string? ToStr(string path, bool logError = false)
        {
            if (!File.Exists(path))
            {
                if (logError)
                    _Log.Error("_File.cs ToStr no file: " + path);
                return null;
            }

            //utf8 file only !!
            using var file = new StreamReader(path, Encoding.UTF8);
            return file.ReadToEnd();
        }

        /// <summary>
        /// 非同步以 UTF-8 讀檔，檔案不存在回傳 null。
        /// </summary>
        public static async Task<string?> ToStrA(string path)
        {
            if (!File.Exists(path)) return null;

            //utf8 file only !!
            using var file = new StreamReader(path, Encoding.UTF8);
            return await file.ReadToEndAsync();
        }

        /// <summary>
        /// 以 UTF-8 覆寫檔案，成功回傳 true；捕捉寫入例外後記錄並回傳 false。
        /// </summary>
        public static async Task<bool> StrToFileA(string str, string path)
        {
            try
            {
                await File.WriteAllTextAsync(path, str, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                await _Log.ErrorRootA("_File.cs StrToFileA() failed: " + ex.Message);
                return false;
            }
        }

        /*
        //file to bytes
        public static byte[] FileToBytes(string path)
        {
            byte[] bytes = null;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                //Read all bytes into an array from the specified file.
                var fsLen = (int)fs.Length;
                bytes = new byte[fsLen];
                var byteRead = fs.Read(bytes, 0, fsLen);
            }
            return bytes;
        }
        */

        /// <summary>
        /// 尋找未存在的加序號路徑，不建立檔案；tailPos 控制序號在副檔名前或後。
        /// </summary>
        public static string GetNextFileName(string filePath, bool tailPos)
        {
            //split the path into parts
            var dir = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var fileExt = Path.GetExtension(filePath);  //has dot(.)

            int i = 0;
            if (tailPos)
                while (File.Exists(filePath))
                    filePath = Path.Combine(dir!, fileName + fileExt + "(" + ++i + ")");
            else
                while (File.Exists(filePath))
                    filePath = Path.Combine(dir!, fileName + "(" + ++i + ")" + fileExt);

            return filePath;
        }

        /// <summary>
        /// 依上傳分類取得副檔名清單，全部類型為 *，未知類型為 ??。
        /// </summary>
        public static string TypeToExts(UpFileTypeEnum type, string fileTypeExts)
        {
            return type switch
            {
                UpFileTypeEnum.Image => "jpg,jpeg,png,gif",
                UpFileTypeEnum.Custom => fileTypeExts,
                //"E" => "xls,xlsx",  //excel
                //"W" => "doc,docx",  //word
                //"P" => "pdf",       //pdf
                UpFileTypeEnum.All => "*",
                _ => "??",
            };
        }

        /// <summary>
        /// 取得目錄中符合檔名前綴的第一個路徑，無結果回傳預設圖片路徑。
        /// </summary>
        public static string GetFirstPath(string dir, string preName, string noImagePath)
        {
            var files = Directory.GetFiles(dir, preName + "*");
            return (files.Length == 0)
                ? noImagePath
                : files[0];
        }

        /* 直接用 _Http.ExtToContentType() 判斷是否為圖片檔
        /// <summary>
        /// is image file or not
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static bool IsImageExt(string ext)
        {
            //ext = ext.Replace(".", "").ToLower();
            return (",jpg,jpeg,png,gif,tif,tiff,").Contains("," + ext + ",");
        }
        */

        /// <summary>
        /// 以字串分割取得目錄，優先採最後一個斜線，不進行路徑正規化。
        /// </summary>
        public static string PathToDir(string path)
        {
            var dir = _Str.GetLeft2(path, "/");
            if (dir == path)
                dir = _Str.GetLeft2(path, "\\");
            return dir;
        }

        /// <summary>
        /// 以識別碼與原檔副檔名組成檔名；原檔名為空時回傳空字串。
        /// </summary>
        public static string IdToFileName(string id, string fileName)
        {
            return (string.IsNullOrEmpty(fileName))
                ? "" 
                : id + "." + GetFileExt(fileName);
        }

    }//class
}
