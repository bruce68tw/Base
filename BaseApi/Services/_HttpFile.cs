using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    /// <summary>
    /// 處理表單上傳檔案的讀取、檢查與儲存，並從既有檔案建立 MVC 檔案回應。
    /// </summary>
    public class _HttpFile
    {
        /// <summary>
        /// 讀取上傳檔的文字內容，可選擇移除 CR 和 LF。讀取失敗時記錄錯誤並回傳空字串。
        /// </summary>
        /// <param name="file">要讀取文字內容的表單上傳檔案。</param>
        /// <param name="removeCarrier">是否從讀出的內容移除 CR 和 LF。</param>
        public static async Task<string> ToStrA(IFormFile file, bool removeCarrier)
        {
            try
            {
                //如果回傳json字串, 則必須轉換換行符號
                using var reader = new StreamReader(file.OpenReadStream());
                var str = await reader.ReadToEndAsync();
                return removeCarrier
                    ? str.Replace("\r", "").Replace("\n", "")
                    : str;
            }
            catch (Exception ex)
            {
                _Log.Error("_HttpFile.cs ToStr() failed: " + ex.Message);
                return "";
            }
        }

        /// <summary>
        /// 檢查上傳檔案是否在大小上限內。
        /// </summary>
        /// <param name="file">要檢查大小的上傳檔案。</param>
        /// <param name="size">檔案大小上限，每單位為 1,048,576 位元組（MiB）。</param>
        /// <returns>檔案大小未超過上限時為 true。</returns>
        public static bool CheckFileSize(IFormFile file, int size)
        {
            return (file.Length <= size * 1024 * 1024);
        }

        /// <summary>
        /// 檢查上傳檔案的副檔名是否符合允許清單。
        /// 清單項目使用不帶句點的副檔名，以逗號分隔，比對時忽略大小寫。
        /// </summary>
        /// <param name="file">要檢查副檔名的上傳檔案。</param>
        /// <param name="exts">允許的副檔名清單，以逗號分隔，項目不帶句點。</param>
        /// <returns>副檔名在允許清單內時為 true。</returns>
        public static bool CheckFileExt(IFormFile file, string exts)
        {
            var ext = Path.GetExtension(file.FileName).Replace(".", "").ToLower();
            return ("," + exts.ToLower() + ",").Contains("," + ext + ",");
        }

        /// <summary>
        /// 回傳不含句點的小寫副檔名。輸入為 null 或沒有副檔名時回傳空字串。
        /// </summary>
        /// <param name="file">要取得副檔名的上傳檔案，可為 null。</param>
        public static string GetFileExt(IFormFile? file)
        {
            return (file == null)
                ? ""
                : Path.GetExtension(file.FileName).Replace(".", "").ToLower();
        }

        /// <summary>
        /// 依指定路徑建立目錄並寫入檔案。已有同名檔會被覆寫，失敗時記錄錯誤並回傳 false。
        /// </summary>
        /// <param name="file">要複製到磁碟的表單上傳檔案。</param>
        /// <param name="filePath">包含檔名的儲存路徑。同名檔案會被覆寫。</param>
        /// <returns>寫入成功時為 true。</returns>
        public static async Task<bool> SaveFileA(IFormFile file, string filePath)
        {
            try
            {
                //make dir first
                var dir = _File.PathToDir(filePath);
                _File.MakeDir(dir);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                return true;
            }
            catch (Exception ex)
            {
                await _Log.ErrorRootA("_HttpFile.cs SaveFileA() failed: " + ex.Message);
                return false;
            }
        }

        /*
        /// <summary>
        /// save upload file(HttpPostedFileBase)
        /// </summary>
        /// <param name="toDir">save dir path</param>
        /// <param name="file"></param>
        /// <param name="savePath">return file http path</param>
        /// <returns></returns>
        public static async Task<string> SaveFileAsync(string toDir, IFormFile file)
        {
            try
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine(toDir, Path.GetFileName(file.FileName)); //硬碟路徑
                    if (File.Exists(filePath))
                    {
                        var fileName = _Guid.Encode(new Guid()) + Path.GetExtension(file.FileName);
                        toDir = Path.Combine(toDir, fileName);
                    }
                    //file.SaveAs(savePath);
                    await _WebFile.SaveFileAsync(file, filePath);

                    filePath = toDir + Path.GetFileName(file.FileName); //http path
                    return filePath;
                }
                else
                {
                    return "";
                }                    
            }
            catch
            {
                throw;
            }
        }          
        */

        /// <summary>
        /// 以新產生的識別值命名檔案，並保留原副檔名。
        /// </summary>
        /// <param name="file">提供檔案內容與原始副檔名的上傳檔案。</param>
        /// <param name="saveDir">儲存新命名檔案的目錄。</param>
        /// <returns>儲存後的檔名，寫入失敗時為空字串。</returns>
        public static async Task<string> SaveFileByNewIdA(IFormFile file, string saveDir)
        {
            //saveDir = _Str.EmptyToValue(saveDir, _Web.DirWeb + "image");
            var fileName = _Str.NewId() + Path.GetExtension(file.FileName);
            var path = _Str.AddDirSep(saveDir) + fileName;
            return (await SaveFileA(file, path))
                ? fileName : "";
        }

        //return related path for save file
        //tail has unique key for file name, prevent duplicated & browser cache (yyyyMMddHHmmss)
        private static string GetFilePath(string dir, string fileTail, IFormFile file)
        {
            //DateTime.Now.ToString("ddmmfff")
            return dir + Path.GetFileNameWithoutExtension(file.FileName) +
                "_" + fileTail + _Str.NewId() + Path.GetExtension(file.FileName);
        }

        /// <summary>
        /// 將單一 CRUD 附件存入草稿目錄。未提供檔案時回傳 true。
        /// </summary>
        /// <param name="inputJson">包含 _fileJson 附件與資料列鍵值對應的 CRUD 輸入。</param>
        /// <param name="newKey">CRUD 建立資料後回傳的新主鍵對照，供尚未取得主鍵的資料列使用。</param>
        /// <param name="file">要儲存的表單上傳檔案。</param>
        /// <param name="serverFid">附件欄位識別值，格式為 txx_fid。</param>
        /// <param name="preFile">加入附件檔名開頭的前綴，可為空字串。</param>
        public static async Task<bool> SaveDraftFileA(JObject inputJson, JObject newKey, IFormFile file, 
            string serverFid, string preFile)
        {
            if (file == null) return true;
            return await SaveDraftFilesA(inputJson, newKey, [file], serverFid, false, preFile);
        }

        /// <summary>
        /// 依 CRUD 附件與資料列鍵值對應，將檔案存入草稿目錄。
        /// </summary>
        /// <param name="inputJson">包含 _fileJson 附件與資料列鍵值對應的 CRUD 輸入。</param>
        /// <param name="newKey">CRUD 建立資料後回傳的新主鍵對照，供尚未取得主鍵的資料列使用。</param>
        /// <param name="files">要依 _fileJson 對應儲存的上傳檔案清單。</param>
        /// <param name="serverFid">附件欄位識別值，格式為 txx_fid。</param>
        /// <param name="isMulti">是否依清單索引在 serverFid 後附加序號來查找附件對應。</param>
        /// <param name="preFile">加入附件檔名開頭的前綴，可為空字串。</param>
        public static async Task<bool> SaveDraftFilesA(JObject inputJson, JObject newKey, List<IFormFile> files, 
            string serverFid, bool isMulti, string preFile)
        {
            return await SaveCrudFilesA(inputJson, newKey, _Fun.DirDraft, files, serverFid, isMulti, preFile);
        }

        /// <summary>
        /// 以單檔對應規則儲存附件。未提供檔案時回傳 true。
        /// </summary>
        /// <param name="inputJson">包含 _fileJson 附件與資料列鍵值對應的 CRUD 輸入。</param>
        /// <param name="newKey">CRUD 建立資料後回傳的新主鍵對照，供尚未取得主鍵的資料列使用。</param>
        /// <param name="saveDir">儲存附件的目錄。</param>
        /// <param name="file">要儲存的表單上傳檔案。</param>
        /// <param name="serverFid">附件欄位識別值，格式為 txx_fid。</param>
        public static async Task<bool> SaveCrudFileA(JObject inputJson, JObject newKey, string saveDir, 
            IFormFile file, string serverFid)
        {
            if (file == null) return true;
            return await SaveCrudFilesA(inputJson, newKey, saveDir, [file], serverFid, false);
        }

        /// <summary>
        /// 依 CRUD 資料列主鍵儲存多個附件。
        /// 以 _fileJson 與 newKey 對應檔案和資料列主鍵。逐檔寫入結果未納入回傳值。
        /// </summary>
        /// <param name="inputJson">包含 _fileJson 附件與資料列鍵值對應的 CRUD 輸入。</param>
        /// <param name="newKey">CRUD 建立資料後回傳的新主鍵對照，供尚未取得主鍵的資料列使用。</param>
        /// <param name="saveDir">儲存附件的目錄。</param>
        /// <param name="files">要依 _fileJson 對應儲存的上傳檔案清單。</param>
        /// <param name="serverFid">附件欄位識別值，格式為 txx_fid。</param>
        /// <param name="isMulti">是否依清單索引在 serverFid 後附加序號來查找附件對應。</param>
        /// <param name="preFile">加入附件檔名開頭的前綴，可為空字串。</param>
        public static async Task<bool> SaveCrudFilesA(JObject inputJson, JObject newKey, string saveDir, 
            List<IFormFile> files, string serverFid, bool isMulti = true, string preFile =  "")
        {
            if (_List.IsEmpty(files)) return true;

            string error;
            if (inputJson[_Fun2.FileJson] == null)
            {
                error = "inputJson[" + _Fun2.FileJson + "] is empty.";
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
            saveDir = _Str.AddDirSep(saveDir);
            var fileJson = (JObject)inputJson[_Fun2.FileJson]!;   //file vs pkey
            var newKeyCol = values[0];  //equals to 't' + levelStr, ex:t0
            var fid = values[1];        //file fid, ex:ImageFile
            JObject? newKey2 = (newKey[newKeyCol] == null) ? null : (JObject)newKey[newKeyCol]!;
            for (var i=0; i<files.Count; i++)
            {
                //多筆的檔案 serverFid後面會加序號
                var col = isMulti ? serverFid + i : serverFid;
                if (fileJson![col] == null)
                {
                    error = "fileJson[" + col + "] is empty.";
                    goto lab_error;
                }

                var key = fileJson[col]!.ToString();
                var keyIdx = (key == "" && !isMulti)
                    ? 1 : _Num.KeyToUpRowNo(key);

                //get key if need, case of new key: set key
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
                    key = newKey2["f" + keyIdx]!.ToString();
                }

                //todo: 判斷 SaveFileA 回傳結果
                //save file: 欄位名稱_PKey + .副檔名(有rename)
                var filePath = saveDir + preFile + fid + "_" + key + _File.UpExtRename(Path.GetExtension(files[i].FileName));
                await SaveFileA(files[i], filePath);
            }

            //case ok ok
            return true;

        lab_error:
            await _Log.ErrorRootA("_HttpFile.cs SaveCrudFilesA failed: " + error);
            return false;
        }

        /// <summary>
        /// 儲存時保留原檔名的主體，再附加尾碼及識別值。
        /// 現行方法儲存檔案並更新傳入的 JSON 資料列。刪除旗標只清空欄位，實際刪檔仍未實作。
        /// </summary>
        /// <param name="file">要儲存的上傳檔案，未提供時只處理清空欄位旗標。</param>
        /// <param name="row">要原地更新附件路徑的 JSON 物件。</param>
        /// <param name="fileTail">附加在原檔名後、系統產生的識別值之前的字串。</param>
        /// <param name="fileFid">用來寫回附件路徑的 JSON 欄位名。</param>
        /// <param name="path">提供儲存目錄及回填路徑前綴的設定。</param>
        public static async Task<JObject> SetRowFileA(JObject row, IFormFile file, UploadDto path, string fileTail, string fileFid)
        {
            //consider delete file
            //if (file == null)
            //    return row;

            //get path
            var saveDir = _Str.AddDirSep(path.SaveDir);
            var serverPath = (path.ServerPath == "") 
                ? AppDomain.CurrentDomain.BaseDirectory : path.ServerPath;
            serverPath = _Str.AddDirSep(serverPath);

            //if (fid == "")
            //    fid = fileName;
            var fid2 = "_" + fileFid;   //underline means delete file !!
            if (row[fid2] != null && row[fid2]!.ToString() == "1")
            {
                //TODO: delete file, can not know file ext !!
                //File.Delete(saveDir + filePath);

                row[fileFid] = "";  //reset field value
            }
            else if (file != null)
            {
                var filePath = GetFilePath(saveDir, fileTail, file);
                //file.SaveAs(serverPath + "\\" + filePath);
                await SaveFileA(file, filePath);
                row[fileFid] = path.PreUrl + filePath.Replace("\\", "/");
            }
            return row;
        }

        /// <summary>
        /// 現行方法依各列的 _fileNo 選取上傳檔並更新 JSON 欄位。資料庫寫入由呼叫端處理。
        /// </summary>
        /// <param name="rows">要原地更新附件路徑的 JSON 資料列。</param>
        /// <param name="files">上傳檔案清單，各列以 _fileNo 作為索引。</param>
        /// <param name="path">提供儲存目錄及回填路徑前綴的設定。</param>
        /// <param name="fileTail">附加在原檔名後、系統產生的識別值之前的字串。</param>
        /// <param name="fileFid">用來寫回附件路徑的 JSON 欄位名。</param>
        public static async Task SetRowsFilesA(JArray rows, List<IFormFile> files, UploadDto path, string fileTail, string fileFid)
        {
            //check input
            if (files == null || files.Count == 0 || rows == null || rows.Count == 0)
                return;

            //get path
            var saveDir = _Str.AddDirSep(path.SaveDir);
            var serverPath = (path.ServerPath == "") 
                ? AppDomain.CurrentDomain.BaseDirectory : path.ServerPath;
            serverPath = _Str.AddDirSep(serverPath);

            //var key = "";   //pkey value
            foreach (JObject row in rows)
            {
                var fileNo = Convert.ToInt32(row["_fileNo"]);
                if (fileNo < 0) continue;

                var file = files[fileNo];
                //save file
                //var filePath = saveDir + fileTail + key + Path.GetExtension(file.FileName);
                //var filePath = GetFilePath(saveDir, fileTail + key, file);
                var filePath = GetFilePath(saveDir, fileTail, file);
                //file.SaveAs(serverPath + "\\" + filePath);
                await SaveFileA(file, filePath);

                //change field value of uploadf file(save related path)
                row[fileFid] = path.PreUrl + filePath.Replace("\\", "/");
            }
        }

        /// <summary>
        /// 依實際的上傳檔路徑建立檔案結果。找不到圖片時使用預設圖片，其他檔案找不到時回傳 null。
        /// </summary>
        /// <param name="path">檔案路徑，方法會據此取得實際上傳檔路徑。</param>
        /// <param name="downFileName">回應使用的檔名，空字串時沿用原路徑的檔名。</param>
        public static FileResult? ViewFile(string path, string downFileName = "")
        {
            var ext = _File.GetFileExt(path);
            var upPath = _File.PathExtToUp(path, ext);  //上傳檔案會改副檔名for資安
            var hasFile = File.Exists(upPath);
            //圖檔直接開啟, 不考慮 isView
            var contentType = _Http.ExtToContentType(ext, false);
            var isImage = contentType.Contains("image");
            if (!hasFile)
            {
                if (isImage)
                {
                    upPath = _Fun.NoImagePath;
                    contentType = "image/jpg";
                }
                else
                {
                    return null;
                }
            }

            return new PhysicalFileResult(upPath, contentType)
            {
                FileDownloadName = _Str.IsEmpty(downFileName)
                    ? _File.GetFileName(path)
                    : downFileName
            };
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