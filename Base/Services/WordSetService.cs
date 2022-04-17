using Base.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

//for docx image
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Drawing.Pictures;
using WP = DocumentFormat.OpenXml.Drawing.Wordprocessing;

namespace Base.Services
{
    public class WordSetService
    {
        //word carrier
        public const string Carrier = "<w:br/>";
        public const string PageBreak = "<w:p><w:pPr><w:sectPr><w:type w:val=\"nextPage\" /></w:sectPr></w:pPr></w:p>";

        //checked char type(yes/no)
        public const int Checkbox = 1;  //checkbox
        public const int Radio = 2;     //radio
        public const int CharV = 3;     //V char

        //instance variables
        private WordprocessingDocument _docx;

        //template start/end position
        //private int _tplStartPos, _tplEndPos;

        //constructor
        public WordSetService(WordprocessingDocument docx)
        {
            _docx = docx;
        }

        /// <summary>
        /// return page break string
        /// </summary>
        /// <returns></returns>
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
            using var sr = new StreamReader(_docx.MainDocumentPart.GetStream());
            return sr.ReadToEnd();
        }

        /// <summary>
        /// write string into docx mainpart
        /// </summary>
        public void SetMainPartStr(string str)
        {
            //string to docx stream, must use FileMode.Create, or target file can not open !!
            using var sw = new StreamWriter(_docx.MainDocumentPart.GetStream(FileMode.Create));
            sw.Write(str);
        }

        /// <summary>
        /// word docx add images
        /// </summary>
        /// <param name="srcTpl">(ref) source template string</param>
        /// <param name="imageDtos"></param>
        /// <returns>new template string</returns>
        public async Task<string> AddImagesAsync(string srcTpl, List<WordImageDto> imageDtos)
        {
            if (imageDtos == null || imageDtos.Count == 0)
                return "";

            var mainPart = _docx.MainDocumentPart;
            foreach (var imageInfo in imageDtos)
            {
                //get image file info
                var runDto = GetImageRunDto(imageInfo.FilePath);
                ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);
                var imageId = mainPart.GetIdOfPart(imagePart);
                imagePart.FeedData(runDto.DataStream);
                var newRun = GetImageRun(imageId, runDto);

                //find drawing object from srcTpl
                var drawTpl = await GetDrawTplAsync(srcTpl, imageInfo.Code);
                if (drawTpl == null)
                    continue;

                //find graphicData object(map to D.Graphic) from drawing object
                var graphTpl = await GetGraphDataTplAsync(drawTpl.TplStr);
                if (graphTpl == null)
                    continue;

                //replace graphic section
                //newRun.Descendants<D.Graphic>().First().InnerXml cause .docx open failed !!
                var newGraph = await GetGraphDataTplAsync(newRun.InnerXml);
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
        /// return docx body template string
        /// </summary>
        /// <param name="srcText"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public async Task<WordSetTplDto> GetBodyTplAsync(string srcText)
        {
            return await GetRangeTplAsync(srcText, false, "<w:body>", "</w:body>");
        }

        /// <summary>
        /// return tr(row) template string
        /// </summary>
        /// <param name="srcText"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="childIdx"></param>
        /// <returns></returns>
        public async Task<WordSetTplDto> GetRowTplAsync(string srcText, int childIdx = -1)
        {
            var findMid = (childIdx < 0) ? "[!]" : $"[!{childIdx}]";
            var result = await GetRangeTplAsync(srcText, true, "<w:tr ", "</w:tr>", findMid);
            result.TplStr = result.TplStr.Replace(findMid, "");
            return result;
        }

        /// <summary>
        /// get drawing template string for one photo
        /// </summary>
        /// <param name="srcText"></param>
        /// <param name="imageCode"></param>
        /// <returns></returns>
        public async Task<WordSetTplDto> GetDrawTplAsync(string srcText, string imageCode)
        {
            return await GetRangeTplAsync(srcText, true, "<w:drawing>", "</w:drawing>", $"descr=\"{imageCode}\"");
        }

        private async Task<WordSetTplDto> GetGraphDataTplAsync(string srcText)
        {
            return await GetRangeTplAsync(srcText, true, "<a:graphicData", "</a:graphicData>");
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
        private async Task<WordSetTplDto> GetRangeTplAsync(string srcText, bool hasTag, string findStart, 
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
            await _Log.ErrorAsync($"WordSetService.cs GetRangeStr() get empty result.(findStart='{findStart}', findMid='{findMid}')");
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

        private WordImageRunDto GetImageRunDto(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            //pixels / 300(dpi) * 2.54(inch to cm) * 36000(cm to emu)
            const double PixelToEmu = 304.8;    

            var ms = new MemoryStream(File.ReadAllBytes(filePath));
            var img = new Bitmap(ms);   //for get width/height
            var result = new WordImageRunDto()
            {
                DataStream = ms,
                FileName = _File.GetFileName(filePath),
                WidthEmu = Convert.ToInt64(img.Width * PixelToEmu),
                HeightEmu = Convert.ToInt64(img.Height * PixelToEmu),
                ImageCode = $"IMG{_Str.NewId()}",
            };

            //important !!, or cause docx show image failed: not have enough memory.
            ms.Position = 0;    
            return result;
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
                     new WP.Inline(
                         //Size of image, unit = EMU(English Metric Unit)
                         //1 cm = 360000 EMUs
                         new WP.Extent() { Cx = imageRun.WidthEmu, Cy = imageRun.HeightEmu },
                         new WP.EffectExtent()
                         {
                             LeftEdge = 0L,
                             TopEdge = 0L,
                             RightEdge = 0L,
                             BottomEdge = 0L
                         },
                         new WP.DocProperties()
                         {
                             Id = (UInt32Value)1U,
                             Name = imageRun.ImageCode,
                         },
                         new WP.NonVisualGraphicFrameDrawingProperties(
                             new D.GraphicFrameLocks() { NoChangeAspect = true }),
                         new D.Graphic(
                             new D.GraphicData(
                                 new P.Picture(
                                     new P.NonVisualPictureProperties(
                                         new P.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = imageRun.FileName,
                                         },
                                         new P.NonVisualPictureDrawingProperties()),
                                     new P.BlipFill(
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
                                     new P.ShapeProperties(
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
