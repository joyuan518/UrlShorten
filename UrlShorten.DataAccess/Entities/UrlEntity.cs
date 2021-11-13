using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Bson.Serialization.Attributes;

namespace UrlShorten.DataAccess.Entities
{
    internal class UrlEntity : BaseEntity
    {
        [BsonElement("token")]
        public string Token
        {
            get;
            set;
        }

        [BsonElement("url")]
        public string OriginalUrl
        {
            get;
            set;
        }

        [BsonElement("userid")]
        public string UserId
        {
            get;
            set;
        }

        [BsonElement("clickcount")]
        public int ClickCount
        {
            get;
            set;
        }

        [BsonElement("createtime")]
        public DateTime CreateTime
        {
            get;
            set;
        }
    }
}
