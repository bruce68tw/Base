using Base.Models;
using Newtonsoft.Json.Linq;

namespace Base.Services
{
    abstract public class MyEdit
    {
        public string Ctrl;

        public MyEdit(string ctrl) 
        {
            Ctrl = ctrl; 
        }

        abstract public EditDto GetDto();

        //derived class override !!
        public CrudEdit Service()
        {
            return new CrudEdit(Ctrl, GetDto());
        }

        public JObject GetUpdateJson(string key)
        {
            return Service().GetUpdateJson(key);
        }

        public JObject GetViewJson(string key)
        {
            return Service().GetViewJson(key);
        }

        public ResultDto Create(JObject json)
        {
            return Service().Create(json);
        }

        public ResultDto Update(string key, JObject json)
        {
            return Service().Update(key, json);
        }

        public ResultDto Delete(string key)
        {
            return Service().Delete(key);
        }

    }//class
}
