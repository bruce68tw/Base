using Base.Models;
using Novell.Directory.Ldap;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    //_Ad(windows only) 改成 _Ldap(跨平台), 使用 Novell.Directory.Ldap 套件
    public class _Ldap
    {
        public static async Task<LoginUserDto?> LoginA(string server, string id, string pwd)
        {
            var cols = server.Split(',');
            var colLen = cols.Length;
            if (colLen < 2)
            {
                _Log.Error("AD Server should has 2/3 args: " + server);
                return null;
            }

            //default 389 port
            var port = (colLen == 3) ? Convert.ToInt32(cols[2]) : 389;
            try
            {
                using var conn = new LdapConnection();
                await conn.ConnectAsync($"{cols[0]}.{cols[1]}", port);
                await conn.BindAsync($"{id}@{cols[1]}", pwd);

                var baseDn = GetBaseDn(cols[1]);
                var filter = $"(sAMAccountName={Escape(id)})";
                var results = await conn.SearchAsync(
                    baseDn,
                    LdapConnection.ScopeSub,
                    filter,
                    ["sn", "mail"],
                    false
                );

                if (!await results.HasMoreAsync())
                    return null;

                var entry = await results.NextAsync();
                var attrSet = entry.GetAttributeSet();
                return new LoginUserDto
                {
                    Id = id,
                    Name = attrSet.GetAttribute("sn")!.StringValue,
                    Email = attrSet.GetAttribute("mail")!.StringValue,
                };
            }
            catch(Exception ex)
            {
                _Log.Error("_Ldap.cs LoginA failed: " + ex.Message);
                return null;
            }
        }

        //LDAP injection 防護
        private static string Escape(string input)
        {
            return input
                .Replace("\\", "\\5c")
                .Replace("*", "\\2a")
                .Replace("(", "\\28")
                .Replace(")", "\\29")
                .Replace("\0", "\\00");
        }

        //AD Base DN（請依公司網域修改）
        private static string GetBaseDn(string server)
        {
            // 範例：corp.company.com → dc=corp,dc=company,dc=com
            var parts = server.Split('.');
            return string.Join(",", parts.Select(p => $"dc={p}"));
        }

    }//class
}
