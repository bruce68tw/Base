using Base.Models;
using Newtonsoft.Json.Linq;
using NPOI.SS.Formula;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    public static class _Word
    {
        //checked char type(yes/no)
        public const int Checkbox = 1;  //checkbox
        public const int Radio = 2;     //radio
        public const int CharV = 3;     //V char

        /// <summary>
        /// _HttpWord.MsByTplRow -> _Word.TplToMsA
        /// word to memoryStream for convert pdf
        /// </summary>
        /// <param name="tplPath"></param>
        /// <param name="fileName"></param>
        /// <param name="row"></param>
        /// <param name="childs"></param>
        /// <param name="images"></param>
        /// <returns></returns>
        public static async Task<MemoryStream?> TplToMsA(string tplPath, dynamic row,
            List<dynamic>? childs = null, List<WordImageDto>? images = null)
        {
            //1.check template file
            if (!File.Exists(tplPath))
            {
                await _Log.ErrorRootA($"_Word.cs TplToMsA() no template file ({tplPath})");
                return null;
            }

            //2.read word template file to docx
            using var fs = new FileStream(tplPath, FileMode.Open, FileAccess.Read);
            var docx = new XWPFDocument(fs);
            return new WordSetSvc(docx).GetMs(row, childs, images);
        }

        /// <summary>
        /// merge word files into one
        /// </summary>
        /// <param name="srcFiles">source word files/param>
        /// <param name="toFile">target word file</param>
        /// <param name="deleteSrc">delete source file or not</param>
        public static void MergeFiles(string[] srcFiles, string toFile, bool deleteSrc)
        {
            // copy first file to target
            File.Copy(srcFiles[0], toFile, true);

            using (var fs = new FileStream(toFile, FileMode.Open, FileAccess.ReadWrite))
            {
                var docx = new XWPFDocument(fs);
                for (var i = 1; i < srcFiles.Length; i++)
                {
                    // add page break
                    var paragraph = docx.CreateParagraph();
                    paragraph.CreateRun().AddBreak(BreakType.PAGE);

                    // add file content
                    using var srcFs = new FileStream(srcFiles[i], FileMode.Open, FileAccess.Read);
                    var srcDoc = new XWPFDocument(srcFs);
                    foreach (var para in srcDoc.Paragraphs)
                    {
                        var newParagraph = docx.CreateParagraph();
                        newParagraph.Alignment = para.Alignment;
                        var run = newParagraph.CreateRun();
                        run.SetText(para.Text);
                    }
                }

                docx.Write(fs);
            }

            // delete source files if needed
            if (deleteSrc)
            {
                foreach (var file in srcFiles)
                    File.Delete(file);
            }
        }

        /// <summary>
        /// Check if a DOCX file is valid
        /// </summary>
        public static bool IsDocxValid(string filePath)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var docx = new XWPFDocument(fs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Add an image to a DOCX file
        /// </summary>
        public static void DocxAddImage(string filePath, string imagePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            var docx = new XWPFDocument(fs);
            var run = docx.CreateParagraph().CreateRun();
            using (var imgFs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                run.AddPicture(imgFs, (int)PictureType.JPEG, imagePath, Units.ToEMU(200), Units.ToEMU(200));
            }
            docx.Write(fs);
        }

        /// <summary>
        /// Retrieve text from a DOCX file
        /// </summary>
        public static string GetDocxText(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var docx = new XWPFDocument(fs);
            var text = new StringWriter();
            foreach (var para in docx.Paragraphs)
            {
                text.WriteLine(para.Text);
            }
            return text.ToString();
        }


        //=== no change ===
        /*
        /// <summary>
        /// fill template string and return row string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowTpl"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string TplFillRow(string rowTpl, dynamic row)
        {
            return (row is JObject)
                ? TplFillJson(rowTpl, row) 
                : TplFillModel(rowTpl, row);
        }

        public static string TplFillModel<T>(string rowTpl, T row)
        {
            //if (row == null) return rowTpl;

            var props = row!.GetType().GetProperties();
            var result = rowTpl;
            foreach (var prop in props)
            {
                var value = prop.GetValue(row, null);
                result = result.Replace("[" + prop.Name + "]", (value == null) ? "" : value.ToString());
            }
            return result;
        }

        public static string TplFillJson(string rowTpl, JObject row)
        {
            var result = rowTpl;
            foreach (var item in row)
            {
                result = result.Replace("[" + item.Key + "]", item.Value!.ToString());
            }
            return result;
        }
        */

        /// <summary>
        /// fill template string and return rows string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowTpl"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        /*
        public static string TplFillRows(string rowTpl, IEnumerable<dynamic> rows)
        {
            if (!rows.Any()) return "";

            //var rows = (List<T>)row0s;
            //if (rows.Count == 0)
            //    return "";

            var result = "";
            var row0 = rows.First();
            if (row0 is JObject)
            {
                foreach (var row in rows)
                {
                    result += TplFillJson(rowTpl, row);
                }
            }
            else
            {
                var props = row0.GetType().GetProperties(); //減少在loop取值
                foreach (var row in rows)
                {
                    var text = rowTpl;
                    foreach (var prop in props)
                    {
                        var value = prop.GetValue(row, null);
                        text = text.Replace("[" + prop.Name + "]", (value == null) ? "" : value.ToString());
                    }
                    result += text;
                }
            }
            return result;
        }
        */

        /// <summary>
        /// if multiple area has fixed rows, can treat as single row
        /// table field id add pre a,b,c(for multiple tables), add tail 0,1,2 for row no
        /// </summary>
        /// <param name="rows">multiple rows, nullable</param>
        /// <param name="row">single row to write into</param>
        /// <param name="preTable">table field id pre a,b,c</param>
        /// <param name="maxRows">multiple area with fixed rows</param>
        /// <param name="cols">table column list, no pre/tail char (rows could be null, so this input is need)</param>
        public static void FixedRowsToRow(JArray rows, JObject row, string preTable, int maxRows, List<string> cols)
        {
            //write row
            //if (extCols != null)
            //    cols.AddRange(extCols);
            var colLen = cols.Count;
            var rowLen = (rows == null) ? 0 : rows.Count;
            for (var i = 0; i < maxRows; i++)
            {
                //reset table column or write into
                if (rowLen > i)
                    for (var j = 0; j < colLen; j++)
                        row[preTable + cols[j] + i] = rows![i][cols[j]]!.ToString();
                else
                    for (var j = 0; j < colLen; j++)
                        row[preTable + cols[j] + i] = "";

            }
        }

        /// <summary>
        /// set multiple checkbox fields
        /// </summary>
        /// <param name="row">source row</param>
        /// <param name="value">field value</param>
        /// <param name="preFid">field pre char</param>
        /// <param name="startNo">start column no</param>
        /// <param name="endNo">end column no</param>
        /// <param name="type">char type, 1:checkbox, 2:radio, 3:V</param>
        public static void ValueToChecks(JObject row, string value, string preFid, int startNo, int endNo, int type = Checkbox)
        {
            for (var i = startNo; i <= endNo; i++)
            {
                var fid = preFid + i;
                row[fid] = YesNo(value == i.ToString(), type);
            }
        }

        //get Checkbox/Radio button
        public static string YesNo(string status, int type = Checkbox)
        {
            return YesNo(status == "1", type);
        }

        public static string YesNo(bool status, int type = Checkbox)
        {
            if (type == Checkbox) return status ? "■" : "□";
            if (type == Radio) return status ? "●" : "○";
            else return status ? "V" : "";
        }

        #region remark code
        /// <summary>
        /// write into docx stream, consider multiple rows(copy from _WebWord.cs Output())
        /// </summary>
        /// <param name="row"></param>
        /// <param name="stream"></param>
        /// <param name="images"></param>
        /// <param name="rows">"multiple" rows</param>
        /*
        public static bool StreamFillData(JObject row, Stream stream, List<WordImageDto> images = null, List<WordRowsDto> wordRows = null)
        {
            //stream -> docx
            using (var docx = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
                //call delegate if need
                //var mainPart = docx.MainDocumentPart;

                //=== 2.do single row start ===
                //read template file
                var mainTpl = GetMainTpl(docx);

                //add image first
                if (images != null)
                    DocxAddImage(docx, ref mainTpl, images);

                //fill master row
                mainTpl = StrFillRow(mainTpl, row);
                //foreach (var item in row)
                //    mainTpl = mainTpl.Replace("[" + item.Key + "]", item.Value.ToString());

                //multiple rows
                if (wordRows != null)
                {
                    foreach (var wordRow in wordRows)
                    {
                        //find tag name
                        //find box tag & row -> template string
                        var tplStart = 0;
                        var tplEnd = 0;
                        var rowTpl = "";
                        var rowList = "";
                        foreach (JObject row2 in wordRow.Rows)
                        {
                            var rowStr = rowTpl;
                            foreach (var item in row2)
                                rowStr = rowStr.Replace("[" + item.Key + "]", item.Value.ToString());
                            rowList += rowStr;
                        }

                        mainTpl = mainTpl.Substring(0, tplStart) + rowList + mainTpl.Substring(0, tplEnd);
                    }
                }

                StrToDocxMain(mainTpl, docx);

                //Debug.Assert(IsDocxValid(doc), "Invalid File!");

                //no save, but can debug !!
                //mainPart.Document.Save();
                //=== 2. end ===
                return true;
            }
        }
        */

        /// <summary>
        /// convert json row + template file to word file
        /// </summary>
        /// <param name="row">source data</param>
        /// <param name="tplPath">template path</param>
        /// <param name="filePath">output file path</param>
        /// <param name="images">image data, four fields for one imge(path,width,height,tag)</param>
        /*
        public static bool RowToFile(JObject row, string tplPath, string filePath, List<WordImageDto> images = null)
        {
            //check template file
            if (!File.Exists(tplPath))
            {
                _Log.Error("_Word.TplToFile() error: no file " + tplPath);
                return false;
            }

            //if (fileName.IndexOf(".") < 0)
            //    fileName += ".docx";

            File.Copy(tplPath, filePath, true);

            //openXml start
            var stream = new FileStream(filePath, FileMode.Open);
            StreamFillData(row, stream, images);
            stream.Dispose();
            return true;
        }
        */

        /// <summary>
        /// convert template word file(with row) to stream
        /// </summary>
        /// <param name="row"></param>
        /// <param name="tplPath"></param>
        /// <param name="stream"></param>
        /// <param name="images"></param>
        /*
        public static bool RowToStream(JObject row, string tplPath, Stream stream, List<WordImageDto> images = null)
        {
            //check template file
            if (!File.Exists(tplPath))
            {
                _Log.Error("_Word.TplRowToStream() error: no file " + tplPath);
                return false;
            }

            //template file to stream
            var tplBytes = File.ReadAllBytes(tplPath);
            stream.Write(tplBytes, 0, (int)tplBytes.Length);
            return StreamFillData(row, stream, images);
        }
        */

        /// <summary>
        /// template string fill json row
        /// </summary>
        /// <param name="str"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        /*
        public static string StrFillRow(string str, JObject row)
        {
            foreach (var item in row)
                str = str.Replace("[" + item.Key + "]", item.Value.ToString());
            return str;
        }
        */


        #endregion

    }//class
}
