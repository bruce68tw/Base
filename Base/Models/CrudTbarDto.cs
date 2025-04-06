
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
        public string FnOnFind = "_me.crudR.onFind()";

        /// <summary>
        /// fn onclick find2 button
        /// </summary>
        public string FnOnFind2 = "_me.crudR.onFind2()";

        /// <summary>
        /// onCreate
        /// </summary>
        public string FnOnCreate = "_me.crudR.onCreate()";

        /// <summary>
        /// onCreate
        /// </summary>
        public string FnOnExport = "_me.crudR.onExport()";
    }
}