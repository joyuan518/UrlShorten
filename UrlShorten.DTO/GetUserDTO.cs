using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShorten.DTO
{
    public class GetUserDTO
    {
        public string UserId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The encoded password hash string
        /// </summary>
        public string PasswordHash
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public DateTime CreateTime
        {
            get;
            set;
        }
    }
}
