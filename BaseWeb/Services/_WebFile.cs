using Base.Services;
using Base.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BaseWeb.Services
{
    public class _WebFile
    {
        /// <summary>
        /// check upload file size
        /// </summary>
        /// <param name="file"></param>
        /// <param name="size">file size limit(MB)</param>
        /// <returns>true(match)</returns>
        public static bool CheckFileSize(IFormFile file, int size)
        {
            return (file.Length <= size * 1024 * 1024);
        }

        /// <summary>
        /// check upload file extension
        /// </summary>
        /// <param name="file"></param>
        /// <param name="exts">extension list(sep with ',')</param>
        /// <returns>check status</returns>
        public static bool CheckFileExt(IFormFile file, string exts)
        {
            var ext = Path.GetExtension(file.FileName).Replace(".", "").ToLower();
            return ("," + exts.ToLower() + ",").Contains("," + ext + ",");
        }

        public static async Task<bool> SaveFileAsync(IFormFile file, string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return true;
            }
            catch (Exception ex)
            {
                _Log.Error("_WebFile.cs SaveUploadFile() failed: " + ex.Message);
                return false;
            }
        }

        //return related path for save file
        //tail has unique key for file name, prevent duplicated & browser cache (yyyyMMddHHmmss)
        private static string GetFilePath(string dir, string fileTail, IFormFile file)
        {
            //DateTime.Now.ToString("ddmmfff")
            return dir + Path.GetFileNameWithoutExtension(file.FileName) +
                "_" + fileTail + _Str.NewId() + Path.GetExtension(file.FileName);
        }

        public static async Task<bool> SaveCrudFileAsnyc(JObject inputJson, JObject newKey, string saveDir, IFormFile file, string serverFid)
        {
            if (file == null)
                return true;

            await SaveCrudFilesAsnyc(inputJson, newKey, saveDir, new List<IFormFile> { file }, serverFid, false);
            return true;
        }

        /// <summary>
        /// save crud files
        /// </summary>
        /// <param name="inputJson"></param>
        /// <param name="newKey"></param>
        /// <param name="saveDir"></param>
        /// <param name="files"></param>
        /// <param name="serverFid"></param>
        /// <param name="isMulti">if true, fileJson fid will like 't03_FileNameX', X is row no</param>
        /// <returns></returns>
        public static async Task<bool> SaveCrudFilesAsnyc(JObject inputJson, JObject newKey, string saveDir, List<IFormFile> files, string serverFid, bool isMulti = true)
        {
            if (files == null || files.Count == 0)
                return true;

            string error;
            if (inputJson[_Web.FileJson] == null)
            {
                error = "inputJson[" + _Web.FileJson + "] is empty.";
                goto lab_error;
            }

            var values = serverFid.Split('_');    //format: txx_fid
            if (values.Length != 2)
            {
                error = "serverFid format wrong.(" + serverFid + ")";
                goto lab_error;
            }

            //save files
            _File.MakeDir(saveDir);
            saveDir = _Str.AddAntiSlash(saveDir);
            var fileJson = (JObject)inputJson[_Web.FileJson];   //file vs pkey
            var newKeyCol = values[0];  //equals to 't' + levelStr
            var fid = values[1];        //file fid
            JObject newKey2 = (newKey[newKeyCol] == null) ? null : (JObject)newKey[newKeyCol];
            for (var i=0; i<files.Count; i++)
            {
                var col = isMulti ? serverFid + i : serverFid;
                if (fileJson[col] == null)
                {
                    error = "fileJson[" + col + "] is empty.";
                    goto lab_error;
                }

                var key = fileJson[col].ToString();
                var keyIdx = (key == "" && !isMulti)
                    ? 1 : ParseKey(key);

                //case of new key: set key
                if (keyIdx > 0)
                {
                    //var col2 = "f" + levelStr;
                    if (newKey2 == null)
                    {
                        error = "newKey[" + newKeyCol + "] is empty.";
                        goto lab_error;
                    }
                    /*
                    else if(newKey2.Count < i + 1)
                    {
                        error = "newKey[" + newKeyCol + "].Count is less.";
                        goto lab_error;
                    }
                    */
                    key = newKey2["f" + keyIdx].ToString();
                }

                //save file
                var filePath = saveDir + fid + "_" + key + Path.GetExtension(files[i].FileName);
                await SaveFileAsync(files[i], filePath);
            }

            //here
            return true;

        lab_error:
            _Log.Error("_WebFile.cs SaveCrudFiles failed: " + error);
            return false;
        }

        private static int ParseKey(string key)
        {
            return _Str.IsEmpty(key) ? -1 :
                Int32.TryParse(key, out int num) ? num :
                0;
        }

        /// <summary>
        /// (single row)save upload file, & update db(DB save file related path !!)
        /// consider accessibility html page, upload file name should be same as saved file
        /// consider delete file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="row">row to update db</param>
        /// <param name="fileTail">file tail for save</param>
        /// <param name="fileFid">field id for file to save db, if empty then same as fname</param>
        /// <returns></returns>
        //public static void SaveFilesAndSetRows(HttpFileCollectionBase files, JArray rows, string dirSaveServer, string dirSave, string preFile, string kid, string fid)
        public static async Task<JObject> SetRowFileAsync(JObject row, IFormFile file, UploadDto path, string fileTail, string fileFid)
        {
            //consider delete file
            //if (file == null)
            //    return row;

            //get path
            var saveDir = _Str.AddAntiSlash(path.SaveDir);
            var serverPath = (path.ServerPath == "") ? AppDomain.CurrentDomain.BaseDirectory : path.ServerPath;
            serverPath = _Str.AddAntiSlash(serverPath);

            //if (fid == "")
            //    fid = fileName;
            var fid2 = "_" + fileFid;   //underline means delete file !!
            if (row[fid2] != null && row[fid2].ToString() == "1")
            {
                //TODO: delete file, can not know file ext !!
                //File.Delete(saveDir + filePath);

                row[fileFid] = "";  //reset field value
            }
            else if (file != null)
            {
                var filePath = GetFilePath(saveDir, fileTail, file);
                //file.SaveAs(serverPath + "\\" + filePath);
                await SaveFileAsync(file, filePath);
                row[fileFid] = path.PreUrl + filePath.Replace("\\", "/");
            }
            return row;
        }

        /// <summary>
        /// (multi rows)save multi upload files, & update db(DB save file related path !!)
        /// multi files no need to delete file !!
        /// </summary>
        /// <param name="rows">rows for update db</param>
        /// <param name="files">uploaf files array</param>
        /// <param name="path">save file model</param>
        /// <param name="fileTail">file tail to save</param>
        /// <param name="fileFid">column id for save file path</param>
        /// <returns></returns>
        public static async Task SetRowsFilesAsync(JArray rows, List<IFormFile> files, UploadDto path, string fileTail, string fileFid)
        {
            //check input
            if (files == null || files.Count == 0 || rows == null || rows.Count == 0)
                return;

            //get path
            var saveDir = _Str.AddAntiSlash(path.SaveDir);
            var serverPath = (path.ServerPath == "") ? AppDomain.CurrentDomain.BaseDirectory : path.ServerPath;
            serverPath = _Str.AddAntiSlash(serverPath);

            //var key = "";   //pkey value
            foreach (JObject row in rows)
            {
                var fileNo = Convert.ToInt32(row["_fileNo"]);
                if (fileNo < 0)
                    continue;

                var file = files[fileNo];

                //save file
                //var filePath = saveDir + fileTail + key + Path.GetExtension(file.FileName);
                //var filePath = GetFilePath(saveDir, fileTail + key, file);
                var filePath = GetFilePath(saveDir, fileTail, file);
                //file.SaveAs(serverPath + "\\" + filePath);
                await SaveFileAsync(file, filePath);

                //change field value of uploadf file(save related path)
                row[fileFid] = path.PreUrl + filePath.Replace("\\", "/");
            }
        }

        public static FileContentResult EchoImage(string path)
        {
            return new FileContentResult(File.ReadAllBytes(path), "image/jpg");
        }

        #region remark code
        /*
        //save file & return file path for html editor(summernote) !!
        public static string SaveUploadFile(HttpPostedFileBase file, UploadModel path, string fileTail)
        {
            var saveDir = _Str.AddRightSlash(path.SaveDir);
            var serverPath = (path.ServerPath == "") ? AppDomain.CurrentDomain.BaseDirectory : path.ServerPath;
            serverPath = _Str.AddRightSlash(serverPath);

            //var file = Request.Files[0];
            //var filePath = saveDir + fileTail + Path.GetExtension(file.FileName);
            var filePath = GetFilePath(saveDir, fileTail, file);
            file.SaveAs(serverPath + "\\" + filePath);
            return path.PreUrl + filePath.Replace("\\", "/");
        }
        */

        /*
        //傳回目前的分+秒+毫秒, 加在檔名後面, 以防止重複與browser cache
        private static string GetTime()
        {
            return DateTime.Now.ToString("mmssfff");
        }
        */
        #endregion

    }//class
}