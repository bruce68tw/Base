using Base.Models;

namespace Base.Services
{
    public static class _Datatable
    {
        //get value in findJson
        public static string GetFindValue(DtDto dt, string fid)
        {
            if (dt.findJson == null) return "";
            var json = _Str.ToJson(dt.findJson);
            return (json == null)
                ? "" : _Json.NullFieldToEmpty(json, fid);

            //return _Json.NullFieldToEmpty(_Str.ToJson(dt.findJson), fid);
            //return (findJson == null)
            //    ? _Object.NullToEmpty(findJson![fid])
            //    : "";
        }

    }//class
}
