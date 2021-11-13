using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using UrlShorten.WebAPI.Models;
using UrlShorten.WebAPI.Models.Configuration;
using UrlShorten.DataAccess.Interface;
using UrlShorten.DTO;

namespace UrlShorten.WebAPI.Controllers
{
    [ApiController]
    [Route("user")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserDataAccess _dataAccess;
        private readonly JwtConfiguration _jwtConfiguration;
        private readonly ApiConfiguration _apiConfiguration;

        public UserController(ILogger<UserController> logger,
                                IOptions<ApiConfiguration> apiConfiguration,
                                IOptions<JwtConfiguration> jwtConfiguration,
                                IUserDataAccess dataAccess)
        {
            _logger = logger;
            _apiConfiguration = apiConfiguration.Value;
            _jwtConfiguration = jwtConfiguration.Value;
            _dataAccess = dataAccess;
        }

        /// <summary>
        /// Get the access token which is required for the web api
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /user/accesstoken?userid=tom123&amp;password=123456
        /// </remarks>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <response code="200">The token is generated and returned</response>
        /// <response code="400">The request is invalid</response>
        /// <response code="404">The user is not found or the password is incorrect</response>
        [HttpGet]
        [Route("accesstoken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccessToken([FromQuery][Required][MaxLength(50)]string userId
                                                                , [FromQuery][Required][MaxLength(50)] string password)
        {
            try
            {
                var userDto = await _dataAccess.GetUserAsync(userId);

                if (userDto == null)
                {
                    return NotFound(new { error = "User not found." });
                }

                var pwd1 = userDto.PasswordHash;
                var pwd2 = string.Empty;

                using (MD5 md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                    pwd2 = Convert.ToBase64String(hash);
                }

                if (pwd1 != pwd2)
                {
                    return NotFound(new { error = "User password is incorrect." });
                }

                var serverSecretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.ServerSecret));
                var handler = new JwtSecurityTokenHandler();
                var claims = new Claim[] { new Claim("userid", userId) };
                var now = DateTime.Now;

                var token = handler.CreateJwtSecurityToken(_jwtConfiguration.Issuer,
                                                            _jwtConfiguration.Audience,
                                                            new ClaimsIdentity(claims),
                                                            now,
                                                            now.Add(TimeSpan.FromHours(_jwtConfiguration.ExpirationDuration)),
                                                            now,
                                                            new SigningCredentials(serverSecretKey, SecurityAlgorithms.HmacSha256));
                var encodedJwt = handler.WriteToken(token);

                return Ok(new GetAccessTokenResponse { Token = encodedJwt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccessToken method invoked with userId: {0}", userId);
                throw;
            }
        }

        /// <summary>
        /// Create an new user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /user
        ///     {
        ///        "userid": "tom123",
        ///        "name": "Tom Hanks",
        ///        "email": "tom@gmail.com",
        ///        "password": "abc123"
        ///     }
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="201">The new user is created</response>
        /// <response code="400">The request is invalid</response>
        /// <response code="409">The user with the same user id already exists</response>
        [HttpPost]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Post(CreateUserRequest request)
        {
            try
            {
                if (await _dataAccess.ExistsUserAsync(request.UserId))
                {
                    return Conflict(new { error = "User already exists." });
                }

                string pwd = string.Empty;

                using (MD5 md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
                    pwd = Convert.ToBase64String(hash);
                }

                await _dataAccess.AddUserAsync(new AddUserDTO
                {
                    UserId = request.UserId,
                    UserName = request.Name,
                    Email = request.Email,
                    PasswordHash = pwd
                });

                return Created($"{_apiConfiguration.HostUrl}/user/{request.UserId}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post method invoked with userId: {0}, name: {1}, email: {2}", request.UserId, request.Name, request.Email);
                throw;
            }
        }
    }
}
