using Base.Services;
using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// for Crud List/Query form
    /// </summary>
    public class ReadDto
    {
        /// <summary>
        /// same as DbReadModel.ColList
        /// </summary>
        public string ColList = "";

        /// <summary>
        /// sql string, column select order must same to client side for datatable sorting !!
        /// </summary>
        public string ReadSql = "";

        /// <summary>
        /// sql string for export excel, default to ReadSql.
        /// </summary>
        public string ExportSql = "";

        /// <summary>
        /// sql use square, as: [from],[where],[group],[order]
        /// (TODO: add [whereCond] for client condition !!)
        /// </summary>
        public bool UseSquare = false;

        /// <summary>
        /// default table alias name
        /// </summary>
        public string TableAs = "";

        /// <summary>
        /// (for AuthType=Data only) user fid, default to _Fun.FindUserFid
        /// </summary>
        public string WhereUserFid = _Fun.WhereUserFid;

        /// <summary>
        /// (for AuthType=Data only) dept fid, default to _Fun.FindDeptFid
        /// </summary>
        public string WhereDeptFid = _Fun.WhereDeptFid;

        /// <summary>
        /// for quick search, include table alias, will get like %xx% query
        /// </summary>
        public string[] FindCols;

        /// <summary>
        /// or query for column group, suggest to List more easy !!
        /// </summary>
        public List<List<string>> OrGroups;

        /// <summary>
        /// query condition fields
        /// </summary>
        public QitemDto[] Items;

    }//class
}
