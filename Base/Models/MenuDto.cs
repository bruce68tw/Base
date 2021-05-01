using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// menu item
    /// </summary>
    public class MenuDto
    {
        public MenuDto()
        {
            Items = new List<MenuDto>();
        }

        //program Id
        public string Code { get; set; }

        //program name
        public string Name { get; set; }

        //if emtpy, means has child menu
        public string Url { get; set; }

        //sort
        public byte Sort { get; set; }

        //icon
        public string Icon { get; set; }

        //sub ment items
        public List<MenuDto> Items { get; set; }
    }
}
