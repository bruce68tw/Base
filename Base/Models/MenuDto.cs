using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// menu item
    /// </summary>
    public class MenuDto
    {
        /*
        public MenuDto()
        {
            Items = new List<MenuDto>();
        }
        */

        //yves 250320 測試Git checkout and create new branch
        //如果功能表有2層, 使用GroupName分群
        public string GroupName { get; set; } = "";

        //program Id
        public string Code { get; set; } = "";

        //program name
        public string Name { get; set; } = "";

        //if emtpy, means has child menu
        public string Url { get; set; } = "";

        //sort
        public int Sort { get; set; }

        //icon class name
        public string Icon { get; set; } = "";

        //sub menu items
        public List<MenuDto> Items { get; set; } = new();
    }
}
