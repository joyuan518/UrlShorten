using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShorten.WebAPI.Models.Configuration
{
    /// <summary>
    /// Configuration class for the settings needed for ShortenUrl api
    /// </summary>
    public class ApiConfiguration
    {
        /// <summary>
        /// The internet url of the host of this ShortenUrl API
        /// </summary>        
        public string HostUrl
        {
            get;
            set;
        }

        /// <summary>
        /// The duration that how long a item will be cached before it expires, in minutes
        /// </summary>
        public int CacheDuration
        {
            get;
            set;
        }
    }
}
