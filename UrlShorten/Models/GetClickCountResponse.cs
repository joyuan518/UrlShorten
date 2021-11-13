using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShorten.WebAPI.Models
{
    public class GetClickCountResponse
    {
        /// <summary>
        /// The url token
        /// </summary>
        public string Token
        {
            get;
            set;
        }

        /// <summary>
        /// The click count of the shorten url
        /// </summary>
        public int ClickCount
        {
            get; set;
        }
    }
}
