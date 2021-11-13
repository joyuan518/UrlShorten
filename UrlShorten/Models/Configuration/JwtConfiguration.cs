using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShorten.WebAPI.Models.Configuration
{
    public class JwtConfiguration
    {
        /// <summary>
        /// The issuer of the token
        /// </summary>
        public string Issuer
        {
            get;
            set;
        }

        /// <summary>
        /// The audience of the token
        /// </summary>
        public string Audience
        {
            get;
            set;
        }

        /// <summary>
        /// The server secret key which is used to sign the token payload
        /// </summary>
        public string ServerSecret
        {
            get;
            set;
        }

        /// <summary>
        /// The duration in which the token is going to expire, in hours
        /// </summary>
        public int ExpirationDuration
        {
            get;
            set;
        }
    }
}
