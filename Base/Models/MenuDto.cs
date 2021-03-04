using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// menu item
    /// </summary>
    public class MenuDto
    {
        //program Id
        public string Id;

        //program name
        public string Name;

        //if emtpy, means has child menu
        public string Url;

        //sort
        public byte Sort;

        //icon
        public string Icon;

        //sub ment items
        public List<MenuDto> Items = new List<MenuDto>();
    }
}
