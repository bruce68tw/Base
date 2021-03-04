
namespace Base.Models
{
    /// <summary>
    /// crud toolbar model
    /// </summary>
    public class CrudTbarDto
    {
        /// <summary>
        /// fn onclick find button
        /// </summary>
        public string FnOnFind = "_crud.onFind()";

        /// <summary>
        /// fn onclick find2 button
        /// </summary>
        public string FnOnFind2 = "_crud.onCreate()";

        /// <summary>
        /// onCreate
        /// </summary>
        public string FnOnCreate = "_crud.onCreate()";

        /// <summary>
        /// onCreate
        /// </summary>
        public string FnOnExport = "_crud.onExport()";
    }
}