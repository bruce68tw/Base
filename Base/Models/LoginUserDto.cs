namespace Base.Models
{
    /// <summary>
    /// LDAP 登入查得的基本使用者資料。
    /// </summary>
    public class LoginUserDto
    {
        /// <summary>
        /// 登入帳號，對應 sAMAccountName。
        /// </summary>
        public string Id { get; set; } = "";
        /// <summary>
        /// LDAP 的 sn 屬性值。
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// LDAP 的 mail 屬性值。
        /// </summary>
        public string Email { get; set; } = "";
    }
}
