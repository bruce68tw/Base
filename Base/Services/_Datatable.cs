using Base.Models;

namespace Base.Services
{
    public static class _Datatable
    {
        //get value in findJson
        public static string GetFindValue(DtDto dt, string fid)
        {
            if (string.IsNullOrEmpty(dt.findJson))
                return "";

            var findJson = _Json.StrToJson(dt.findJson);
            return (findJson[fid] == null)
                ? ""
                : findJson[fid].ToString();
        }

    }//class
}
