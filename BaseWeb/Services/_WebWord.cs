using Base.Models;
using Base.Services;
using BaseApi.Services;
using DocumentFormat.OpenXml.Packaging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BaseWeb.Services
{

    /// <summary>
    /// use OpenXml output word(docx), cause duplicate, put here(not in _Http.cs)
    /// temlate has 2 type data: (need cancel spell check at word editor !!)
    ///   1.single row: ex:[StartDate], fixed rows could use copy/paste then change font !!
    ///   //2.multi rows: ex:[m_Schools], must put bookmark in docx(named m_Schools)
    /// steps :
    ///   1.do multiple rows first: insert table into docx bookmark (use insertAfter)
    ///   2.then do single row: output docx to string, then find/replace
    /// note:
    ///   1.when edit word template file, word will cut your word auto, must keyin your word at one time !!
    /// </summary>
    public static class _WebWord
    {
        /// <summary>
        /// output word file(docx), use microsoft openXml 
        /// note: use find/replace to fill field value, image need to use same way, or will get wrong !!
        /// see: https://msdn.microsoft.com/en-us/library/ee945362(v=office.11).aspx
        /// </summary>
        /// <param name="row">data source</param>
        /// <param name="tplPath">template path</param>
        /// <param name="fileName">default output file name</param>
        /// <param name="images">image data, four fields for one imge(path,width,height,tag)</param>
        public static void zz_EchoByTplRow(JObject row, string tplPath, string fileName, List<WordImageDto> images = null)
        {
            /* 
            //TODO: pending
            //check template file
            if (!File.Exists(tplPath))
            {
                _Log.Error("_Word.ExportByTplFile() error: no file " + tplPath);
                return;
            }

            //declare stream & load tmpleta(with) into stream
            var stream = new MemoryStream();
            _Word.TplRowToStream(tplPath, row, stream, images);

            EchoStream(stream, fileName);
            stream.Dispose();
            */
        }

        /// <summary>
        /// export file by template and row
        /// </summary>
        /// <param name="tplPath">tpl path</param>
        /// <param name="fileName">export file name</param>
        /// <param name="row"></param>
        /// <param name="childs">IEnumerable for anonymous type</param>
        /// <param name="images"></param>
        /// <returns>error msg if any</returns>
        public static async Task<bool> ExportByTplRowAsync(string tplPath, string fileName, 
            dynamic row, List<IEnumerable<dynamic>> childs = null, 
            List <WordImageDto> images = null)
        {
            #region 1.check template file
            if (!File.Exists(tplPath))
            {
                await _Log.ErrorAsync($"_WebWord.cs ExportByTplRow() no tpl file ({tplPath})");
                return false;
            }
            #endregion

            #region 2.prepare memory stream
            var ms = new MemoryStream();
            var tplBytes = File.ReadAllBytes(tplPath);
            ms.Write(tplBytes, 0, tplBytes.Length);
            #endregion

            //3.binding stream && docx
            var fileStr = "";
            using (var docx = WordprocessingDocument.Open(ms, true))
            {
                //initial 
                var wordSet = new WordSetService(docx);
                var mainStr = wordSet.GetMainPartStr();

                //4.add images first
                if (images != null)
                    mainStr = await wordSet.AddImagesAsync(mainStr, images);

                //get word body start/end pos
                //int bodyStart = 0, bodyEnd = 0; //no start/end tag
                var bodyTpl = await wordSet.GetBodyTplAsync(mainStr);

                #region 5.fill row && childs rows
                var hasChild = (childs != null && childs.Count > 0);
                if (hasChild)
                {
                    var childLen = childs.Count;
                    int oldStart = 0, oldEnd = 0;
                    for (var i = 0; i < childLen; i++)
                    {
                        //int rowStart = 0, rowEnd = 0;
                        var rowTpl = await wordSet.GetRowTplAsync(bodyTpl.TplStr, i);
                        if (rowTpl.TplStr == "")
                            continue;

                        //set head or add left string of rows
                        if (i == 0)
                            fileStr = _Word.TplFillRow(bodyTpl.TplStr.Substring(0, rowTpl.StartPos), row);
                        else
                            fileStr += bodyTpl.TplStr.Substring(oldEnd + 1, rowTpl.StartPos - oldEnd - 1);

                        //add middle
                        fileStr += _Word.TplFillRows(rowTpl.TplStr, childs[i]);

                        //add tail
                        if (i == childLen - 1)
                            fileStr += _Word.TplFillRow(bodyTpl.TplStr.Substring(rowTpl.EndPos + 1), row);

                        //set old pos
                        oldStart = rowTpl.StartPos;
                        oldEnd = rowTpl.EndPos;
                    }//for childs

                     //set word file string
                    fileStr = mainStr.Substring(0, bodyTpl.StartPos) +
                        fileStr +
                        mainStr.Substring(bodyTpl.EndPos + 1);
                }
                else
                {
                    fileStr = _Word.TplFillRow(bodyTpl.TplStr, row);
                }
                #endregion

                //write into docx
                wordSet.SetMainPartStr(fileStr);
            }

            //check (for debug)
            //_Word.IsDocxValid(docx);

            //6.export file by stream
            await _Web.ExportByStream(ms, fileName);
            return true;
        }

    }//class
}
