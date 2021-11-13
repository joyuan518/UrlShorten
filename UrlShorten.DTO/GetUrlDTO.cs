using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShorten.DTO
{
    public class GetUrlDTO
    {
        /// <summary>
        /// The original url of a given token
        /// </summary>
        public string Url
        {
            get;
            set;
        }

        /// <summary>
        /// The token of the url
        /// </summary>
        public string Token
        {
            get;
            set;
        }

        /// <summary>
        /// The create time of the entry
        /// </summary>
        public DateTime CreatedTime
        {
            get;
            set;
        }
    }
}
