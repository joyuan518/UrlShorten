using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UrlShorten.WebAPI.Models
{
    public class CreateUserRequest
    {
        [Required]
        [MaxLength(50)]
        public string UserId
        {
            get;
            set;
        }

        [Required]
        [MaxLength(50)]
        public string Name
        {
            get;
            set;
        }

        [Required]
        [MaxLength(80)]
        [EmailAddress]
        public string Email
        {
            get;
            set;
        }

        [Required]
        [MaxLength(50)]
        public string Password
        {
            get;
            set;
        }
    }
}
