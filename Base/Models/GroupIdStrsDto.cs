using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 具有群組標題的選項集合。
    /// </summary>
    public class GroupIdStrsDto
    {
        /// <summary>
        /// 選項群組顯示文字。
        /// </summary>
        public string Group { get; set; } = "";
        /// <summary>
        /// 群組內的識別碼與顯示文字清單，預設未初始化。
        /// </summary>
        public List<IdStrDto> Items { get; set; } = null!;
    }
}
