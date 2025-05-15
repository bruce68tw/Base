using Base.Models;
using Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    /// <summary>
    /// use NPOI output word(docx), cause duplicate, put here(not in _Http.cs)
    /// temlate has 2 type data: (need cancel spell check at word editor !!)
    ///   1.single row: ex:[StartDate], fixed rows could use copy/paste then change font !!
    ///   //2.multi rows: ex:[m_Schools], must put bookmark in docx(named m_Schools)
    /// steps :
    ///   1.do multiple rows first: insert table into docx bookmark (use insertAfter)
    ///   2.then do single row: output docx to string, then find/replace
    /// note:
    ///   1.when edit word template file, word will cut your word auto, must keyin your word at one time !!
    /// </summary>
    public static class _HttpWord
    {
        /// <summary>
        /// ExportByTplRowA -> OutputTplA
        /// export file by template and row
        /// </summary>
        /// <param name="tplPath">tpl path</param>
        /// <param name="fileName">export file name</param>
        /// <param name="row">可為JObject或Model</param>
        /// <param name="childs">可為JArray或List<Model>, IEnumerable for anonymous type</param>
        /// <param name="images"></param>
        /// <returns>error msg if any</returns>
        public static async Task<bool> OutputTplA(string tplPath, string fileName, dynamic row,
            List<IEnumerable<dynamic>>? childs = null, List<WordImageDto>? images = null)
        {
            var ms = await _Word.TplToMsA(tplPath, row, childs, images);
            if (ms == null) return false;

            await _FunApi.ExportByStreamA(ms, fileName);
            return true;
        }

    }//class
}
