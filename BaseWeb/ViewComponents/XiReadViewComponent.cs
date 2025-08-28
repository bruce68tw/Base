using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;

namespace BaseWeb.ViewComponents
{
    //readonly field
    public class XiReadViewComponent
    {
        public HtmlString Invoke(XiReadDto dto)
        {
            return new HtmlString(_Input.XiRead(dto));
        }

    }
}
