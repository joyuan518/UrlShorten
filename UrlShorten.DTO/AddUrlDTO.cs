using System;

namespace UrlShorten.DTO
{
    /// <summary>
    /// Data transfer object for adding a url entry
    /// </summary>
    public class AddUrlDTO
    {
        /// <summary>
        /// The user id of the user
        /// </summary>
        public string UserId
        {
            get;
            set;
        }

        /// <summary>
        /// The token generated for an url
        /// </summary>
        public string UrlToken
        {
            get;
            set;
        }

        /// <summary>
        /// The orginal url
        /// </summary>
        public string Url
        {
            get;
            set;
        }
    }
}
