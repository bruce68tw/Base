using Base.Models;
using System.IO;

namespace Base.Services
{
    //singleton called type
    public class BaseResService : IBaseResService
    {
        private BaseResDto _br;

        //constructor, single locale only
        public BaseResService()
        {
            _br = new BaseResDto(); //initial value
            var file = _Fun.DirRoot + "wwwroot/locale/" + _Fun.Config.DefaultLocale + "/BR.json";
            if (!File.Exists(file))
            {
                _Log.Error("no file: " + file);
                return;
            }

            //set _baseR
            var json = _Json.StrToJson(_File.ToStr(file));
            _Json.CopyToModel(json, _br);
        }

        //get base info
        public BaseResDto GetData()
        {
            return _br;
        }
    }
}
