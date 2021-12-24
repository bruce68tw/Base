using System.IO;

namespace Base.Models
{
    //word(office) image Run
    public class WordImageRunDto
    {
        public string FileName;
        public string ImageCode;
        public Stream DataStream;
        public long WidthEmu;
        public long HeightEmu;
    }
}