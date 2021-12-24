using Base.Models;

namespace Base.Services
{
    public static class _Datatable
    {
        //get value in findJson
        public static string GetFindValue(DtDto dt, string fid)
        {
            if (_Str.IsEmpty(dt.findJson))
                return "";

            var findJson = _Str.ToJson(dt.findJson);
            return (findJson[fid] == null)
                ? ""
                : findJson[fid].ToString();
        }

    }//class
}
