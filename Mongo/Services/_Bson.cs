using MongoDB.Bson;
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

    }//class
}