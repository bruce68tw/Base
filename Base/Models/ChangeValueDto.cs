
namespace Base.Models
{
    //for edit ofr
    public class ChangeValueDto
    {
        /// <summary>
        /// key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// old value
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// new value
        /// </summary>
        public string NewValue { get; set; }
    }
}