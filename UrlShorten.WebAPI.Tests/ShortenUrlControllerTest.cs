using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

using Xunit;
using Moq;

using UrlShorten.WebAPI.Controllers;
using UrlShorten.WebAPI.Models;
using UrlShorten.DataAccess.Interface;
using UrlShorten.DTO;
using UrlShorten.WebAPI.Models.Configuration;
using UrlShorten.WebAPI.UrlTokenGenerator;

namespace UrlShorten.WebAPI.Tests
{
    public class ShortenUrlControllerTest
    {
        [Fact]
        public async Task Jump_Should_Return_NotFound_When_UrlToken_Not_Exists()
        {
            var mockLogger = new Mock<ILogger<ShortenUrlController>>();

            var mockDataAccess = new Mock<IUrlDataAccess>();
            mockDataAccess.Setup(da => da.GetUrlAsync("16PKFK5")).ReturnsAsync(() => null);

            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();

            var mockCache = new Mock<IDistributedCache>();
            mockCache.Setup(c => c.GetAsync("16PKFK5", It.IsAny<CancellationToken>())).ReturnsAsync(() => null);

            var mockUrlTokenGenerator = new Mock<IUrlTokenGenerator>();

            var contorller = new ShortenUrlController(mockLogger.Object, 
                                                      mockApiConfiguration.Object, 
                                                      mockCache.Object, 
                                                      mockDataAccess.Object, 
                                                      mockUrlTokenGenerator.Object);

            var result = await contorller.Jump("16PKFK5");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Jump_Should_Return_Redirect_When_UrlToken_Exists()
        {
            var mockLogger = new Mock<ILogger<ShortenUrlController>>();

            var mockDataAccess = new Mock<IUrlDataAccess>();
            mockDataAccess.Setup(da => da.GetUrlAsync("16PKFK5")).ReturnsAsync("http://www.geeksforgeeks.org/common-language-runtime-clr-in-c-sharp");

            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            mockApiConfiguration.Setup(ac => ac.Value).Returns(new ApiConfiguration { CacheDuration = 60 });

            var mockCache = new Mock<IDistributedCache>();
            mockCache.Setup(c => c.GetAsync("16PKFK5", It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(),
                                                  It.IsAny<byte[]>(), 
                                                  It.IsAny<DistributedCacheEntryOptions>(),
                                                  It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask).Verifiable();

            var mockUrlTokenGenerator = new Mock<IUrlTokenGenerator>();

            var contorller = new ShortenUrlController(mockLogger.Object,
                                                      mockApiConfiguration.Object,
                                                      mockCache.Object,
                                                      mockDataAccess.Object,
                                                      mockUrlTokenGenerator.Object);

            var result = await contorller.Jump("16PKFK5");

            Assert.IsType<RedirectResult>(result);
            mockCache.Verify();
        }

        [Fact]
        public async Task Get_Should_Return_NotFound_When_UrlToken_Not_Exists()
        {
            var mockLogger = new Mock<ILogger<ShortenUrlController>>();
            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockCache = new Mock<IDistributedCache>();
            var mockUrlTokenGenerator = new Mock<IUrlTokenGenerator>();

            var mockDataAccess = new Mock<IUrlDataAccess>();
            mockDataAccess.Setup(da => da.GetUrlAsync("16PKFK5")).ReturnsAsync(() => null);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(hc => hc.User.FindFirst("userid")).Returns(new Claim("userid", "tom123"));


            var contorller = new ShortenUrlController(mockLogger.Object,
                                                      mockApiConfiguration.Object,
                                                      mockCache.Object,
                                                      mockDataAccess.Object,
                                                      mockUrlTokenGenerator.Object);
            contorller.ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object };

            var result = await contorller.Get("16PKFK5");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Get_Should_Return_Url_When_UrlToken_Exists()
        {
            var mockLogger = new Mock<ILogger<ShortenUrlController>>();
            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockCache = new Mock<IDistributedCache>();
            var mockUrlTokenGenerator = new Mock<IUrlTokenGenerator>();

            var mockDataAccess = new Mock<IUrlDataAccess>();
            mockDataAccess.Setup(da => da.GetUrlAsync("16PKFK5", "tom123")).ReturnsAsync(new GetUrlDTO());

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(hc => hc.User.FindFirst("userid")).Returns(new Claim("userid", "tom123"));


            var contorller = new ShortenUrlController(mockLogger.Object,
                                                      mockApiConfiguration.Object,
                                                      mockCache.Object,
                                                      mockDataAccess.Object,
                                                      mockUrlTokenGenerator.Object);
            contorller.ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object };

            var result = await contorller.Get("16PKFK5");


            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Post_Should_Return_Conflict_When_Url_Exists_For_User()
        {
            var mockLogger = new Mock<ILogger<ShortenUrlController>>();
            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockCache = new Mock<IDistributedCache>();
            var mockUrlTokenGenerator = new Mock<IUrlTokenGenerator>();

            var mockDataAccess = new Mock<IUrlDataAccess>();
            mockDataAccess.Setup(da => da.ExsitsUrlAsync("http://www.geeksforgeeks.org/common-language-runtime-clr-in-c-sharp", "tom123")).ReturnsAsync(true);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(hc => hc.User.FindFirst("userid")).Returns(new Claim("userid", "tom123"));


            var contorller = new ShortenUrlController(mockLogger.Object,
                                                      mockApiConfiguration.Object,
                                                      mockCache.Object,
                                                      mockDataAccess.Object,
                                                      mockUrlTokenGenerator.Object);
            contorller.ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object };

            var result = await contorller.Post(new CreateShortenUrlRequest
            {
                Url = "http://www.geeksforgeeks.org/common-language-runtime-clr-in-c-sharp"
            });


            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Post_Should_Return_Created_When_Url_Not_Exists_For_User()
        {
            var mockLogger = new Mock<ILogger<ShortenUrlController>>();

            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            mockApiConfiguration.Setup(ac => ac.Value).Returns(new ApiConfiguration { HostUrl = "http://localhost:5918" });
            var mockCache = new Mock<IDistributedCache>();

            var mockUrlTokenGenerator = new Mock<IUrlTokenGenerator>();
            mockUrlTokenGenerator.Setup(utg => utg.GetUrlToken("http://www.geeksforgeeks.org/common-language-runtime-clr-in-c-sharp")).Returns(It.IsAny<string>()).Verifiable();

            var mockDataAccess = new Mock<IUrlDataAccess>();
            mockDataAccess.Setup(da => da.ExsitsUrlAsync("http://www.geeksforgeeks.org/common-language-runtime-clr-in-c-sharp", "tom123")).ReturnsAsync(false);
            mockDataAccess.Setup(da => da.AddUrlAsync(It.IsAny<AddUrlDTO>())).Returns(Task.CompletedTask).Verifiable();

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(hc => hc.User.FindFirst("userid")).Returns(new Claim("userid", "tom123"));


            var contorller = new ShortenUrlController(mockLogger.Object,
                                                      mockApiConfiguration.Object,
                                                      mockCache.Object,
                                                      mockDataAccess.Object,
                                                      mockUrlTokenGenerator.Object);
            contorller.ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object };

            var result = await contorller.Post(new CreateShortenUrlRequest
            {
                Url = "http://www.geeksforgeeks.org/common-language-runtime-clr-in-c-sharp"
            });


            Assert.IsType<CreatedResult>(result);
            mockUrlTokenGenerator.Verify();
            mockDataAccess.Verify();
        }

        [Fact]
        public async Task Delete_Should_Return_NoFound_When_UrlToken_Not_Exists()
        {
            var mockLogger = new Mock<ILogger<ShortenUrlController>>();
            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockCache = new Mock<IDistributedCache>();
            var mockUrlTokenGenerator = new Mock<IUrlTokenGenerator>();

            var mockDataAccess = new Mock<IUrlDataAccess>();
            mockDataAccess.Setup(da => da.ExsitsTokenAsync("16PKFK5", "tom123")).ReturnsAsync(false);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(hc => hc.User.FindFirst("userid")).Returns(new Claim("userid","tom123"));


            var contorller = new ShortenUrlController(mockLogger.Object,
                                                      mockApiConfiguration.Object,
                                                      mockCache.Object,
                                                      mockDataAccess.Object,
                                                      mockUrlTokenGenerator.Object);
            contorller.ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object };

            var result = await contorller.Delete("16PKFK5");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_Should_Return_NoContent_When_UrlToken_Exists()
        {
            var mockLogger = new Mock<ILogger<ShortenUrlController>>();
            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockUrlTokenGenerator = new Mock<IUrlTokenGenerator>();

            var mockCache = new Mock<IDistributedCache>();
            mockCache.Setup(c => c.RemoveAsync("16PKFK5", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var mockDataAccess = new Mock<IUrlDataAccess>();
            mockDataAccess.Setup(da => da.ExsitsTokenAsync("16PKFK5", "tom123")).ReturnsAsync(true);
            mockDataAccess.Setup(da => da.DeleteUrlAsync("16PKFK5")).Returns(Task.CompletedTask).Verifiable();

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(hc => hc.User.FindFirst("userid")).Returns(new Claim("userid", "tom123"));

            var contorller = new ShortenUrlController(mockLogger.Object,
                                                      mockApiConfiguration.Object,
                                                      mockCache.Object,
                                                      mockDataAccess.Object,
                                                      mockUrlTokenGenerator.Object);
            contorller.ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object };

            var result = await contorller.Delete("16PKFK5");

            Assert.IsType<NoContentResult>(result);
            mockCache.Verify();
            mockDataAccess.Verify();
        }

        [Fact]
        public async Task GetClickCount_Should_Return_NotFound_When_UrlToken_Not_Exists()
        {
            var mockLogger = new Mock<ILogger<ShortenUrlController>>();
            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockCache = new Mock<IDistributedCache>();
            var mockUrlTokenGenerator = new Mock<IUrlTokenGenerator>();

            var mockDataAccess = new Mock<IUrlDataAccess>();
            mockDataAccess.Setup(da => da.ExsitsTokenAsync("16PKFK5", "tom123")).ReturnsAsync(false);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(hc => hc.User.FindFirst("userid")).Returns(new Claim("userid", "tom123"));


            var contorller = new ShortenUrlController(mockLogger.Object,
                                                      mockApiConfiguration.Object,
                                                      mockCache.Object,
                                                      mockDataAccess.Object,
                                                      mockUrlTokenGenerator.Object);
            contorller.ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object };

            var result = await contorller.GetClickCount("16PKFK5");


            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetClickCount_Should_Return_ClickCount_When_UrlToken_Exists()
        {
            var mockLogger = new Mock<ILogger<ShortenUrlController>>();
            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockCache = new Mock<IDistributedCache>();
            var mockUrlTokenGenerator = new Mock<IUrlTokenGenerator>();

            var mockDataAccess = new Mock<IUrlDataAccess>();
            mockDataAccess.Setup(da => da.ExsitsTokenAsync("16PKFK5", "tom123")).ReturnsAsync(true);
            mockDataAccess.Setup(da => da.GetClickCountAsync("16PKFK5")).ReturnsAsync(0);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(hc => hc.User.FindFirst("userid")).Returns(new Claim("userid", "tom123"));


            var contorller = new ShortenUrlController(mockLogger.Object,
                                                      mockApiConfiguration.Object,
                                                      mockCache.Object,
                                                      mockDataAccess.Object,
                                                      mockUrlTokenGenerator.Object);
            contorller.ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object };

            var result = await contorller.GetClickCount("16PKFK5");


            Assert.IsType<OkObjectResult>(result);
        }
    }
}
