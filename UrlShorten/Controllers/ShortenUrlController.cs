using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using UrlShorten.WebAPI.Models;
using UrlShorten.WebAPI.Models.Configuration;
using UrlShorten.WebAPI.UrlTokenGenerator;
using UrlShorten.DataAccess.Interface;
using UrlShorten.DataAccess.Exceptions;
using UrlShorten.DTO;

namespace UrlShorten.WebAPI.Controllers
{
    [ApiController]
    [Route("url")]
    [Produces("application/json")]
    public class ShortenUrlController : ControllerBase
    {
        private readonly ILogger<ShortenUrlController> _logger;
        private readonly IDistributedCache _cache;
        private readonly IUrlDataAccess _dataAccess;
        private readonly IUrlTokenGenerator _urlTokenGenerator;
        private readonly ApiConfiguration _apiConfiguration;

        public ShortenUrlController(ILogger<ShortenUrlController> logger,
                                    IOptions<ApiConfiguration> apiConfiguration,
                                    IDistributedCache cache,
                                    IUrlDataAccess dataAccess,
                                    IUrlTokenGenerator urlTokenGenerator)
        {
            _logger = logger;
            _apiConfiguration = apiConfiguration.Value;
            _cache = cache;
            _dataAccess = dataAccess;
            _urlTokenGenerator = urlTokenGenerator;
        }

        /// <summary>
        /// Get the original url of a shorten url, and redirect the request to the original url
        /// </summary>
        /// <param name="urlToken">The token of the original url</param>
        /// <returns></returns>
        /// <response code="302">The url is found and redirect the request to the oringal url</response>
        /// <response code="400">The request is invalid</response>
        /// <response code="404">The token can't be found in the backend</response>
        [HttpGet]
        [Route("/{urlToken}")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Jump([MaxLength(7)][Required]string urlToken)
        {
            try
            {
                var urlBytes = await _cache.GetAsync(urlToken);
                var url = string.Empty;

                if (urlBytes != null)
                {
                    url = Encoding.UTF8.GetString(urlBytes);
                }

                if (url.IsNullOrEmpty())
                {
                    url = await _dataAccess.GetUrlAsync(urlToken);

                    if (url.IsNullOrEmpty())
                    {
                        return NotFound( new { error = "The given url token is not found." } );
                    }
                                        
                    await _cache.SetAsync(urlToken, Encoding.UTF8.GetBytes(url), 
                                                new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(_apiConfiguration.CacheDuration)));
                }

                await _dataAccess.IncreaseClickCountAsync(urlToken);

                return Redirect(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get method invoked with urlToken: {0}", urlToken);
                throw;
            }
        }

        /// <summary>
        /// Get the original url of a shorten url
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /url/werjo24
        /// </remarks>        
        /// <param name="urlToken"></param>
        /// <returns></returns>
        /// <response code="200">The url is found and returned</response>
        /// <response code="400">The request is invalid</response>
        /// <response code="404">The token can't be found in the backend</response>
        /// <response code="401">The request is unauthorized</response>
        [HttpGet]
        [Route("{urlToken}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([Required][MaxLength(7)]string urlToken)
        {
            string userId = string.Empty;

            try
            {
                userId = User.FindFirst("userid").Value;
                var urlDto = await _dataAccess.GetUrlAsync(urlToken, userId);

                if (urlDto == null)
                {
                    return NotFound(new { error = "The given url token is not found for the user." });
                }

                return Ok(new GetShortenUrlResponse { Url = urlDto.Url, Token = urlDto.Token, CreatedTime = urlDto.CreatedTime });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get method invoked with urlToken: {0}, userId: {1}", urlToken, userId);
                throw;
            }
        }

        /// <summary>
        /// Create a shorten url for the given original url
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /url
        ///     {
        ///        "url": "http://www.programminghunter.com/article/6548948008",
        ///     }
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">The request is invalid</response>          
        /// <response code="401">The request is unauthorized</response>
        /// <response code="409">The original url already exists for the current user</response>
        [HttpPost]
        [Route("")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Post(CreateShortenUrlRequest request)
        {
            string userId = string.Empty;

            try
            {
                userId = User.FindFirst("userid").Value;

                if (await _dataAccess.ExsitsUrlAsync(request.Url, userId))
                {
                    return Conflict(new { error = "The url entry already exists for the user." });
                }

                string urlToken = string.Empty;

                //TODO: To be refined, the injected denpendency object at controller level shouldn't be disposed in method level,
                using (_urlTokenGenerator)
                {
                    while (true)
                    {
                        try
                        {
                            urlToken = _urlTokenGenerator.GetUrlToken(request.Url);
                            await _dataAccess.AddUrlAsync(new AddUrlDTO { Url = request.Url, UrlToken = urlToken, UserId = userId });

                            break;
                        }
                        //Catch the exception of duplicate token insertion, and retry until a new unique token found
                        //This is to solve the possibility of duplicate tokens got inserted in massive concurrent situation 
                        catch (DuplicateKeyException)
                        {
                            _logger.LogInformation("Duplicate url token encountered: {0}, retry with new tokens.", urlToken);
                        }
                    }
                }

                return Created($"{_apiConfiguration.HostUrl}/url/{urlToken}", 
                                new CreateShortenUrlResponse 
                                { 
                                    Token = urlToken, 
                                    Url = $"{_apiConfiguration.HostUrl}/{urlToken}" 
                                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post method invoked with url: {0}, userId: {1}", request.Url, userId);
                throw;
            }
        }

        /// <summary>
        /// Delete a shorten url record for the user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /url/Yfwo34f
        /// </remarks>
        /// <param name="urlToken"></param>
        /// <returns></returns>
        /// <response code="204">The url record has been deleted from the backend</response>
        /// <response code="400">The request is invalid</response>
        /// <response code="401">The request is unauthorized</response>
        /// <response code="404">The url has not been found</response>
        [HttpDelete]
        [Route("{urlToken}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([Required][MaxLength(7)]string urlToken)
        {
            string userId = string.Empty;

            try
            {
                userId = User.FindFirst("userid").Value;

                if (!(await _dataAccess.ExsitsTokenAsync(urlToken, userId)))
                {
                    return NotFound(new { error = "The given url token is not found for the user." });
                }

                await _cache.RemoveAsync(urlToken);
                await _dataAccess.DeleteUrlAsync(urlToken);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DELETE method invoked with url token: {0}, userId: {1}", urlToken, userId);
                throw;
            }
        }

        /// <summary>
        /// Get the click count of a shorten url of the user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /url/clickcount/Yfwo34f
        /// </remarks>
        /// <param name="urlToken"></param>
        /// <returns></returns>
        /// <response code="200">The click count of the url is found and returned</response>
        /// <response code="400">The request is invalid</response>
        /// <response code="404">The url can't be found in the backend</response>
        /// <response code="401">The request is unauthorized</response>
        [HttpGet]
        [Route("clickcount/{urlToken}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClickCount([Required][MaxLength(7)]string urlToken)
        {
            string userId = string.Empty;

            try
            {
                userId = User.FindFirst("userid").Value;

                if (!(await _dataAccess.ExsitsTokenAsync(urlToken, userId)))
                {
                    return NotFound(new { error = "The given url token is not found for the user." });
                }

                var clickCount = await _dataAccess.GetClickCountAsync(urlToken);

                return Ok(new GetClickCountResponse
                {
                    ClickCount = clickCount.Value,
                    Token = urlToken
                }); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClickCount method invoked with url token: {0}, userId: {1}", urlToken, userId);
                throw;
            }
        }
    }
}
