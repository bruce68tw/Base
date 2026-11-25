using BaseApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.Services
{
    //project service
    public static class _Fun3
    {

        public static FileResult? ViewFile(string dir, string fid, string key, string ext)
        {
            var path = $"{dir}{fid}_{key}.{ext}";
            return _HttpFile.ViewFile(path, $"{fid}.{ext}");
        }

    }//class
}
