namespace Base.Models
{
    /// <summary>
    /// AdUserDto -> LoginUserDto
    /// summary data
    /// </summary>
    public class LoginUserDto
    {
        //(SamAccountName)
        public string Id { get; set; } = "";

        //(sn)
        public string Name { get; set; } = "";

        //(mail)
        public string Email { get; set; } = "";
    }
}
