namespace Base.Models
{
    /// <summary>
    /// 共用服務使用的目前使用者、部門與授權資料；由 IBaseUserSvc 提供。
    /// </summary>
    public class BaseUserDto
    {
        //login or not
        //public bool IsLogin;
        /// <summary>
        /// 是否具有密碼，預設 false。
        /// </summary>
        public bool HasPwd = false;
        /// <summary>
        /// 使用者語系代碼。
        /// </summary>
        public string Locale = "";
        /// <summary>
        /// 目前使用者識別碼；資料列權限查詢會檢查是否為空。
        /// </summary>
        public string UserId = "";
        /// <summary>
        /// 使用者顯示名稱。
        /// </summary>
        public string UserName = "";
        /// <summary>
        /// 所屬部門識別碼。
        /// </summary>
        public string DeptId = "";
        /// <summary>
        /// 部門顯示名稱。
        /// </summary>
        public string DeptName = "";
        /// <summary>
        /// 由應用定義的使用者分類。
        /// </summary>
        public string UserType = "";
        /// <summary>
        /// 登入流程帶入的第一個擴充欄位。
        /// </summary>
        public string ExtCol = "";
        /// <summary>
        /// 登入流程帶入的第二個擴充欄位。
        /// </summary>
        public string ExtCol2 = "";
        /// <summary>
        /// 登入流程帶入的第三個擴充欄位。
        /// </summary>
        public string ExtCol3 = "";

        //diffence hours to GMT for Db saving
        //public double HourDiff = 0;
        /// <summary>
        /// 作業權限字串；控制器模式為逗號包圍的作業清單，功能模式另含冒號與權限數字。
        /// </summary>
        public string ProgAuthStrs = "";

    }//class
}
