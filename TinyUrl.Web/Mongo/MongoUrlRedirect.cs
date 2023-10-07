using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyUrl.Dal.Mongo.Entities
{

    public class MongoUrlRedirect
    {
        [BsonId]
        public string ShortUrl { get; set; }

        public string LongUrl { get; set; }
    }

    public class MongoLongUrl
    {
        [BsonId]
        public string LongUrl { get; set; }
    }
}
