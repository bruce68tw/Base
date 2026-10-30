using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 可包含子選單的功能表項目。
    /// </summary>
    public class MenuDto
    {
        /*
        public MenuDto()
        {
            Items = new List<MenuDto>();
        }
        */
        /// <summary>
        /// 分組顯示的群組名稱。
        /// </summary>
        public string GroupName { get; set; } = "";
        /// <summary>
        /// 作業代碼。
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// 選單顯示名稱。
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 作業連結；空白可表示具有子選單。
        /// </summary>
        public string Url { get; set; } = "";
        /// <summary>
        /// 選單排序值。
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 圖示的 CSS 類別名稱。
        /// </summary>
        public string Icon { get; set; } = "";
        /// <summary>
        /// 子選單清單，預設為空集合。
        /// </summary>
        public List<MenuDto> Items { get; set; } = [];
    }
}
