using System;

namespace Base.Services
{
    /// <summary>
    /// GUID 字串及 22 字元的 URL 友善 Base64 表示轉換。
    /// </summary>
    public static class _Guid
    {
        /// <summary>
        /// 建立含連字號的標準 GUID 字串。
        /// </summary>
        public static string NewStr()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 將 GUID 或合法 GUID 字串編成不含尾端等號的 22 字元字串。
        /// </summary>
        public static string Encode(string guidText)
        {
            var guid = new Guid(guidText);
            return Encode(guid);
        }

        /// <summary>
        /// 將 GUID 或合法 GUID 字串編成不含尾端等號的 22 字元字串。
        /// </summary>
        public static string Encode(Guid guid)
        {
            var code = Convert.ToBase64String(guid.ToByteArray());
            code = code.Replace("/", "_");
            code = code.Replace("+", "-");
            return code[..22];
        }

        /// <summary>
        /// 還原 Encode 產生的字串；格式錯誤會拋出例外。
        /// </summary>
        public static Guid Decode(string encode)
        {
            encode = encode.Replace("_", "/");
            encode = encode.Replace("-", "+");
            var buffer = Convert.FromBase64String(encode + "==");
            return new Guid(buffer);
        }

    }//class
}
