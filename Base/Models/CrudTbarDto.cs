
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
        public string FnOnFind = "_crudR.onFind()";

        /// <summary>
        /// fn onclick find2 button
        /// </summary>
        public string FnOnFind2 = "_crudR.onFind2()";

        /// <summary>
        /// onCreate
        /// </summary>
        public string FnOnCreate = "_crudR.onCreate()";

        /// <summary>
        /// onCreate
        /// </summary>
        public string FnOnExport = "_crudR.onExport()";
    }
}