using System.Collections.Generic;

namespace Base.Models
{
    public class GroupIdStrsDto
    {
        /// <summary>
        /// group text
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// for select options
        /// </summary>
        public List<IdStrDto> Items { get; set; }
    }
}