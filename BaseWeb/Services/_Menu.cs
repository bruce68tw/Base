using Base.Models;
using Base.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseWeb.Services
{

    //功能表
    public static class _Menu
    {
        //public const string XdRequired = "required";     //for input ??

        /// <summary>
        /// 加上menu群組
        /// </summary>
        /// <returns></returns>
        public static async Task<List<MenuDto>> GetMenuA(bool superMode = false)
        {
            var sql = @"
select 
    p.Code, p.Sort,
	p.Name, p.Url,
	GroupName=c.Name
from dbo.XpProg p
join dbo.XpCode c on c.Type='MenuGroup' and p.MenuGroup=c.Value
where p.Status=1
order by c.Sort, p.Sort
";
            var allMenus = await _Db.GetModelsA<MenuDto>(sql);
            if (allMenus == null)
                return [];

            //分群組(2層)
            var menus = allMenus!.GroupBy(a => a.GroupName)
                .Select(a => new MenuDto
                {
                    Name = a.Key,
                    Items = a.Select(b => new MenuDto
                    {
                        Code = b.Code,
                        Name = b.Name,
                        Url = b.Url,
                        Sort = b.Sort,
                    }).ToList()
                })
                .ToList();

            if (!superMode)
                _Auth.FilterMenu(menus);
            return menus;
        }

    }//class
}
