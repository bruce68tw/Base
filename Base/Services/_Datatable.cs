using Base.Models;

namespace Base.Services
{
    /// <summary>
    /// DataTables 查詢輸入的欄位取值工具。
    /// </summary>
    public static class _Datatable
    {
        /// <summary>
        /// 從 findJson 取得欄位字串；JSON 無效、為空或欄位缺失時回傳空字串。
        /// </summary>
        public static string GetFindValue(DtDto dt, string fid)
        {
            if (dt.findJson == null) return "";
            var json = _Str.ToJson(dt.findJson);
            return (json == null)
                ? "" : _Json.NullToEmpty(json, fid);

            //return _Json.NullFieldToEmpty(_Str.ToJson(dt.findJson), fid);
            //return (findJson == null)
            //    ? _Object.NullToEmpty(findJson![fid])
            //    : "";
        }

    }//class
}
