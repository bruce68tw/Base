using Base.Models;
using System;
using System.DirectoryServices;

namespace Base.Services
{
    public class _Ad
    {
        //login Ad server
        //see: http://www.itread01.com/articles/1478571605.html
        public static AdUserDto? Login(string server, string id, string pwd)
        {
            var entry = new DirectoryEntry(server, id, pwd);
            try
            {
                //object obj = entry.NativeObject;
                var search = new DirectorySearcher(entry)
                {
                    Filter = string.Format("(SAMAccountName={0})", id)
                };

                // 指定需要傳回的屬性
                search.PropertiesToLoad.Add("sn");      //姓名
                search.PropertiesToLoad.Add("mail");    //電子郵件

                SearchResult result = search.FindOne();
                return (result == null)
                    ? null
                    : new AdUserDto()
                    {
                        Id = id,
                        Name = result.Properties["sn"][0]!.ToString(),
                        Email = result.Properties["mail"][0]!.ToString(),
                    };                
            }
            catch
            {
                return null;
            }
            finally
            {
                entry.Close();
            }
        }

    }//class
}
