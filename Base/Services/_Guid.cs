using System;

namespace Base.Services
{
    //
    public static class _Guid
    {
        public static string NewStr()
        {
            return Guid.NewGuid().ToString();
        }

        public static string Encode(string guidText)
        {
            var guid = new Guid(guidText);
            return Encode(guid);
        }

        public static string Encode(Guid guid)
        {
            var code = Convert.ToBase64String(guid.ToByteArray());
            code = code.Replace("/", "_");
            code = code.Replace("+", "-");
            return code.Substring(0, 22);
        }

        public static Guid Decode(string encode)
        {
            encode = encode.Replace("_", "/");
            encode = encode.Replace("-", "+");
            var buffer = Convert.FromBase64String(encode + "==");
            return new Guid(buffer);
        }

    }//class
}
