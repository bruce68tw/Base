using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Base.Services
{
    public class _File
    {		
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
            return Path.GetExtension(path).Replace(".", "").ToLower();
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
        /// read utf8 text file to string, asynchronous for big file size
        /// </summary>
        /// <param name="path">file path</param>
        /// <returns>file string, return null if no file</returns>
        public static async Task<string> ToStrAsync(string path)
        {
            if (!File.Exists(path))
                return null;

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
        public static async Task<bool> StrToFileAsync(string str, string path)
        {
            try
            {
                await File.WriteAllTextAsync(path, str, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                await _Log.ErrorAsync("_File.cs StrToFileAsync() failed: " + ex.Message);
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
                    filePath = Path.Combine(dir, fileName + fileExt + "(" + ++i + ")");
            else
                while (File.Exists(filePath))
                    filePath = Path.Combine(dir, fileName + "(" + ++i + ")" + fileExt);

            return filePath;
        }

        //get file exts by type
        public static string TypeToExts(string type)
        {
            return type switch
            {                
                "I" => "jpg,jpeg,png,gif",  //image                
                "E" => "xls,xlsx",  //excel                
                "W" => "doc,docx",  //word                
                "*" => "*",         //all
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

        /// <summary>
        /// is image file or not
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static bool IsImageExt(string ext)
        {
            //ext = ext.Replace(".", "").ToLower();
            return (",jpg,jpeg,png,gif,tif,tiff,").IndexOf("," + ext + ",") >= 0;
        }

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

    }//class
}