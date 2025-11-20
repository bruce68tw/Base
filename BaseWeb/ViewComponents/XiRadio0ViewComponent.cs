using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

//只有一個 radio 按鈕, 自行組合
namespace BaseWeb.ViewComponents
{
    public class XiRadio0ViewComponent : ViewComponent
    {
        public HtmlString Invoke(string fid, string value, string label)
        {
            return new HtmlString(_Input.XiRadio0(fid, value, label));
        }

    }//class
}
