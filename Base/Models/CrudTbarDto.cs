
namespace Base.Models
{
    /// <summary>
    /// CRUD 工具列按鈕使用的前端呼叫字串。
    /// </summary>
    public class CrudTbarDto
    {
        /// <summary>
        /// 查詢按鈕的前端呼叫。
        /// </summary>
        public string FnOnFind = "_me.crudR.onFind()";
        /// <summary>
        /// 第二種查詢按鈕的前端呼叫。
        /// </summary>
        public string FnOnFind2 = "_me.crudR.onFind2()";
        /// <summary>
        /// 新增按鈕的前端呼叫。
        /// </summary>
        public string FnOnCreate = "_me.crudR.onCreate()";
        /// <summary>
        /// 匯出按鈕的前端呼叫。
        /// </summary>
        public string FnOnExport = "_me.crudR.onExport()";
    }
}
