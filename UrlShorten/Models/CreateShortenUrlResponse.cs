using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShorten.WebAPI.Models
{
    public class CreateShortenUrlResponse
    {
        /// <summary>
        /// The shorten url
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
    }
}
