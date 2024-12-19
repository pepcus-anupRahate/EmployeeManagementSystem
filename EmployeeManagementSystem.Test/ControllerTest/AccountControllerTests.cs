using System.Security.Claims;
using EmployeeManagementSystem.Controllers;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmployeeManagementSystem.Test.ControllerTest
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IAuthenticationService> _mockAuthService;
        private Mock<IJwtTokenService> _mockJwtTokenService;
        private AccountController _controller;

        [SetUp]
        public void Setup()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockJwtTokenService = new Mock<IJwtTokenService>();

            _controller = new AccountController(_mockJwtTokenService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public void Login_ShouldDeleteAuthTokenCookie()
        {
            // Arrange
            var mockCookies = new Mock<IResponseCookies>();
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);
            _mockHttpContext.SetupGet(x => x.Response).Returns(mockResponse.Object);

            // Act
            var result = _controller.Login() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null); // Verify it returns a ViewResult
            mockCookies.Verify(cookies => cookies.Delete("AuthToken"), Times.Once);
        }



        [Test]
        public void Login_ValidCredentials_ReturnsRedirectToEmployeeIndex()
        {
            // Arrange
            string username = "admin";
            string password = "password";
            string expectedToken = "mockToken";

            _mockJwtTokenService
                .Setup(service => service.GenerateToken(username, "Admin"))
                .Returns(expectedToken);

            var mockResponseCookies = new Mock<IResponseCookies>();
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(r => r.Cookies).Returns(mockResponseCookies.Object);
            _mockHttpContext.Setup(h => h.Response).Returns(mockResponse.Object);

            // Act
            var result = _controller.Login(username, password) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.ActionName, Is.EqualTo("Index"));
                Assert.That(result.ControllerName, Is.EqualTo("Employee"));
            });
            mockResponseCookies.Verify(cookies => cookies.Append("AuthToken", expectedToken, It.IsAny<CookieOptions>()), Times.Once);
        }

        [Test]
        public void Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            string username = "user";
            string password = "wrongpassword";

            // Act
            var result = _controller.Login(username, password) as UnauthorizedResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public void SecurePage_AuthorizedUser_ReturnsWelcomeMessage()
        {
            // Act
            var result = _controller.SecurePage() as ContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo("Welcome to the secure page!"));
        }

        [Test]
        public void AdminPage_UserInAdminRole_ReturnsWelcomeMessage()
        {
            // Arrange
            _mockHttpContext.Setup(h => h.User.IsInRole("Admin")).Returns(true);

            // Act
            var result = _controller.AdminPage() as ContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo("Welcome to the admin page!"));
        }    
    }
}