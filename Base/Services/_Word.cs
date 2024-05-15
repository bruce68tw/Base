﻿using Base.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//for docx image
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace Base.Services
{
    public static class _Word
    {
        //word carrier
        //public const string Carrier = "<w:br/>";
        //public const string PageBreak = "<w:p><w:pPr><w:sectPr><w:type w:val=\"nextPage\" /></w:sectPr></w:pPr></w:p>";

        //checked char type(yes/no)
        public const int Checkbox = 1;  //checkbox
        public const int Radio = 2;     //radio
        public const int CharV = 3;     //V char

        /// <summary>
        /// merge word files into one
        /// </summary>
        /// <param name="srcFiles">source word files/param>
        /// <param name="toFile">target word file</param>
        /// <param name="deleteSrc">delete source file or not</param>
        public static void MergeFiles(string[] srcFiles, string toFile, bool deleteSrc)
        {
            //copy first file to target
            File.Copy(srcFiles[0], toFile, true);

            using (var docx = WordprocessingDocument.Open(toFile, true))
            {
                var mainPart = docx.MainDocumentPart;

                //skip first file
                for (var i = 1; i < srcFiles.Length; i++)
                {
                    //add page break
                    mainPart!.Document.Body!.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));

                    //add file
                    var altChunkId = "AltChunkId" + i;
                    var chunk = mainPart.AddAlternativeFormatImportPart(
                        AlternativeFormatImportPartType.WordprocessingML, altChunkId);
                    using (var fileStream = File.Open(srcFiles[i], FileMode.Open))
                    {
                        chunk.FeedData(fileStream);
                    }
                    var altChunk = new AltChunk
                    {
                        Id = altChunkId
                    };
                    mainPart.Document.Body.InsertAfter(altChunk, mainPart.Document.Body.Elements<Paragraph>().Last());
                }
                mainPart!.Document.Save();
                //docx.Close();
            }

            //delete source files if need
            if (deleteSrc)
            {
                foreach (var file in srcFiles)
                    File.Delete(file);
            }
        }

        /// <summary>
        /// fill template string and return row string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowTpl"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string TplFillRow<T>(string rowTpl, T row)
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

        /// <summary>
        /// fill template string and return rows string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowTpl"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static string TplFillRows(string rowTpl, IEnumerable<dynamic> rows)
        {
            if (!rows.Any()) return "";

            //var rows = (List<T>)row0s;
            //if (rows.Count == 0)
            //    return "";

            var props = rows.First().GetType().GetProperties();
            var result = "";
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
            return result;
        }

        //word doc add image, and convert image to text
        public static void DocxAddImage(WordprocessingDocument docx, ref string text, List<WordImageDto> images)
        {
            if (images.Count == 0) return;

            var mainPart = docx.MainDocumentPart;
            foreach (var image in images)
            {
                //var imagePath = images[i];
                //var width = Convert.ToDouble(images[i + 1]);
                //var height = Convert.ToDouble(images[i + 2]);
                //var tag = images[i + 3];
                var imageService = new WordImageSvc(image.FilePath, image.Width, image.Height);
                var newText = "";
                if (imageService.DataStream != null)
                {
                    //TODO: how multiple images ??
                    var imagePart = mainPart!.AddImagePart(ImagePartType.Jpeg);
                    imagePart.FeedData(imageService.DataStream);
                    var imagePartId = mainPart.GetIdOfPart(imagePart);
                    newText = GetImageRun(imagePartId, imageService).InnerXml;
                }

                text.Replace(image.Tag, newText);
            }
        }

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

        //check docx content has error or not
        public static bool IsDocxValid(WordprocessingDocument docx)
        {
            OpenXmlValidator validator = new OpenXmlValidator();
            var errors = validator.Validate(docx);
            foreach (ValidationErrorInfo error in errors)
            {
                var aa = error.Description;
            }
            return (errors.Count() == 0);
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

        //for add word image
        //http://blog.darkthread.net/post-2017-11-06-insert-image-to-docx.aspx
        //https://msdn.microsoft.com/zh-tw/library/office/bb497430.aspx
        public static Run GetImageRun(string imagePartId, WordImageSvc imageService)
        {
            // Define the reference of the image.
            var element =
                 new Drawing(
                     new DW.Inline(
                         //Size of image, unit = EMU(English Metric Unit)
                         //1 cm = 360000 EMUs
                         new DW.Extent() { Cx = imageService.WidthInEMU, Cy = imageService.HeightInEMU },
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
                             Name = imageService.ImageName,
                         },
                         new DW.NonVisualGraphicFrameDrawingProperties(
                             new A.GraphicFrameLocks() { NoChangeAspect = true }),
                         new A.Graphic(
                             new A.GraphicData(
                                 new PIC.Picture(
                                     new PIC.NonVisualPictureProperties(
                                         new PIC.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = imageService.FileName,
                                         },
                                         new PIC.NonVisualPictureDrawingProperties()),
                                     new PIC.BlipFill(
                                         new A.Blip(
                                             new A.BlipExtensionList(
                                                 new A.BlipExtension()
                                                 {
                                                     Uri =
                                                        "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                 })
                                         )
                                         {
                                             Embed = imagePartId,
                                             CompressionState =
                                             A.BlipCompressionValues.Print
                                         },
                                         new A.Stretch(
                                             new A.FillRectangle())),
                                     new PIC.ShapeProperties(
                                         new A.Transform2D(
                                             new A.Offset() { X = 0L, Y = 0L },
                                             new A.Extents()
                                             {
                                                 Cx = imageService.WidthInEMU,
                                                 Cy = imageService.HeightInEMU
                                             }),
                                         new A.PresetGeometry(
                                             new A.AdjustValueList()
                                         )
                                         { Preset = A.ShapeTypeValues.Rectangle }))
                             )
                             { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                     )
                     {
                         DistanceFromTop = (UInt32Value)0U,
                         DistanceFromBottom = (UInt32Value)0U,
                         DistanceFromLeft = (UInt32Value)0U,
                         DistanceFromRight = (UInt32Value)0U,
                         //EditId = "50D07946", //remark line, cause word 2007 cannot open !!
                     });
            return new Run(element);
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
