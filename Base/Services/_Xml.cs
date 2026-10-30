using Newtonsoft.Json;
using System;
using System.IO;
using System.Xml;

namespace Base.Services
{
    /// <summary>
    /// XML 節點序列化與 XPath 屬性讀取。
    /// </summary>
    public class _Xml
    {
        /// <summary>
        /// 將 XML 節點序列化為 JSON 字串。
        /// </summary>
        public static string ToStr(XmlNode node)
        {
            return JsonConvert.SerializeXmlNode(node);
        }

        /// <summary>
        /// 依 XPath 讀取屬性；檔案或節點缺失回傳 null，XML 格式錯誤仍可能拋出。
        /// </summary>
        public static string? GetKeyProp(string filePath, string keyPath, string propName)
        {
            //check
            if (string.IsNullOrEmpty(filePath))
                return null;

            if (!File.Exists(filePath))
            {
                _Log.Error($"No File: {filePath}");
                return null;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            var node = xmlDoc.SelectSingleNode(keyPath);
            if (node == null)
                return null;
            if (node?.Attributes == null || node?.Attributes.Count == 0)
                return null;
            
            var prop = node?.Attributes[propName];
            return (prop == null)
                ? null
                : prop.Value;
        }

    }//class
}
