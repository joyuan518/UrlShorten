using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UrlShorten.WebAPI.Models
{
    public class CreateShortenUrlRequest
    {
        /// <summary>
        /// The original url to be shorten
        /// </summary>
        [Url]
        [Required]
        [MaxLength(300)]
        public string Url
        { 
            get; 
            set; 
        }
    }
}
