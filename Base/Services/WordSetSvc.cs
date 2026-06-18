using Base.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;
using Pipelines.Sockets.Unofficial.Arenas;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Draw = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;

namespace Base.Services
{
    //word套表
    public class WordSetSvc
    {
        //checked char type(yes/no)
        public const int Checkbox = 1;  //checkbox
        public const int Radio = 2;     //radio
        public const int CharV = 3;     //V char

        public const string Childs = "Childs";  //childs欄位名稱
        public const string Child = "Child";    //childs欄位名稱

        //instance variables
        private bool _isOk = false;
        private WordprocessingDocument _docx = null!;
        private MemoryStream _ms = null!;
        private Body _tplBody = null!;

        //constructor
        public WordSetSvc(string tplPath)
        {
            // 1. 檢查模板檔案
            if (!File.Exists(tplPath))
            {
                _Log.Error($"WordSetSvc() constructor failed, no template file ({tplPath})");
                return;
                //await _Log.ErrorRootA($"_Word.cs TplToMsA() no template file ({tplPath})");
                //return null;
            }

            // 開啟 Word 文件 (唯讀)
            //using var fs = new FileStream(tplPath, FileMode.Open, FileAccess.Read);

            // 2. 將模板內容讀入記憶體流
            _ms = new MemoryStream();
            File.OpenRead(tplPath).CopyTo(_ms);
            _ms.Position = 0;  // 重設流位置

            _docx = WordprocessingDocument.Open(_ms, true);
            _tplBody = _docx.MainDocumentPart!.Document!.Body!;
            _isOk = true;
        }

        public bool IsOk()
        {
            return _isOk;
        }

        /// <summary>
        /// 一筆資料產生memorystream
        /// </summary>
        /// <param name="row">可為json或model, 包含Child或Childs欄位</param>
        /// <param name="images"></param>
        /// <returns></returns>
        public MemoryStream? RowToMs(dynamic row, List<WordImageDto>? images = null)
        {
            FillRow(_tplBody, row);

            //fill images
            FillImages(images);

            _docx.MainDocumentPart!.Document!.Save();
            _docx.Dispose();
            _ms.Position = 0;
            return _ms;
        }

        /// <summary>
        /// _HttpWord.MsByTplRow -> _Word.TplToMsA -> TplRowsToMsA
        /// word to memoryStream for convert pdf
        /// </summary>
        /// <param name="rows">可為json或model</param>
        /// <param name="images"></param>
        /// <returns></returns>
        public MemoryStream? RowsToMs(IEnumerable<dynamic> rows, List<WordImageDto>? images = null)
        {
            //body.RemoveAllChildren(); // 清空原內容
            var hasPage = false;
            var newBody = new Body();
            foreach (var row in rows)
            {
                //clone whole template body
                var newPage = _tplBody.CloneNode(true);

                FillRow(newPage, row);

                //只 append children，不要 append body
                foreach (var child in newPage.ChildElements)
                {
                    newBody.Append(child.CloneNode(true));
                }

                //移除 SectionProperties ??
                //newPage.RemoveAllChildren<SectionProperties>();

                //add page break
                if (hasPage)
                {
                    newBody.Append(new Paragraph(
                        new Run(new Break { Type = BreakValues.Page })
                    ));
                }
                hasPage = true;
            }

            //fill images
            FillImages(images);

            //替換body
            _docx.MainDocumentPart!.Document!.Body = newBody;
            _docx.MainDocumentPart.Document.Save();
            _docx.Dispose();
            _ms.Position = 0;
            return _ms;
        }

        private void FillRow(OpenXmlElement page, dynamic row)
        {
            //fill childs first(只存在table), 減少row的欄位
            JObject json = (row is JObject)
                ? (row as JObject)!
                : JObject.FromObject(row);
            var childs = json[Childs] as JArray;
            var child = json[Child] as JArray;
            if (childs == null && child != null)
                childs = [child];

            //fill main table
            var texts = page
                .Descendants<Text>()
                .ToList();

            foreach (var text in texts ?? [])
            {
                // 使用類字典加速替換
                foreach (var item in json)
                {
                    if (text!.Text.Contains(item.Key))
                        text.Text = text.Text.Replace(item.Key, item.Value!.ToString());
                }
            }

            //fill childs
            if (childs!.Any())
            {
                for (var i = 0; i < childs!.Count; i++)
                    FillTable(page, i, childs[i] as JArray);
            }
        }

        /// <summary>
        /// 將資料填入word table(only)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="rows">可以是json或model</param>
        private void FillTable(OpenXmlElement page, int index, JArray? jsons)
        {
            if (jsons == null || !jsons.Any()) return;

            // 找到包含指定標記的表格列            
            var tag = $"[!{index}]";    // 找含有 [!x] 的範本列, base 0
            var oldTplRow = page.Elements<Table>()
                .SelectMany(t => t.Elements<TableRow>())
                .FirstOrDefault(r => r.Elements<TableCell>().Any(c => c.InnerText.Contains(tag)));
            if (oldTplRow == null) return;

            /*
            // 清除tplRow的欄位標記
            var cell = oldTplRow.Elements<TableCell>()
                .First(c => c.InnerText.Contains(tag));
            foreach (var paragraph in cell.Elements<Paragraph>())
                foreach (var run in paragraph.Elements<Run>())
                    foreach (var text in run.Elements<Text>())
                        text.Text = text.Text.Replace(tag, "");
            */

            // 取得表格
            var oldTable = (oldTplRow.Parent as Table)!;
            var tplRow = (TableRow)oldTplRow.CloneNode(true);
            oldTplRow.Remove();
            var table = (Table)oldTable.CloneNode(true);    //new table, 不含範本列

            //if (table == null) return;

            // 取得tplRow的欄位資訊
            var idNums = new List<IdNumDto>();
            //var row0 = (JObject)rows[0];
            var cellIdxs = oldTplRow.Elements<TableCell>()
                .Select((cell, idx) => new { cell, idx })
                .ToList();
            foreach (var item in (JObject)jsons[0])
            {
                var findData = cellIdxs
                    .FirstOrDefault(ci => ci.cell.InnerText.Contains(item.Key));
                if (findData != null)
                {
                    idNums.Add(new IdNumDto
                    {
                        Id = item.Key,
                        Num = findData.idx,
                    });
                }
            }

            // 新增資料列
            foreach (JObject json in jsons)
            {
                // 複製範本列
                var newRow = (TableRow)oldTplRow.CloneNode(true);

                // 動態替換欄位，例如 [Name]、[Age]
                foreach (var item in idNums)
                {
                    var value = json[item.Id]?.ToString() ?? "";
                    var cell2 = newRow.Elements<TableCell>().ElementAtOrDefault(item.Num);
                    if (cell2 != null)
                        CellSetText(cell2, value);
                }

                // 將新列加入表格
                table.AppendChild(newRow);
            }

            // 新增完成後刪除範本列
            //tplRow.Remove();

            //寫入 root
            oldTable.Parent!.ReplaceChild(table, oldTable);
        }

        /// <summary>
        /// 設定cell文字, 同時保留style !!
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="value"></param>
        private void CellSetText(TableCell cell, string value)
        {
            var para = cell.Elements<Paragraph>().FirstOrDefault() ?? new Paragraph();
            var paraProps = para.GetFirstChild<ParagraphProperties>();
            var runProps = para.Elements<Run>().FirstOrDefault()?.GetFirstChild<RunProperties>();

            para.RemoveAllChildren();

            if (paraProps != null) para.AppendChild(paraProps.CloneNode(true));

            var run = new Run();
            if (runProps != null) run.AppendChild(runProps.CloneNode(true));
            run.AppendChild(new Text(value));

            para.AppendChild(run);

            if (!cell.Elements<Paragraph>().Any()) cell.Append(para);
        }

        /// <summary>
        /// word內使用anchor類型圖案, 直接寫入 _docx !!
        /// </summary>
        /// <param name="image"></param>
        private void FillImages(List<WordImageDto>? images)
        {
            var mainPart = _docx.MainDocumentPart;
            if (mainPart == null || images == null || images.Count == 0) return;

            foreach (var image in images)
            {
                // 找到指定名稱的圖片 (非視覺屬性 name = imageDto.Code)
                var pic = mainPart.Document!
                    .Descendants<DW.Inline>()
                    .FirstOrDefault(inl => inl.DocProperties?.Description == image.Code);
                if (pic == null) continue;

                //var aa = mainPart.Document.Descendants<D.Blip>()
                //    .ToList();

                // 取得圖片嵌入 (blip)
                var blip = pic.Descendants<Draw.Blip>().FirstOrDefault();
                if (blip == null) continue;

                // 取得圖片大小 Extents
                var ext = pic.Descendants<Draw.Extents>().FirstOrDefault();
                if (ext == null) continue;

                long width = ext!.Cx!;
                long height = ext!.Cy!;

                // 讀取新圖片檔案 Bytes
                byte[] picBytes = File.ReadAllBytes(image.FilePath);

                // 刪除舊圖片 Part
                var oldRelId = blip.Embed?.Value;
                if (string.IsNullOrEmpty(oldRelId)) continue;

                //var oldImagePart = (ImagePart)mainPart.GetPartById(oldRelId);
                //mainPart.DeletePart(oldImagePart);
                if (mainPart.Parts.Any(p => p.RelationshipId == oldRelId))
                {
                    var oldImagePart = (ImagePart)mainPart.GetPartById(oldRelId);
                    mainPart.DeletePart(oldImagePart);
                }

                // 新增圖片 Part
                var newImagePart = mainPart.AddImagePart(ImagePartType.Png);
                using (var stream = new MemoryStream(picBytes))
                {
                    newImagePart.FeedData(stream);
                }

                // 更新圖片 Embed ID
                blip!.Embed!.Value = mainPart.GetIdOfPart(newImagePart);

                // 保持圖片大小
                ext.Cx = width;
                ext.Cy = height;
            }
        }


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
        private void ValueToChecks(JObject row, string value, string preFid, 
            int startNo, int endNo, int type = Checkbox)
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
            return type switch
            {
                Checkbox => status ? "■" : "□",
                Radio => status ? "●" : "○",
                _ => status ? "V" : "",     //default is checkbox
            };
        }

    }//class
}
