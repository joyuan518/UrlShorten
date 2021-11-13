using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using UrlShorten.DTO;

namespace UrlShorten.DataAccess.Interface
{
    public interface IUserDataAccess
    {
        /// <summary>
        /// Add a new user entry
        /// </summary>
        /// <param name=""></param>
        Task AddUserAsync(AddUserDTO user);

        /// <summary>
        /// Get user info by user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<GetUserDTO> GetUserAsync(string userId);

        /// <summary>
        /// Check whether a user with given user id already exists
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> ExistsUserAsync(string userId);
    }
}
