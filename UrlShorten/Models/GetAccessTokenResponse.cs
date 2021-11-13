using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShorten.WebAPI.Models
{
    public class GetAccessTokenResponse
    {
        /// <summary>
        /// The jwt token which can be used for the API calls
        /// </summary>
        public string Token
        {
            get;
            set;
        }
    }
}
