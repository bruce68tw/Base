using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;

namespace Mongo.Services
{
    /// <summary>
    /// package Db class to static 
    /// </summary>
    public class _Bson
    {
        /// <summary>
        /// json to bson
        /// </summary>
        /// <param name="json"></param>
        /// <returns>BsonDocument</returns>
        public static BsonDocument JsonToBson(JObject json)
        {
            string str = json.ToString(Newtonsoft.Json.Formatting.None);
            return BsonDocument.Parse(str);
        }

        /// <summary>
        /// 將 JArray 轉換為 List<BsonDocument>
        /// </summary>
        public static List<BsonDocument>? JsonsToBsons(JArray? jsons)
        {
            return (jsons == null)
                ? default
                : BsonSerializer.Deserialize<List<BsonDocument>>(jsons.ToString());
        }

        /// <summary>
        /// bson to model
        /// </summary>
        public static T? ToModel<T>(BsonDocument bson)
        {
            return (bson == null)
                ? default
                : BsonSerializer.Deserialize<T>(bson);
        }

    }//class
}