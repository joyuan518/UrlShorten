using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Bson.Serialization.Attributes;

namespace UrlShorten.DataAccess.Entities
{
    internal class UserEntity : BaseEntity
    {
        [BsonElement("userid")]
        public string UserId
        {
            get;
            set;
        }

        [BsonElement("name")]
        public string Name
        {
            get;
            set;
        }

        [BsonElement("email")]
        public string Email
        {
            get;
            set;
        }

        [BsonElement("pwdhash")]
        public string PasswordHash
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
