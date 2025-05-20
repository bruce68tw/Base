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
        //checked char type(yes/no)
        public const int Checkbox = 1;  //checkbox
        public const int Radio = 2;     //radio
        public const int CharV = 3;     //V char

        //instance variables
        private WordprocessingDocument _docx = null!;
        private MemoryStream _ms = null!;

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
        public MemoryStream? GetResultMs(dynamic? row,
            List<dynamic>? childs = null, List<WordImageDto>? images = null)
        {
            //fill childs first(只存在table), 減少row的欄位
            if (childs != null)
                for (var i = 0; i < childs.Count; i++)
                    FillTable(i, childs[i]);

            //fill row, 包含段落和table
            if (row != null)
                FillMain(row);

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

        private void FillMain(dynamic sourceRow)
        {
            if (sourceRow == null || _docx.MainDocumentPart?.Document.Body == null)
                return;

            Dictionary<string, string> row;
            if (_Object.IsJObject(sourceRow))
            {
                var obj = sourceRow as JObject;
                row = obj!.Properties()
                    .ToDictionary(
                        prop => $"[{prop.Name}]",   //包含[]方便後面運算
                        prop => prop.Value!.ToString()
                    );
            }
            else
            {
                var obj = (object)sourceRow!;
                row = obj.GetType().GetProperties()
                    .ToDictionary(
                        prop => $"[{prop.Name}]",
                        prop => prop.GetValue(obj)?.ToString() ?? string.Empty
                    );
            }
            FillMainByRow(row);
        }

        /*
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
        */

        private void FillMainByRow(Dictionary<string, string> row)
        {
            // 取得所有文字節點，篩選出含有任何屬性標記的節點
            var body = _docx.MainDocumentPart?.Document.Body!;
            var texts = body.Elements<Paragraph>()
                .Concat(body.Elements<Table>()
                    .SelectMany(table => table.Descendants<Paragraph>()))
                .SelectMany(para => para.Elements<Run>())
                .Select(run => run.GetFirstChild<Text>())
                .Where(text => text != null && row.Keys.Any(key => text!.Text.Contains(key)))
                .ToList();

            foreach (var text in texts ?? [])
            {
                // 使用字典加速替換
                foreach (var col in row)
                {
                    if (text!.Text.Contains(col.Key))
                        text.Text = text.Text.Replace(col.Key, col.Value);
                }
            }
        }

        /*
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
        */

        /// <summary>
        /// 將資料填入table(only)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sourceRows"></param>
        private void FillTable(int index, IEnumerable<dynamic>? sourceRows)
        {
            if (sourceRows == null || !sourceRows.Any()) return;

            // 找到包含指定標記的表格列            
            var tag = $"[!{index}]";    // 找含有 [!x] 的範本列, base 0
            var tplRow = _docx.MainDocumentPart?.Document.Body?.Elements<Table>()
                .SelectMany(t => t.Elements<TableRow>())
                .FirstOrDefault(r => r.Elements<TableCell>()
                    .Any(c => c.InnerText.Contains(tag)));
            if (tplRow == null) return;

            // 清除tplRow的標記
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
            List<Dictionary<string, string>> rows = [];
            if (_Object.IsJObject(sourceRows.First()))
            {
                foreach (var sourceRow in sourceRows)
                {
                    var obj = sourceRow as JObject;
                    rows.Add(obj!.Properties()
                        .ToDictionary(
                            prop => $"[{prop.Name}]",   //包含[]方便後面運算
                            prop => prop.Value!.ToString()
                        ));
                }
            }
            else
            {
                foreach (var sourceRow in sourceRows)
                {
                    var obj = (object)sourceRow!;
                    rows.Add(obj.GetType().GetProperties()
                        .ToDictionary(
                            prop => $"[{prop.Name}]",   //包含[]方便後面運算
                            prop => prop.GetValue(obj)?.ToString() ?? string.Empty
                        ));
                }
            }

            FillTableByRows(table, tplRow, rows);
        }

        private void FillTableByRows(Table table, TableRow tplRow, List<Dictionary<string, string>> rows)
        {
            // 取得tplRow的欄位資訊
            var idNums = new List<IdNumDto>();
            //var row0 = (JObject)rows[0];
            var cellIdxs = tplRow.Elements<TableCell>()
                .Select((cell, idx) => new { cell, idx })
                .ToList();
            foreach (var col in rows[0])
            {
                var findData = cellIdxs
                    .FirstOrDefault(ci => ci.cell.InnerText.Contains(col.Key));
                if (findData != null)
                {
                    idNums.Add(new IdNumDto
                    {
                        Id = col.Key,
                        Num = findData.idx,
                    });
                }
            }

            // 新增資料列
            foreach (var row in rows)
            {
                // 複製範本列
                var newRow = (TableRow)tplRow.CloneNode(true);

                // 動態替換欄位，例如 [Name]、[Age]
                foreach (var item in idNums)
                {
                    var value = row[item.Id]?.ToString() ?? "";
                    var cell = newRow.Elements<TableCell>().ElementAtOrDefault(item.Num);
                    if (cell != null)
                        CellSetText(cell, value);
                }

                // 將新列加入表格
                table.AppendChild(newRow);
            }

            // 新增完成後刪除範本列
            tplRow.Remove();
        }

        /*
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
        */

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

            //var oldImagePart = (ImagePart)mainPart.GetPartById(oldRelId);
            //mainPart.DeletePart(oldImagePart);
            if (mainPart.Parts.Any(p => p.RelationshipId == oldRelId))
            {
                var oldImagePart = (ImagePart)mainPart.GetPartById(oldRelId);
                mainPart.DeletePart(oldImagePart);
            }

            // 新增圖片 Part
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


        #region remark code
        /*
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
