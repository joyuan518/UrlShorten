using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UrlShorten.WebAPI.Models
{
    public class GetShortenUrlResponse
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
