using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

using Xunit;
using Moq;

using UrlShorten.WebAPI.Controllers;
using UrlShorten.WebAPI.Models;
using UrlShorten.DataAccess.Interface;
using UrlShorten.DTO;
using UrlShorten.WebAPI.Models.Configuration;

namespace UrlShorten.WebAPI.Tests
{
    public class UserControllerTest
    {
        [Fact]
        public async Task GetAccessToken_Should_Return_NotFound_When_User_Not_Exists()
        {
            var mockLogger = new Mock<ILogger<UserController>>();

            var mockDataAccess = new Mock<IUserDataAccess>();
            mockDataAccess.Setup(da => da.GetUserAsync("tom123")).ReturnsAsync(() => null);

            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockjwtConfiguration = new Mock<IOptions<JwtConfiguration>>();


            var controller = new UserController(mockLogger.Object,
                                                mockApiConfiguration.Object,
                                                mockjwtConfiguration.Object,
                                                mockDataAccess.Object);
            var result = await controller.GetAccessToken("tom123", "abc123");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetAccessToken_Should_Return_NotFound_When_Password_Incorrect()
        {
            var mockLogger = new Mock<ILogger<UserController>>();

            var mockDataAccess = new Mock<IUserDataAccess>();
            mockDataAccess.Setup(da => da.GetUserAsync("tom123")).ReturnsAsync(new GetUserDTO { PasswordHash = "2f3fjosdfNXyYIU2eJIuAw==" });

            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockjwtConfiguration = new Mock<IOptions<JwtConfiguration>>();


            var controller = new UserController(mockLogger.Object,
                                                mockApiConfiguration.Object,
                                                mockjwtConfiguration.Object,
                                                mockDataAccess.Object);
            var result = await controller.GetAccessToken("tom123", "abc123");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetAccessToken_Should_Return_Valid_Token_When_Password_Correct()
        {
            var mockLogger = new Mock<ILogger<UserController>>();

            var mockDataAccess = new Mock<IUserDataAccess>();
            mockDataAccess.Setup(da => da.GetUserAsync("tom123")).ReturnsAsync(new GetUserDTO { PasswordHash = "6ZoYxCjLONXyYIU2eJIuAw==" });

            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockjwtConfiguration = new Mock<IOptions<JwtConfiguration>>();
            mockjwtConfiguration.Setup(jc => jc.Value).Returns(new JwtConfiguration { ServerSecret = "wefjowerFSAD@#$123", Audience = "ShortenUrlClient", Issuer = "ShortenUrlServer", ExpirationDuration = 1 });


            var controller = new UserController(mockLogger.Object,
                                                mockApiConfiguration.Object,
                                                mockjwtConfiguration.Object,
                                                mockDataAccess.Object);
            var result = await controller.GetAccessToken("tom123", "abc123");


            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Post_Should_Return_Conflict_When_User_Exists()
        {
            var mockLogger = new Mock<ILogger<UserController>>();

            var mockDataAccess = new Mock<IUserDataAccess>();
            mockDataAccess.Setup(da => da.ExistsUserAsync("tom123")).ReturnsAsync(true);

            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            var mockjwtConfiguration = new Mock<IOptions<JwtConfiguration>>();


            var controller = new UserController(mockLogger.Object,
                                                mockApiConfiguration.Object,
                                                mockjwtConfiguration.Object,
                                                mockDataAccess.Object);
            var result = await controller.Post(new CreateUserRequest
            {
                UserId = "tom123",
                Password = "abc123",
                Email = "tom@tom.com",
                Name = "Tom"
            });

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Post_Should_Create_New_User_When_UserId_Not_Exists()
        {
            var mockLogger = new Mock<ILogger<UserController>>();

            var mockDataAccess = new Mock<IUserDataAccess>();
            mockDataAccess.Setup(da => da.ExistsUserAsync("tom123")).ReturnsAsync(false);
            mockDataAccess.Setup(da => da.AddUserAsync(It.IsAny<AddUserDTO>())).Returns(Task.CompletedTask).Verifiable();

            var mockApiConfiguration = new Mock<IOptions<ApiConfiguration>>();
            mockApiConfiguration.Setup(ac => ac.Value).Returns(new ApiConfiguration { HostUrl = "http://localhost:5918" });
            var mockjwtConfiguration = new Mock<IOptions<JwtConfiguration>>();


            var controller = new UserController(mockLogger.Object,
                                                mockApiConfiguration.Object,
                                                mockjwtConfiguration.Object,
                                                mockDataAccess.Object);
            var result = await controller.Post(new CreateUserRequest 
            {
                UserId = "tom123",
                Password = "abc123",
                Email = "tom@tom.com",
                Name = "Tom"
            });

            Assert.IsType<CreatedResult>(result);
            mockDataAccess.Verify();
        }
    }
}
