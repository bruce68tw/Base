using System;
using System.Drawing;
using System.IO;

namespace Base.Services
{
    //Word docx image
    //http://blog.darkthread.net/post-2017-11-06-insert-image-to-docx.aspx
    //ImageDate -> WordImageModel
    public class WordImageService
    {
        //private const decimal INCH_TO_CM = 2.54M;
        private const decimal CM_TO_EMU = 360000M;

        public string FileName = string.Empty;
        //public byte[] BinaryData;
        //public Stream DataStream => new MemoryStream(BinaryData);
        public Stream DataStream;

        //public int SourceWidth;
        //public int SourceHeight;
        //public decimal Width;
        //public decimal Height;
        //public long WidthInEMU => Convert.ToInt64(Width * CM_TO_EMU);
        //public long HeightInEMU => Convert.ToInt64(Height * CM_TO_EMU);
        public long WidthInEMU;
        public long HeightInEMU;
        public string ImageName;

        //constructor
        //width/height: unit is celimeter
        //public WordImageModel(string fileName, byte[] data, int dpi = 300)
        public WordImageService(string fileName, double width, double height, int dpi = 300)
        {
            if (!File.Exists(fileName))
                return;

            var data = File.ReadAllBytes(fileName);
            DataStream = new MemoryStream(data);

            FileName = fileName;
            //BinaryData = data;
            Bitmap img = new Bitmap(new MemoryStream(data));
            //SourceWidth = img.Width;
            //SourceHeight = img.Height;
            //Width = ((decimal)SourceWidth) / dpi * INCH_TO_CM;
            //Height = ((decimal)SourceHeight) / dpi * INCH_TO_CM;
            //WidthInEMU = Convert.ToInt64(width / dpi * INCH_TO_CM * CM_TO_EMU);
            //HeightInEMU = Convert.ToInt64(height / dpi * INCH_TO_CM * CM_TO_EMU);
            WidthInEMU = Convert.ToInt64((decimal)width * CM_TO_EMU);
            HeightInEMU = Convert.ToInt64((decimal)height * CM_TO_EMU);

            ImageName = $"IMG_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        /*
        //constructor
        public WordImageModel(string fileName, int dpi = 300) :
            this(fileName, File.ReadAllBytes(fileName), dpi)
        {
        }
        */

        /*
        //get file type
        public ImagePartType ImageType
        {
            get
            {
                var ext = Path.GetExtension(FileName).TrimStart('.').ToLower();
                switch (ext)
                {
                    case "jpg":
                        return ImagePartType.Jpeg;
                    case "png":
                        return ImagePartType.Png;
                    case "":
                        return ImagePartType.Gif;
                    case "bmp":
                        return ImagePartType.Bmp;
                }
                throw new ApplicationException($"Unsupported image type: {ext}");
            }
        }
        */

    }//class
}
