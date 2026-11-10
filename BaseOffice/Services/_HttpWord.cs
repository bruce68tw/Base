using Base.Models;
using BaseApi.Services;
using BaseOffice.Models;

namespace BaseOffice.Services
{
    /// <summary>以 Open XML 將單筆或多筆資料填入 Word 範本並下載。 (docx) (not in _Http.cs) (need cancel spell check at word editor !!) (named m_Schools) (use insertAfter)</summary>
    public static class _HttpWord
    {
        // 現行套表由 WordSetSvc 使用 Open XML 完成。此類別將產生的 DOCX 串流寫入目前的 HTTP 回應。
        /// <summary>以一筆資料填入 Word 範本並寫入目前 HTTP 回應。無法產生串流時回傳 false。</summary>
        /// <param name="tplPath">要讀取的範本檔案路徑</param>
        /// <param name="fileName">檔案名稱，必要時包含副檔名</param>
        /// <param name="images">要依範本標記填入的圖片清單，可為 null</param>
        /// <param name="row">提供範本標記對應值的單筆資料。</param>
        /// <returns>已輸出時為 true，無法建立文件串流時為 false。</returns>
        public static async Task<bool> OutputTplRowA(string tplPath, string fileName, dynamic row, List<WordImageDto>? images = null)
        {
            var ms = _Word.TplRowToMs(tplPath, row, images);
            if (ms == null) return false;

            // 匯出時文件已儲存並關閉，ExportByStreamA 會從串流開頭寫入回應。
            await _Fun2.ExportByStreamA(ms, fileName);
            return true;
        }

        /// <summary>以多筆資料組成一份 Word 文件並寫入目前 HTTP 回應。無法產生串流時回傳 false。</summary>
        /// <param name="tplPath">要讀取的範本檔案路徑</param>
        /// <param name="fileName">檔案名稱，必要時包含副檔名</param>
        /// <param name="rows">依輸出順序套用 Word 範本的資料列。</param>
        /// <param name="images">要依範本標記填入的圖片清單，可為 null</param>
        /// <returns>已輸出時為 true，無法建立文件串流時為 false。</returns>
        public static async Task<bool> OutputTplRowsA(string tplPath, string fileName, List<dynamic> rows, List<WordImageDto>? images = null)
        {
            // 多筆資料共用同一份範本，組成單一 DOCX 後下載。
            var ms = _Word.TplRowsToMs(tplPath, rows, images);
            if (ms == null) return false;

            await _Fun2.ExportByStreamA(ms, fileName);
            return true;
        }

        /*
        //test
        public static bool OutputTplA2()
        {
            var oldWordPath = "C:\\_project\\HrAdm\\_template\\zh-TW\\UserExt.docx";
            var newWordPath = "C:\\_project\\HrAdm\\_template\\zh-TW\\UserExt_new.docx";
            var newImagePath = "C:\\_project\\HrAdm\\_upload\\UserExt\\PhotoFile_Alex.png";
            _WordSet.AddImage(oldWordPath, newWordPath, newImagePath);
            return false;
        }
        */
    }//class
}
