using Newtonsoft.Json;
using System.Xml;

namespace Base.Services
{
    public class _Xml
    {
        public static string ToStr(XmlNode node)
        {
            return JsonConvert.SerializeXmlNode(node);
        }

    }//class
}
