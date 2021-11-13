using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShorten.WebAPI.UrlTokenGenerator
{
    /// <summary>
    /// The interface for the classes which can be used for generating short urls
    /// </summary>
    public interface IUrlTokenGenerator : IDisposable
    {
        /// <summary>
        /// Generate and return the token of the given orinal url
        /// </summary>
        /// <param name="originalUrl"></param>
        /// <returns></returns>
        string GetUrlToken(string originalUrl);
    }
}
