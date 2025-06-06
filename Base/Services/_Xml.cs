using Newtonsoft.Json;
using System;
using System.IO;
using System.Xml;

namespace Base.Services
{
    public class _Xml
    {
        public static string ToStr(XmlNode node)
        {
            return JsonConvert.SerializeXmlNode(node);
        }

        /// <summary>
        /// get key property(attriibute)
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="keyPath"></param>
        /// <returns></returns>
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
