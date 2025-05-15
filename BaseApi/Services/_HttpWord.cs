using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Routing.Template;
using NPOI.SS.Formula;
using NPOI.XWPF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// export file by template and row
        /// </summary>
        /// <param name="tplPath">tpl path</param>
        /// <param name="fileName">export file name</param>
        /// <param name="row">可為JObject或Model</param>
        /// <param name="childs">可為JArray或List<Model>, IEnumerable for anonymous type</param>
        /// <param name="images"></param>
        /// <returns>error msg if any</returns>
        public static async Task<bool> ExportByTplRowA(string tplPath, string fileName, dynamic row,
            List<IEnumerable<dynamic>>? childs = null, List<WordImageDto>? images = null)
        {
            //1.check template file
            if (!File.Exists(tplPath))
            {
                await _Log.ErrorRootA($"_HttpWord.cs ExportByTplRow() no tpl file ({tplPath})");
                return false;
            }

            var ms = MsByTplRow(tplPath, row, childs, images);
            await _FunApi.ExportByStreamA(ms, fileName);
            return true;
        }

        /// <summary>
        /// word to memoryStream for convert pdf
        /// </summary>
        /// <param name="tplPath"></param>
        /// <param name="fileName"></param>
        /// <param name="row"></param>
        /// <param name="childs"></param>
        /// <param name="images"></param>
        /// <returns></returns>
        public static MemoryStream MsByTplRow(string tplPath, dynamic row,
            List<IEnumerable<dynamic>>? childs = null, List<WordImageDto>? images = null)
        {
            //2.read word template file to docx
            using var fs = new FileStream(tplPath, FileMode.Open, FileAccess.Read);
            var docx = new XWPFDocument(fs);
            /*
            var ms = new MemoryStream();
            var tplBytes = File.ReadAllBytes(tplPath);
            ms.Write(tplBytes, 0, tplBytes.Length);
            ms.Seek(0, SeekOrigin.Begin); // 重置指標
            */

            //3.binding stream && docx
            var fileStr = "";

            //initial 
            var wordSet = new WordSetSvc(docx);
            var mainStr = wordSet.GetMainPartStr();

            //fill main json row

            //4.add images first
            if (images != null)
                mainStr = wordSet.AddImages(mainStr, images);

            //get word body start/end pos
            var bodyTpl = wordSet.GetBodyTpl(mainStr);

            #region 5.fill row && childs rows
            var hasChild = (childs != null && childs.Count > 0);
            if (hasChild)
            {
                var childLen = childs!.Count;
                int oldStart = 0, oldEnd = 0;
                for (var ci = 0; ci < childLen; ci++)
                {
                    // 快速查找含有 [x!] 的範本列, base 0
                    var table = docx.Tables
                        .FirstOrDefault(t => t.Rows.Any(r => r.GetTableCells().Any(c => c.GetText().Contains($"[{ci}!]"))));
                    if (table == null) continue;

                    var rowTpl = wordSet.GetRowTpl(bodyTpl.TplStr, ci);
                    if (rowTpl.TplStr == "") continue;

                    //set head or add left string of rows
                    if (ci == 0)
                        _Word.DocxFillRow(docx, row);
                    else
                        bodyTpl.TplStr[(oldEnd + 1)..rowTpl.StartPos];

                    //add middle
                    fileStr += _Word.TplFillRows(rowTpl.TplStr, childs[ci]);

                    //add tail
                    if (ci == childLen - 1)
                        _Word.DocxFillRow(docx, row);

                    //set old pos
                    oldStart = rowTpl.StartPos;
                    oldEnd = rowTpl.EndPos;
                }//for childs

                //set word file string
                fileStr = mainStr[..bodyTpl.StartPos] +
                    fileStr +
                    mainStr[(bodyTpl.EndPos + 1)..];
            }
            else
            {
                _Word.DocxFillRow(docx, row);
            }
            #endregion

            //write into docx
            wordSet.SetMainPartStr(fileStr);

            return ms;
        }

    }//class
}
