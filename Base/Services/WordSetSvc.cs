using Base.Models;
using Newtonsoft.Json.Linq;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.Formula;
using NPOI.XWPF.UserModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NPOI.XWPF.UserModel;
using System.Xml;
using NPOI.SS.Formula.Functions;

namespace Base.Services
{
    //word套表
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
        public MemoryStream? GetMs(dynamic row,
            List<dynamic>? childs = null, List<WordImageDto>? images = null)
        {
            //fill row
            FillRow(row);

            //fill childs
            if (childs != null)
                for (var i = 0; i < childs.Count; i++)
                    FillRows(i, childs[i]);

            //add images
            if (_List.NotEmpty(images))
                foreach(var image in images!)
                    AddImage(image);

            var ms = new MemoryStream();
            _docx.Write(ms);
            ms.Position = 0;  // 重置位置
            return ms;
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


        private void CellSetText(XWPFTableCell cell, string value)
        {
            // 清空單元格中的所有段落
            cell.RemoveParagraph(0);

            // 新增一個段落並設置文字
            XWPFParagraph paragraph = cell.AddParagraph();
            XWPFRun run = paragraph.CreateRun();
            run.SetText(value);
        }

        //XWPFTable
        private void FillRows(int index, IEnumerable<dynamic>? rows)
        {
            if (rows == null || !rows.Any()) return;

            //找含有 [x!] 的範本列, base 0
            var tag = $"[!{index}]";
            var tplRow = _docx.Tables
                .SelectMany(t => t.Rows)   // 展開所有表格的所有列
                .FirstOrDefault(r => r.GetTableCells().Any(c => c.GetText().Contains(tag)));
            if (tplRow == null) return;

            //清除標記
            var cell = tplRow.GetTableCells().First(a => a.GetText().Contains(tag));
            cell.SetText(cell.GetText().Replace(tag, ""));

            //get table
            var table = tplRow.GetTable();

            //是否json
            if (_Object.IsJObject(rows.First()))
                FillJsons(table, tplRow, JArray.FromObject(rows));
            else
                FillModels(table, tplRow, rows);
        }

        private void FillJsons(XWPFTable table, XWPFTableRow tplRow, JArray rows)
        {
            //if (!rows.Any()) return;

            //get欄位資訊
            var nos = new List<IdNumDto>();
            var row0 = (JObject)rows[0];
            foreach (var item in row0)
            {
                var fid = item!.Key;
                var index = tplRow.GetTableCells()
                    .Select((cell, index) => new { cell, index })
                    .FirstOrDefault(ci => ci.cell.GetText().Contains($"[{fid}]"))?.index ?? -1;
                if (index >= 0)
                    nos.Add(new IdNumDto()
                    {
                        Id = fid,
                        Num = index,
                    });
            }

            //新增資料列
            foreach (var row in rows)
            {
                //複製範本列
                var newRow = new XWPFTableRow(tplRow.GetCTRow().Copy(), table);

                //動態替換欄位，例如 [Name]、[Age]
                foreach (var no in nos)
                {
                    var value = row[no.Id]!.ToString() ?? "";
                    var cell = newRow.GetCell(no.Num);
                    CellSetText(cell, value);
                }

                // 將新列加入表格
                table.AddRow(newRow);
            }

            // 新增完成後刪除範本列
            table.RemoveRow(table.Rows.IndexOf(tplRow));

        }
        private void FillModels<T>(XWPFTable table, XWPFTableRow tplRow, IEnumerable<T> rows)
        {
            //get欄位資訊
            var nos = new List<int>();
            var props = rows.First()!.GetType().GetProperties(); //減少在loop取值
            foreach (var prop in props)
            {
                var fid = prop.Name;
                var index = tplRow.GetTableCells()
                    .Select((cell, index) => new { cell, index })
                    .FirstOrDefault(ci => ci.cell.GetText().Contains($"[{fid}]"))?.index ?? -1;
                if (index >= 0)
                    nos.Add(index);
            }

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
                    CellSetText(cell, value);
                }

                // 將新列加入表格
                table.AddRow(newRow);
            }

            // 新增完成後刪除範本列
            table.RemoveRow(table.Rows.IndexOf(tplRow));
        }

        /// <summary>
        /// word內使用anchor類型圖案
        /// </summary>
        /// <param name="imageDto"></param>
        private void AddImage(WordImageDto imageDto)
        {
            // 遍歷所有段落和 Run
            var pic = _docx.Paragraphs
                .SelectMany(p => p.Runs)
                .SelectMany(r => r.GetEmbeddedPictures())
                .Where(pic => pic.GetCTPicture().nvPicPr.cNvPr.name == imageDto.Code)
                .FirstOrDefault();
            if (pic == null) return;

            byte[] newPicBytes = File.ReadAllBytes(imageDto.FilePath);

            var oldPicData = pic.GetPictureData();
            int picType = oldPicData.GetPictureType();

            // 記錄舊圖片的尺寸
            var ctpic = pic.GetCTPicture();
            long width = ctpic.spPr.xfrm.ext.cx;
            long height = ctpic.spPr.xfrm.ext.cy;

            // 取得新圖片的關聯 ID
            string newRelatId = _docx.AddPictureData(newPicBytes, picType);

            // 更新圖片的 embed id 指向新圖片
            ctpic.blipFill.blip.embed = newRelatId;

            // 更新圖片尺寸為原始大小
            ctpic.spPr.xfrm.ext.cx = width;
            ctpic.spPr.xfrm.ext.cy = height;
        }


        /*
        private int GetPictureType(string imagePath)
        {
            string extension = Path.GetExtension(imagePath).ToLower();
            return extension switch
            {
                ".emf" => (int)PictureType.EMF,
                ".wmf" => (int)PictureType.WMF,
                ".pict" => (int)PictureType.PICT,
                ".jpeg" or ".jpg" => (int)PictureType.JPEG,
                ".png" => (int)PictureType.PNG,
                ".dib" => (int)PictureType.DIB,
                ".gif" => (int)PictureType.GIF,
                ".tiff" => (int)PictureType.TIFF,
                ".eps" => (int)PictureType.EPS,
                ".bmp" => (int)PictureType.BMP,
                ".wpg" => (int)PictureType.WPG,
                _ => (int)PictureType.JPEG
            };
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

    }//class
}
