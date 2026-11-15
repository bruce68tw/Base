using Base.Models;
using Base.Services;

namespace Mongo.Models
{
    /// <summary>
    /// for Crud List/Query form
    /// </summary>
    public class MgoReadDto
    {
        /// <summary>
        /// same as DbReadModel.ColList
        /// </summary>
        public string Table = "";

        /// <summary>
        /// (for AuthType=Row only) user fid, default to _Fun.FindUserFid
        /// </summary>
        public string WhereUserFid = _Fun.UserEqual;

        /// <summary>
        /// (for AuthType=Row only) dept fid, default to _Fun.FindDeptFid
        /// </summary>
        public string WhereDeptFid = _Fun.DeptEqual;

        /// <summary>
        /// query condition fields
        /// </summary>
        public QitemDto[]? Items;

    }//class
}
