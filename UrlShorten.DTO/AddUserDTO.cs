using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShorten.DTO
{
    public class AddUserDTO
    {
        public string UserId
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public string PasswordHash
        {
            get;
            set;
        }
    }
}
