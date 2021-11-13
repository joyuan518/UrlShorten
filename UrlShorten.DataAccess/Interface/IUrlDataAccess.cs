using System;
using System.Threading.Tasks;

using UrlShorten.DTO;

namespace UrlShorten.DataAccess.Interface
{
    public interface IUrlDataAccess
    {
        /// <summary>
        /// Get the original url of the given url token
        /// </summary>
        /// <param name="urlToken"></param>
        /// <returns></returns>
        Task<string> GetUrlAsync(string urlToken);

        /// <summary>
        /// Get the original url of the given url token
        /// </summary>
        /// <param name="urlToken"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<GetUrlDTO> GetUrlAsync(string urlToken, string userId);

        /// <summary>
        /// Check whether a shorten url entry for the user already exists
        /// </summary>
        /// <param name="url"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> ExsitsUrlAsync(string url, string userId);

        /// <summary>
        /// Check whether a url token for the user already exists
        /// </summary>
        /// <param name="urlToken"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> ExsitsTokenAsync(string urlToken, string userId);

        /// <summary>
        /// Add a new shorten url entry
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task AddUrlAsync(AddUrlDTO url);

        /// <summary>
        /// Delete the shorten url entry for the given url token
        /// </summary>
        /// <param name="urlToken"></param>
        Task DeleteUrlAsync(string urlToken);

        /// <summary>
        /// Get the click count of the given url token
        /// </summary>
        /// <param name="urlToken"></param>
        /// <returns></returns>
        Task<int?> GetClickCountAsync(string urlToken);

        /// <summary>
        /// Increase the click count of a shorten url by one
        /// </summary>
        /// <param name="urlToken"></param>
        Task IncreaseClickCountAsync(string urlToken);
    }
}
