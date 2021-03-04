using Base.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Web;
using System;
using System.Collections.Generic;
using Base.Models;
using Microsoft.AspNetCore.Http;

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
        public static void EchoByTplRow(JObject row, string tplPath, string fileName, List<WordImageDto> images = null)
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

        public static void EchoStream(Stream stream, string fileName)
        {
            //response to client, must close docx first, 
            //so put code here, or docx file will get wrong !!
            var response = _Web.GetResponse();

            //consider IE
            //response.AppendHeader("Content-Disposition", "attachment;filename=" + fileName);
            var browser = _Web.GetRequest().Headers["User-Agent"].ToString();
            if (browser != null && browser.Equals("ie", StringComparison.OrdinalIgnoreCase))
                response.Headers.Append("Content-Disposition", "attachment; filename*=UTF-8''" + HttpUtility.UrlPathEncode(fileName) + "\"");
            else
                response.Headers.Append("Content-Disposition", "attachment; filename=\"" + HttpUtility.UrlPathEncode(fileName) + "\"");

            //response.ContentType = "application/vnd.ms-word.document";
            response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            //stream.Flush();
            stream.Position = 0;
            stream.CopyToAsync(response.Body);
            //response.Flush();
            response.Body.FlushAsync();
            //response.End();
            //response.Body..EndWrite();
        }

        #region remark code
        /*
        public static void ExportFile(string filePath, string fileName, bool delete)
        {
            byte[] tplBytes = File.ReadAllBytes(filePath);
            using (var mem = new MemoryStream())
            {
                //template file -> memory
                mem.Write(tplBytes, 0, (int)tplBytes.Length);

                ExportStream(mem, fileName);

                if (delete)
                    File.Delete(filePath);
            }
        }
        */

        /*
        //檢查產生的 docx 內容是否正確
        private static bool IsDocxValid(WordprocessingDocument docx)
        {
            OpenXmlValidator validator = new OpenXmlValidator();
            var errors = validator.Validate(docx);
            foreach (ValidationErrorInfo error in errors)
                Debug.Write(error.Description);
            return (errors.Count() == 0);
        }

        /// <summary>
        /// 設定多個 checkbox欄位內容
        /// </summary>
        /// <param name="row">原始資料列</param>
        /// <param name="value">欄位值</param>
        /// <param name="preFid">要設定的欄位前置字串</param>
        /// <param name="startNo">欄位開始號</param>
        /// <param name="endNo">欄位結束號</param>
        /// <param name="type">符號種類, 1:方形, 2:圓鈕, 3:打勾</param>
        public static void ValueToChecks(JObject row, string value, string preFid, int startNo, int endNo, int type = _Check)
        {
            for (var i=startNo; i<=endNo; i++)
            {
                var fid = preFid + i;
                row[fid] = YesNo(value == i.ToString(), type);
            }
        }

        //傳回Checkbox or Radio button
        public static string YesNo(string status, int type = _Check)
        {
            return YesNo(status == "1", type);
        }
        public static string YesNo(bool status, int type = _Check)
        {
            if (type == _Check)
                return status ? "■" : "□";
            else if (type == _Radio)
                return status ? "●" : "○";
            else 
                return status ? "V" : "";
        }        
        */

        /// <summary>
        /// docx增加圖檔
        /// http://blog.darkthread.net/post-2017-11-06-insert-image-to-docx.aspx
        /// https://msdn.microsoft.com/zh-tw/library/office/bb497430.aspx
        /// </summary>
        /// <param name="wordDoc"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        //public static Run GenerateImageRun(WordprocessingDocument wordDoc, WordImageModel img)
        //public static void AddImage(MainDocumentPart mainPart, string imagePath, double width, double height, string tag)
        //{
        //    //如果圖檔為空白或不存在, 則清除 template的 tag
        //    var run = GetImageRun(mainPart, imagePath, width, height);
        //    //var cat2Img = new ImageData(imagePath);
        //    //var run = GetImageRun2(mainPart, cat2Img);

        //    //get image node
        //    var imageNode = mainPart.Document.Body.Descendants()
        //        .Single(o => o.LocalName == "r" && o.InnerText == tag);

        //    //temp test
        //    mainPart.Document.Body.AppendChild(new Paragraph(run));

        //    /*
        //    //將 InnerXML 置換成圖片 Run 的 InnerXML
        //    imageNode.InnerXml = (run == null)
        //        ? ""
        //        : run.InnerXml;
        //    */
        //}

        //public static void AddImage(MainDocumentPart mainPart, string imagePath, double width, double height, string tag)
        //public static void AddImage(ref string docText, MainDocumentPart mainPart, string imagePath, double width, double height, string tag)
        //{
        //    //如果圖檔為空白或不存在, 則return null
        //    //if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
        //    //    return null;

        //    //temp return;
        //    //return;

        //    /*
        //    //get image node
        //    var imageNode = mainPart.Document.Body.Descendants()
        //        .Single(o => o.LocalName == "r" && o.InnerText == tag);
        //    */

        //    var image = new WordImage(imagePath, width, height);
        //    /*
        //    if (image == null)
        //    {
        //        imageNode.InnerXml = "";
        //        return;
        //    }
        //    */

        //    //MainDocumentPart mainPart = wordDoc.MainDocumentPart;

        //    var imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);
        //    imagePart.FeedData(image.DataStream);

        //    /*
        //    var fileName = @"d:\_temp\Cat.jpg";
        //    using (FileStream stream = new FileStream(fileName, FileMode.Open))
        //    {
        //        imagePart.FeedData(stream);
        //    }
        //    */
        //    var imagePartId = mainPart.GetIdOfPart(imagePart);

        //    //var run = GetImageRun(imagePartId, null);
        //    var run = GetImageRun(imagePartId, image);
        //    docText = docText.Replace(tag, run.InnerXml);
        //    /*
        //    imageNode.InnerXml = run.InnerXml;
        //    //temp test
        //    //mainPart.Document.Body.AppendChild(new Paragraph(run));
        //    var run2 = new Paragraph(run);
        //    run2.InnerXml.ToString();
        //    */
        //}

        /*
        // http://blog.darkthread.net/post-2017-11-06-insert-image-to-docx.aspx
        // https://msdn.microsoft.com/zh-tw/library/office/bb497430.aspx
        private static Run GetImageRun(string imagePartId, WordImage img)
        {
            // Define the reference of the image.
            var element =
                 new Drawing(
                     new DW.Inline(
                         //Size of image, unit = EMU(English Metric Unit)
                         //1 cm = 360000 EMUs
                         new DW.Extent() { Cx = img.WidthInEMU, Cy = img.HeightInEMU },
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
                             Name = img.ImageName,
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
                                             Name = img.FileName,
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
                                                 Cx = img.WidthInEMU,
                                                 Cy = img.HeightInEMU
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
                         //EditId = "50D07946", //word 2007 無法開啟 !!
                     });
            return new Run(element);
        }        
        */
        #endregion

    }//class
}
