using Base.Models;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using NPOI.XWPF.UserModel;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Wordprocessing;
using NPOI.SS.Formula;

namespace Base.Services
{
    //word套表
    public class WordSetSvc
    {
        //word carrier
        public const string Carrier = "<w:br/>";
        public const string PageBreak = "<w:p><w:pPr><w:sectPr><w:type w:val=\"nextPage\" /></w:sectPr></w:pPr></w:p>";

        //checked char type(yes/no)
        public const int Checkbox = 1;  //checkbox
        public const int Radio = 2;     //radio
        public const int CharV = 3;     //V char

        //instance variables
        private XWPFDocument _docx;

        //constructor
        public WordSetSvc(XWPFDocument docx)
        {
            _docx = docx;
        }

        /// <summary>
        /// get memory stream
        /// </summary>
        /// <param name="row"></param>
        /// <param name="childs"></param>
        /// <param name="images"></param>
        /// <returns></returns>
        public async Task<MemoryStream?> GetMsA(dynamic row,
            List<IEnumerable<dynamic>>? childs = null, List<WordImageDto>? images = null)
        {
            //fill row
            FillRow(row);

            //fill childs
            if (childs != null)
            {
                for (var i = 0; i < childs.Count; i++)
                    FillRows(i, childs[i]);
            }

            //3.binding stream && docx
            var fileStr = "";

            //initial 
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

        private void FillRow(dynamic row)
        {
            if (row == null) 
                return;
            else if (row is JObject)
                FillJson(row);
            else
                FillModel(row);
        }

        private void FillModel<T>(T row)
        {
            var props = row!.GetType().GetProperties();
            foreach (var paragraph in _docx.Paragraphs)
                foreach (var run in paragraph.Runs)
                    foreach (var prop in props)
                    {
                        var value = prop.GetValue(row, null);
                        run.SetText(run.Text.Replace($"[{prop.Name}]", (value == null) ? "" : value.ToString()), 0);
                    }
        }

        private void FillJson(JObject row)
        {
            foreach (var paragraph in _docx.Paragraphs)
                foreach (var run in paragraph.Runs)
                    foreach (var item in row)
                        run.SetText(run.Text.Replace($"[{item.Key}]", item.Value!.ToString()), 0);
        }

        //XWPFTable
        private void FillRows(int index, IEnumerable<dynamic> rows)
        {
            if (!rows.Any()) return;

            //找含有 [x!] 的範本列, base 0
            var tplRow = _docx.Tables
                .SelectMany(t => t.Rows)   // 展開所有表格的所有列
                .FirstOrDefault(r => r.GetTableCells().Any(c => c.GetText().Contains("[0!]")));
            if (tplRow == null) return;

            //get table
            var table = tplRow.GetTable();

            //是否json
            if (rows.First() is JObject)
                FillJsons(table, tplRow, rows as JArray);
            else
                FillModels(table, tplRow, rows);
        }

        private void FillJsons(XWPFTable table, XWPFTableRow tplRow, JArray rows)
        {
            //todo
            // get欄位資訊
            var nos = new List<int>();
            var props = rows.First()!.GetType().GetProperties(); //減少在loop取值
            foreach (var prop in props)
            {
                var index = tplRow.GetTableCells()
                    .Select((cell, index) => new { cell, index })
                    .FirstOrDefault(ci => ci.cell.GetText().Contains($"[{prop.Name}]"))?.index ?? -1;
                if (index >= 0)
                    nos.Add(index);
            }

            //新增資料列
            foreach (var row in rows)
            {
                // 複製範本列
                var newRow = new XWPFTableRow(tplRow.GetCTRow().Copy(), table);

                // 動態替換欄位，例如 [Name]、[Age]
                foreach (var item in row)
                    run.SetText(run.Text.Replace($"[{item.Key}]", item.Value!.ToString()), 0);

                foreach (var no in nos)
                {
                    //var prop = props[no];
                    //var value = prop.GetValue(row)?.ToString() ?? "";
                    var cell = newRow.GetCell(no);
                    cell.SetText(value);
                }

                // 將新列加入表格
                table.AddRow(newRow);
            }

            // 新增完成後刪除範本列
            table.RemoveRow(table.Rows.IndexOf(tplRow));

        }
        private void FillModels<T>(XWPFTable table, XWPFTableRow tplRow, List<T> rows)
        {
            // get欄位資訊
            var nos = new List<int>();
            var props = rows.First()!.GetType().GetProperties(); //減少在loop取值
            foreach (var prop in props)
            {
                var index = tplRow.GetTableCells()
                    .Select((cell, index) => new { cell, index })
                    .FirstOrDefault(ci => ci.cell.GetText().Contains($"[{prop.Name}]"))?.index ?? -1;
                if (index >= 0)
                    nos.Add(index);
            }

            //範本列移除標記

            //新增資料列
            foreach (var row in rows)
            {
                // 複製範本列
                var newRow = new XWPFTableRow(tplRow.GetCTRow().Copy(), table);

                // 動態替換欄位，例如 [Name]、[Age]
                foreach (var no in nos)
                {
                    var prop = props[no];
                    var value = prop.GetValue(row)?.ToString() ?? "";
                    var cell = newRow.GetCell(no);
                    cell.SetText(value);
                }

                // 將新列加入表格
                table.AddRow(newRow);
            }

            // 新增完成後刪除範本列
            table.RemoveRow(table.Rows.IndexOf(tplRow));
        }

        /// <summary>
        /// Word docx add images
        /// </summary>
        /// <param name="srcTpl">(ref) source template string</param>
        /// <param name="imageDtos">List of image data</param>
        /// <returns>New template string</returns>
        public string AddImages(string srcTpl, List<WordImageDto> imageDtos)
        {
            if (imageDtos == null || imageDtos.Count == 0)
                return srcTpl;

            // Load the main document part
            var docx = new XWPFDocument();
            foreach (var imageInfo in imageDtos)
            {
                try
                {
                    // Load image from file path
                    using var fileStream = new FileStream(imageInfo.FilePath, FileMode.Open, FileAccess.Read);
                    var runDto = GetImageRunDto(imageInfo.FilePath);
                    if (runDto == null) continue;

                    // Create paragraph and run for image insertion
                    var paragraph = docx.CreateParagraph();
                    var run = paragraph.CreateRun();

                    // Add picture to the run
                    var pictureType = GetPictureType(imageInfo.FilePath);
                    run.AddPicture(fileStream, pictureType, imageInfo.FilePath, runDto.WidthEmu, runDto.HeightEmu);

                    // Replace the placeholder in the template string
                    var placeholder = $"{{{{{imageInfo.Code}}}}}";
                    srcTpl = srcTpl.Replace(placeholder, $"<img src='{imageInfo.FilePath}' width='{runDto.WidthEmu}' height='{runDto.HeightEmu}' />");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding image: {ex.Message}");
                }
            }

            return srcTpl;
        }

        /// <summary>
        /// Determine the picture type based on the file extension
        /// </summary>
        /// <param name="filePath">Image file path</param>
        /// <returns>PictureType for NPOI</returns>
        private int GetPictureType(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".jpeg" or ".jpg" => (int)PictureType.JPEG,
                ".png" => (int)PictureType.PNG,
                ".bmp" => (int)PictureType.BMP,
                ".gif" => (int)PictureType.GIF,
                ".wmf" => (int)PictureType.WMF,
                ".emf" => (int)PictureType.EMF,
                _ => (int)PictureType.JPEG,  // Default to JPEG if unknown
            };
        }

        /// <summary>
        /// image file to Run for docx
        /// </summary>
        private string GetImageRun(string imagePartId, WordImageRunDto imageRun)
        {
            return $"<img src=\"{imagePartId}\" alt=\"{imageRun.ImageCode}\" width=\"{imageRun.WidthEmu}\" height=\"{imageRun.HeightEmu}\">";
        }


        //=== no change ===
        /// <summary>
        /// return page break string
        /// </summary>
        /// <returns></returns>
        public string GetPageBreak()
        {
            return PageBreak;
        }

        /// <summary>
        /// Get the main content of the Word document as a string
        /// </summary>
        /// <returns>Main document content as a string</returns>
        public string GetMainPartStr()
        {
            using var ms = new MemoryStream();
            _docx.Write(ms);
            ms.Position = 0;

            using var sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// write string into docx mainpart
        /// Sets the main content of the Word document from a string
        /// </summary>
        /// <param name="str">The content to be written to the Word document</param>
        public void SetMainPartStr(string str)
        {
            using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(str));
            _docx = new XWPFDocument(ms);
        }

        /// <summary>
        /// return docx body template string
        /// </summary>
        /// <param name="srcText"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public WordSetTplDto GetBodyTpl(string srcText)
        {
            return GetRangeTpl(srcText, false, "<w:body>", "</w:body>")!;
        }

        /// <summary>
        /// return tr(row) template string
        /// </summary>
        /// <param name="srcText"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="childIdx"></param>
        /// <returns></returns>
        public WordSetTplDto GetRowTpl(string srcText, int childIdx = -1)
        {
            var findMid = (childIdx < 0) ? "[!]" : $"[!{childIdx}]";
            var result = GetRangeTpl(srcText, true, "<w:tr ", "</w:tr>", findMid);
            result!.TplStr = result.TplStr.Replace(findMid, "");
            return result;
        }

        /// <summary>
        /// get drawing template string for one photo
        /// </summary>
        /// <param name="srcText"></param>
        /// <param name="imageCode"></param>
        /// <returns></returns>
        public WordSetTplDto GetDrawTpl(string srcText, string imageCode)
        {
            return GetRangeTpl(srcText, true, "<w:drawing>", "</w:drawing>", $"descr=\"{imageCode}\"")!;
        }

        private WordSetTplDto GetGraphDataTpl(string srcText)
        {
            return GetRangeTpl(srcText, true, "<a:graphicData", "</a:graphicData>")!;
        }

        /// <summary>
        /// get range string for word multiple rows
        /// </summary>
        /// <param name="srcText">source text</param>
        /// <param name="hasTag">result start/end pos include tag or not</param>
        /// <param name="findStart">find start string</param>
        /// <param name="findEnd">equal to startStr if empty</param>
        /// <param name="findMid">find start string</param>
        /// <returns></returns>
        private WordSetTplDto? GetRangeTpl(string srcText, bool hasTag, string findStart, 
            string findEnd, string findMid = "")
        {
            //int midStartPos = -1, midEndPos = -1;
            //var result2 = new WordSetTplDto();
            //var tplStr = "";
            int startPos, endPos;
            if (findMid == "")
            {
                startPos = srcText.IndexOf(findStart);
                endPos = srcText.IndexOf(findEnd);
            }
            else
            {
                var midPos = srcText.IndexOf(findMid);
                if (midPos < 0)
                    goto lab_error;

                //var midEndPos = (midEndStr == "") ? midPos : srcText.IndexOf(findMid, midPos);
                //if (midEndPos < 0)
                //    return "";

                startPos = srcText.LastIndexOf(findStart, midPos);
                endPos = srcText.IndexOf(findEnd, midPos);
            }

            if (startPos < 0 || endPos < 0)
                goto lab_error;

            //case of ok
            var startLen = findStart.Length;
            var endLen = findEnd.Length;
            endPos = endPos + endLen - 1;   //adjust
            var tplStr = srcText[startPos..(endPos + 1)];
            if (!hasTag)
            {
                tplStr = tplStr[startLen..^endLen];
                startPos += startLen;
                endPos -= endLen;
            }
            return new WordSetTplDto()
            {
                TplStr = tplStr,
                StartPos = startPos,
                EndPos = endPos,
            }; 

        lab_error:
            _Log.Error($"WordSetSvc.cs GetRangeTpl() get empty result.(findStart='{findStart}', findMid='{findMid}')");
            return null;
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
        public void ValueToChecks(JObject row, string value, string preFid, int startNo, int endNo, int type = Checkbox)
        {
            for (var i = startNo; i <= endNo; i++)
            {
                var fid = preFid + i;
                row[fid] = YesNo(value == i.ToString(), type);
            }
        }

        //get Checkbox/Radio button
        public string YesNo(string status, int type = Checkbox)
        {
            return YesNo(status == "1", type);
        }

        public string YesNo(bool status, int type = Checkbox)
        {
            if (type == Checkbox)
                return status ? "■" : "□";
            if (type == Radio)
                return status ? "●" : "○";
            else
                return status ? "V" : "";
        }

        private WordImageRunDto? GetImageRunDto(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            //pixels / 300(dpi) * 2.54(inch to cm) * 36000(cm to emu)
            const double PixelToEmu = 304.8;    

            var ms = new MemoryStream(File.ReadAllBytes(filePath));
            /*
            var img = new Bitmap(ms);   //for get width/height
            var result = new WordImageRunDto()
            {
                DataStream = ms,
                FileName = _File.GetFileName(filePath),
                WidthEmu = Convert.ToInt64(img.Width * PixelToEmu),
                HeightEmu = Convert.ToInt64(img.Height * PixelToEmu),
                ImageCode = $"IMG{_Str.NewId()}",
            };
            */
            // 使用 ImageSharp 讀取圖片尺寸
            using var img = Image.Load<Rgba32>(ms);
            var result = new WordImageRunDto()
            {
                DataStream = ms,
                FileName = Path.GetFileName(filePath),
                WidthEmu = Convert.ToInt32(img.Width * PixelToEmu),
                HeightEmu = Convert.ToInt32(img.Height * PixelToEmu),
                ImageCode = $"IMG{Guid.NewGuid()}",
            };

            //important !!, or cause docx show image failed: not have enough memory.
            ms.Position = 0;    
            return result;
        }

        #region remark code
        /*
        /// <summary>
        /// template string fill json row
        /// </summary>
        /// <param name="str"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public string StrFillRow(string str, JObject row)
        {
            foreach (var item in row)
                str = str.Replace("[" + item.Key + "]", item.Value.ToString());
            return str;
        }

        /// <summary>
        /// write into docx stream, consider multiple rows(copy from _WebWord.cs Output())
        /// </summary>
        /// <param name="row"></param>
        /// <param name="stream"></param>
        /// <param name="images"></param>
        /// <param name="rows">"multiple" rows</param>
        public bool StreamFillData(JObject row, Stream stream, List<WordImageDto> images = null, List<WordRowsDto> wordRows = null)
        {
            //stream -> docx
            using (var docx = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
                //call delegate if need
                //var mainPart = docx.MainDocumentPart;

                //=== 2.do single row start ===
                //read template file
                var mainTpl = GetMainPartStr();

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
                    foreach(var wordRow in wordRows)
                    {
                        //find tag name
                        //find box tag & row -> template string
                        var tplStart = 0;
                        var tplEnd = 0;
                        var rowTpl = "";
                        var rowList = "";
                        foreach(JObject row2 in wordRow.Rows)
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

        /// <summary>
        /// ?? if multiple area has fixed rows, can treat as single row
        /// table field id add pre a,b,c(for multiple tables), add tail 0,1,2 for row no
        /// </summary>
        /// <param name="rows">multiple rows, nullable</param>
        /// <param name="row">single row to write into</param>
        /// <param name="preTable">table field id pre a,b,c</param>
        /// <param name="maxRows">multiple area with fixed rows</param>
        /// <param name="cols">table column list, no pre/tail char (rows could be null, so this input is need)</param>
        public void FixedRowsToRow(JArray rows, JObject row, string preTable, int maxRows, List<string> cols)
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
                        row[preTable + cols[j] + i] = rows[i][cols[j]].ToString();
                else
                    for (var j = 0; j < colLen; j++)
                        row[preTable + cols[j] + i] = "";

            }
        }
        */
        #endregion

    }//class
}
