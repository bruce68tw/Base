using Base.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using D = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;

namespace Base.Services
{
    public class WordSetSvc
    {
        //word carrier
        //public const string Carrier = "<w:br/>";
        //public const string PageBreak = "<w:p><w:pPr><w:sectPr><w:type w:val=\"nextPage\" /></w:sectPr></w:pPr></w:p>";

        //checked char type(yes/no)
        public const int Checkbox = 1;  //checkbox
        public const int Radio = 2;     //radio
        public const int CharV = 3;     //V char

        //instance variables
        private WordprocessingDocument _docx = null!;
        private MemoryStream _ms = null!;

        //template start/end position
        //private int _tplStartPos, _tplEndPos;

        //constructor
        public WordSetSvc(string tplPath)
        {
            // 1. 檢查模板檔案
            if (!File.Exists(tplPath))
            {
                return;
                //await _Log.ErrorRootA($"_Word.cs TplToMsA() no template file ({tplPath})");
                //return null;
            }

            // 開啟 Word 文件 (唯讀)
            using var fs = new FileStream(tplPath, FileMode.Open, FileAccess.Read);

            // 2. 將模板讀入記憶體流
            _ms = new MemoryStream();
            fs.CopyTo(_ms);
            _ms.Position = 0;  // 重設流位置

            _docx = WordprocessingDocument.Open(_ms, true);
        }

        /// <summary>
        /// get result memory stream
        /// </summary>
        /// <param name="row"></param>
        /// <param name="childs"></param>
        /// <param name="images"></param>
        /// <returns></returns>
        public MemoryStream? GetResultMs(dynamic row,
            List<dynamic>? childs = null, List<WordImageDto>? images = null)
        {
            //fill childs first(只存在table), 減少row的欄位
            if (childs != null)
                for (var i = 0; i < childs.Count; i++)
                    FillRows(i, childs[i]);

            //fill row, 包含段落和table
            FillRow(row);

            //add images
            if (_List.NotEmpty(images))
                foreach (var image in images!)
                    AddImage(image);

            //var ms = new MemoryStream();
            _docx.MainDocumentPart!.Document.Save();
            _docx.Dispose();
            _ms.Position = 0;  // 重置位置
            return _ms;
        }

        private void FillRow(dynamic row)
        {
            if (row == null)
                return;
            else if (_Object.IsJObject(row))
                FillJson(row);
            else
                FillModel(row);
        }

        private void FillModel<T>(T row)
        {
            var props = row!.GetType().GetProperties();
            var body = _docx.MainDocumentPart?.Document.Body!;
            foreach (var para in body.Elements<Paragraph>())
                foreach (var run in para.Elements<Run>())
                    foreach (var prop in props)
                    {
                        var value = prop.GetValue(row, null);
                        var textElm = run.GetFirstChild<Text>();
                        if (textElm != null)
                        {
                            var newText = textElm.Text.Replace($"[{prop.Name}]", (value == null) ? "" : value.ToString());
                            textElm.Text = newText;
                        }
                    }
        }

        private void FillJson(JObject row)
        {
            var body = _docx.MainDocumentPart?.Document.Body!;
            foreach (var para in body.Elements<Paragraph>())
                foreach (var run in para.Elements<Run>())
                {
                    var textElm = run.GetFirstChild<Text>();
                    if (textElm != null)
                    {
                        foreach (var item in row)
                        {
                            var newText = textElm.Text.Replace($"[{item.Key}]", item.Value!.ToString());
                            textElm.Text = newText;
                        }
                    }
                }
        }

        private void CellSetText(TableCell cell, string value)
        {
            // 清空單元格中的所有段落
            cell.RemoveAllChildren<Paragraph>();

            // 新增一個段落並設置文字
            var para = new Paragraph();
            var run = new Run();
            var text = new Text(value);

            run.Append(text);
            para.Append(run);
            cell.Append(para);
        }

        //XWPFTable
        private void FillRows(int index, IEnumerable<dynamic>? rows)
        {
            if (rows == null || !rows.Any()) return;

            // 找到包含指定標記的表格列            
            var tag = $"[!{index}]";    // 找含有 [x!] 的範本列, base 0
            var tplRow = _docx.MainDocumentPart?.Document.Body?.Elements<Table>()
                .SelectMany(t => t.Elements<TableRow>())
                .FirstOrDefault(r => r.Elements<TableCell>()
                    .Any(c => c.InnerText.Contains(tag)));
            if (tplRow == null) return;

            // 清除標記
            var cell = tplRow.Elements<TableCell>()
                .First(c => c.InnerText.Contains(tag));
            foreach (var paragraph in cell.Elements<Paragraph>())
                foreach (var run in paragraph.Elements<Run>())
                    foreach (var text in run.Elements<Text>())
                        text.Text = text.Text.Replace(tag, "");

            // 取得表格
            var table = tplRow.Parent as Table;
            if (table == null) return;

            // 是否為 JSON
            if (_Object.IsJObject(rows.First()))
                FillJsons(table, tplRow, JArray.FromObject(rows));
            else
                FillModels(table, tplRow, rows);
        }

        private void FillJsons(Table table, TableRow tplRow, JArray rows)
        {
            // 取得欄位資訊
            var nos = new List<IdNumDto>();
            var row0 = (JObject)rows[0];
            var cells = tplRow.Elements<TableCell>().ToList();

            foreach (var item in row0)
            {
                var fid = item!.Key;
                var index = cells
                    .Select((cell, idx) => new { cell, idx })
                    .FirstOrDefault(ci => ci.cell.InnerText.Contains($"[{fid}]"))?.idx ?? -1;

                if (index >= 0)
                {
                    nos.Add(new IdNumDto
                    {
                        Id = fid,
                        Num = index,
                    });
                }
            }

            // 新增資料列
            foreach (var row in rows)
            {
                // 複製範本列
                var newRow = (TableRow)tplRow.CloneNode(true);

                // 動態替換欄位，例如 [Name]、[Age]
                foreach (var no in nos)
                {
                    var value = row[no.Id]?.ToString() ?? "";
                    var cell = newRow.Elements<TableCell>().ElementAtOrDefault(no.Num);
                    if (cell != null)
                        CellSetText(cell, value);
                }

                // 將新列加入表格
                table.AppendChild(newRow);
            }

            // 新增完成後刪除範本列
            tplRow.Remove();
        }

        private void FillModels<T>(Table table, TableRow tplRow, IEnumerable<T> rows)
        {
            // 取得欄位資訊
            var nos = new List<int>();
            var props = rows.First()!.GetType().GetProperties(); // 減少在 loop 中取值
            var cells = tplRow.Elements<TableCell>().ToList();

            foreach (var prop in props)
            {
                var fid = prop.Name;
                var index = cells
                    .Select((cell, idx) => new { cell, idx })
                    .FirstOrDefault(ci => ci.cell.InnerText.Contains($"[{fid}]"))?.idx ?? -1;
                if (index >= 0)
                {
                    nos.Add(index);
                }
            }

            // 新增資料列
            foreach (var row in rows)
            {
                // 複製範本列
                var newRow = (TableRow)tplRow.CloneNode(true);

                // 動態替換欄位，例如 [Name]、[Age]
                foreach (var no in nos)
                {
                    var prop = props[no];
                    var value = prop.GetValue(row)?.ToString() ?? "";
                    var cell = newRow.Elements<TableCell>().ElementAtOrDefault(no);
                    if (cell != null)
                    {
                        CellSetText(cell, value);
                    }
                }

                // 將新列加入表格
                table.AppendChild(newRow);
            }

            // 新增完成後刪除範本列
            tplRow.Remove();
        }

        /// <summary>
        /// word內使用anchor類型圖案
        /// </summary>
        /// <param name="imageDto"></param>
        private void AddImage(WordImageDto imageDto)
        {
            var mainPart = _docx.MainDocumentPart;
            if (mainPart == null) return;

            // 找到指定名稱的圖片 (非視覺屬性 name = imageDto.Code)
            var pic = mainPart.Document
                .Descendants<DW.Inline>()
                .FirstOrDefault(inl => inl.DocProperties?.Description == imageDto.Code);
            if (pic == null) return;

            //var aa = mainPart.Document.Descendants<D.Blip>()
            //    .ToList();

            // 取得圖片嵌入 (blip)
            var blip = pic.Descendants<D.Blip>().FirstOrDefault();
            if (blip == null) return;

            // 取得圖片大小 Extents
            var extent = pic.Descendants<D.Extents>().FirstOrDefault();
            if (extent == null) return;

            long width = extent!.Cx!;
            long height = extent!.Cy!;

            // 讀取新圖片檔案 Bytes
            byte[] newPicBytes = File.ReadAllBytes(imageDto.FilePath);

            // 刪除舊圖片 Part
            var oldRelId = blip.Embed?.Value;
            if (string.IsNullOrEmpty(oldRelId)) return;

            var oldImagePart = (ImagePart)mainPart.GetPartById(oldRelId);
            mainPart.DeletePart(oldImagePart);

            // 新增圖片 Part (這裡以 Png 為例，請依圖片格式調整)
            var newImagePart = mainPart.AddImagePart(ImagePartType.Png);
            using (var stream = new MemoryStream(newPicBytes))
            {
                newImagePart.FeedData(stream);
            }

            // 更新圖片 Embed ID
            blip!.Embed!.Value = mainPart.GetIdOfPart(newImagePart);

            // 保持圖片大小
            extent.Cx = width;
            extent.Cy = height;
        }

        /// <summary>
        /// word docx add images
        /// </summary>
        /// <param name="srcTpl">(ref) source template string</param>
        /// <param name="imageDtos"></param>
        /// <returns>new template string</returns>
        /*
        public string AddImages(string srcTpl, List<WordImageDto> imageDtos)
        {
            if (imageDtos == null || imageDtos.Count == 0)
                return "";

            MainDocumentPart mainPart = _docx.MainDocumentPart!;    //not null
            foreach (var imageInfo in imageDtos)
            {
                //get image file info
                var runDto = GetImageRunDto(imageInfo.FilePath);
                if (runDto == null) continue;

                ImagePart imagePart = mainPart!.AddImagePart(ImagePartType.Jpeg);
                var imageId = mainPart.GetIdOfPart(imagePart);
                imagePart.FeedData(runDto!.DataStream!);
                var newRun = GetImageRun(imageId, runDto);

                //find drawing object from srcTpl
                var drawTpl = GetDrawTpl(srcTpl, imageInfo.Code);
                if (drawTpl == null) continue;

                //find graphicData object(map to D.Graphic) from drawing object
                var graphTpl = GetGraphDataTpl(drawTpl.TplStr);
                if (graphTpl == null) continue;

                //replace graphic section
                //newRun.Descendants<D.Graphic>().First().InnerXml cause .docx open failed !!
                var newGraph = GetGraphDataTpl(newRun.InnerXml);
                //int newStart = _tplStartPos, newEnd = _tplEndPos;

                //update srcTpl string
                srcTpl = srcTpl[..(drawTpl.StartPos + graphTpl.StartPos)] +
                    //newRun.Descendants<D.Graphic>().First().InnerXml +    //.docx open failed !!
                    newGraph.TplStr +
                    srcTpl[(drawTpl.StartPos + graphTpl.EndPos + 1)..];
            }

            //return new tpl string
            return srcTpl;
        }

        /// <summary>
        /// image file to Run for docx
        /// https://msdn.microsoft.com/zh-tw/library/office/bb497430.aspx
        /// </summary>
        /// <param name="imagePartId"></param>
        /// <param name="imageRun"></param>
        /// <returns></returns>
        private D.Run GetImageRun(string imagePartId, WordImageRunDto imageRun)
        {
            // Define the reference of the image.
            var element =
                 new Drawing(
                     new DW.Inline(
                         //Size of image, unit = EMU(English Metric Unit)
                         //1 cm = 360000 EMUs
                         new DW.Extent() { Cx = imageRun.WidthEmu, Cy = imageRun.HeightEmu },
                         new DW.EffectExtent()
                         {
                             LeftEdge = 0L,
                             TopEdge = 0L,
                             RightEdge = 0L,
                             BottomEdge = 0L
                         },
                         new DW.DocProperties()
                         {
                             Id = (UInt32Value)1U,
                             Name = imageRun.ImageCode,
                         },
                         new DW.NonVisualGraphicFrameDrawingProperties(
                             new D.GraphicFrameLocks() { NoChangeAspect = true }),
                         new D.Graphic(
                             new D.GraphicData(
                                 new DP.Picture(
                                     new DP.NonVisualPictureProperties(
                                         new DP.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = imageRun.FileName,
                                         },
                                         new DP.NonVisualPictureDrawingProperties()),
                                     new DP.BlipFill(
                                         new D.Blip(
                                             new D.BlipExtensionList(
                                                 new D.BlipExtension()
                                                 {
                                                     Uri =
                                                        "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                 })
                                         )
                                         {
                                             Embed = imagePartId,
                                             CompressionState =
                                             D.BlipCompressionValues.Print
                                         },
                                         new D.Stretch(
                                             new D.FillRectangle())),
                                     new DP.ShapeProperties(
                                         new D.Transform2D(
                                             new D.Offset() { X = 0L, Y = 0L },
                                             new D.Extents()
                                             {
                                                 Cx = imageRun.WidthEmu,
                                                 Cy = imageRun.HeightEmu
                                             }),
                                         new D.PresetGeometry(
                                             new D.AdjustValueList()
                                         )
                                         { Preset = D.ShapeTypeValues.Rectangle }))
                             )
                             { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                     )
                     {
                         DistanceFromTop = (UInt32Value)0U,
                         DistanceFromBottom = (UInt32Value)0U,
                         DistanceFromLeft = (UInt32Value)0U,
                         DistanceFromRight = (UInt32Value)0U,
                         //EditId = "50D07946", //remark, coz word 2007 open failed !!
                     });
            return new D.Run(element);
        }
        */


        //=== no change ===
        /// <summary>
        /// return page break string
        /// </summary>
        /// <returns></returns>
        /*
        public string GetPageBreak()
        {
            return PageBreak;
        }

        /// <summary>
        /// read docx MainDocumentPart to string
        /// </summary>
        /// <returns></returns>
        public string GetMainPartStr()
        {
            using var sr = new StreamReader(_docx.MainDocumentPart!.GetStream());
            return sr.ReadToEnd();
        }

        /// <summary>
        /// write string into docx mainpart
        /// </summary>
        public void SetMainPartStr(string str)
        {
            //string to docx stream, must use FileMode.Create, or target file can not open !!
            using var sw = new StreamWriter(_docx.MainDocumentPart!.GetStream(FileMode.Create));
            sw.Write(str);
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
        */

        /// <summary>
        /// set multiple checkbox fields
        /// </summary>
        /// <param name="row">source row</param>
        /// <param name="value">field value</param>
        /// <param name="preFid">field pre char</param>
        /// <param name="startNo">start column no</param>
        /// <param name="endNo">end column no</param>
        /// <param name="type">char type, 1:checkbox, 2:radio, 3:V</param>
        private void ValueToChecks(JObject row, string value, string preFid, int startNo, int endNo, int type = Checkbox)
        {
            for (var i = startNo; i <= endNo; i++)
            {
                var fid = preFid + i;
                row[fid] = YesNo(value == i.ToString(), type);
            }
        }

        //get Checkbox/Radio button
        private string YesNo(string status, int type = Checkbox)
        {
            return YesNo(status == "1", type);
        }

        private string YesNo(bool status, int type = Checkbox)
        {
            if (type == Checkbox)
                return status ? "■" : "□";
            if (type == Radio)
                return status ? "●" : "○";
            else
                return status ? "V" : "";
        }

        /*
        private WordImageRunDto? GetImageRunDto(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            //pixels / 300(dpi) * 2.54(inch to cm) * 36000(cm to emu)
            const double PixelToEmu = 304.8;    

            var ms = new MemoryStream(File.ReadAllBytes(filePath));
            // 使用 ImageSharp 讀取圖片尺寸
            using var img = Image.Load<Rgba32>(ms);
            var result = new WordImageRunDto()
            {
                DataStream = ms,
                FileName = Path.GetFileName(filePath),
                //WidthEmu = Convert.ToInt64(img.Width * PixelToEmu),
                //HeightEmu = Convert.ToInt64(img.Height * PixelToEmu),
                ImageCode = $"IMG{Guid.NewGuid()}",
            };

            //important !!, or cause docx show image failed: not have enough memory.
            ms.Position = 0;    
            return result;
        }
        */

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
