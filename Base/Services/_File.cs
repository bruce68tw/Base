using Base.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Services
{
    public class _File
    {
        /// <summary>
        /// rename path ext to upload style
        /// </summary>
        /// <param name="path"></param>
        /// <param name="oldExt">如果有值則直接轉換成新的副檔名, 不必重新讀取path的ext</param>
        /// <returns></returns>
        public static string PathExtToUp(string path, string oldExt = "")
        {
            if (string.IsNullOrEmpty(oldExt))
                oldExt = GetFileExt(path);
            return Path.ChangeExtension(path, UpExtRename(oldExt));
        }

        /// <summary>
        /// 將上傳檔案副檔名rename for 資安考慮
        /// </summary>
        /// <param name="ext">是否前面有'.'皆可</param>
        /// <returns></returns>
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
        /// 將上傳檔案副檔名restore
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
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
        /// 壓縮多個檔案成zip檔
        /// </summary>
        /// <param name="fromPaths"></param>
        /// <param name="toPath"></param>
        /// <param name="entryNames">如果要改變zip裡面的每個檔名則必須填此欄位, 長度與fromPaths相同</param>
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
        /// 讀取檔案清單, 傳回字串不含路徑
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="ext">副檔名</param>
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
        /// 讀取檔案數量
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="ext">副檔名</param>
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
        /// make folder
        /// </summary>
        /// <param name="dir">folder path, can has right slash or not</param>
        public static void MakeDir(string dir)
        {
            dir = _Str.RemoveRightSlash(dir);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// get file name(and .ext) with path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// get file ext, lowercase no dot, ex: docx
        /// </summary>
        /// <param name="path">file path</param>
        /// <returns></returns>
        public static string GetFileExt(string path)
        {
            //return Path.GetExtension(path).Replace(".", "").ToLower();
            var ext = Path.GetExtension(path).ToLower();
            return ext.StartsWith('.') ? ext[1..] : ext;
        }

        /// <summary>
        /// 目錄下是否存在符合檔名的各種圖檔
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="fileName">不含副檔名</param>
        /// <returns>傳回第一個符合的圖檔名稱, 不含路徑</returns>
        public static string? DirHasImage(string dir, string fileName)
        {
            if (!Directory.Exists(dir))
                return "";

            string[] imageExts = [".jpg", ".jpeg", ".png", ".gif", ".bmp"];
            foreach (var ext in imageExts)
            {
                var fileName2 = fileName + ext;
                var filePath = Path.Combine(dir, fileName2);
                if (File.Exists(filePath))
                    return fileName2;
            }

            return null;
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
        /// read utf8 text file to string, synchronous for big file size
        /// </summary>
        /// <param name="path">file path</param>
        /// <returns>file string, return null if no file</returns>
        public static string? ToStr(string path)
        {
            if (!File.Exists(path)) return null;

            //utf8 file only !!
            using var file = new StreamReader(path, Encoding.UTF8);
            return file.ReadToEnd();
        }

        /// <summary>
        /// read utf8 text file to string, asynchronous for big file size
        /// </summary>
        /// <param name="path">file path</param>
        /// <returns>file string, return null if no file</returns>
        public static async Task<string?> ToStrA(string path)
        {
            if (!File.Exists(path)) return null;

            //utf8 file only !!
            using var file = new StreamReader(path, Encoding.UTF8);
            return await file.ReadToEndAsync();
        }

        /// <summary>
        /// write string into text file
        /// </summary>
        /// <param name="str">content</param>
        /// <param name="path">file full path</param>
        /// <returns></returns>        
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
        /// get next not repeat file name
        /// </summary>
        /// <param name="filePath">current file path</param>
        /// <param name="tailPos">true:add sn at end, false:add sn before ext</param>
        /// <returns></returns>
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

        //get upload file exts by type
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
        /// get first match file path
        /// </summary>
        /// <param name="fileNoExt">file path without ext</param>
        /// <returns></returns>
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
        /// file path to dir, no right slash
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string PathToDir(string path)
        {
            var dir = _Str.GetLeft2(path, "/");
            if (dir == path)
                dir = _Str.GetLeft2(path, "\\");
            return dir;
        }

        public static string IdToFileName(string id, string fileName)
        {
            return (string.IsNullOrEmpty(fileName))
                ? "" 
                : id + "." + GetFileExt(fileName);
        }

    }//class
}