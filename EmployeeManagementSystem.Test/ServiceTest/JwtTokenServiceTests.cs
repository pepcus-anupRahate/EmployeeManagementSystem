using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EmployeeManagementSystem.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EmployeeManagementSystem.Test.ServiceTest
{
    internal class JwtTokenServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private JwtTokenService _jwtTokenService;

        [SetUp]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("YourSecretKeyToTestTheAppHere1234567890");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("your_issuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("your_audience");

            _jwtTokenService = new JwtTokenService(_mockConfiguration.Object);
        }

        [Test]
        public void GenerateToken_ShouldReturnValidToken()
        {
            // Arrange
            var username = "testuser";
            var role = "testrole";

            // Act
            var token = _jwtTokenService.GenerateToken(username, role);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.ReadJwtToken(token);

            Assert.That(jwtSecurityToken, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(jwtSecurityToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value, Is.EqualTo(username));
                Assert.That(jwtSecurityToken.Claims.First(c => c.Type == ClaimTypes.Role).Value, Is.EqualTo(role));
            });
            Assert.Multiple(() =>
            {
                Assert.That(jwtSecurityToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value, Is.Not.Null);
                Assert.That(jwtSecurityToken.Issuer, Is.EqualTo(_mockConfiguration.Object["Jwt:Issuer"]));
                Assert.That(jwtSecurityToken.Audiences.First(), Is.EqualTo(_mockConfiguration.Object["Jwt:Audience"]));
            });
        }

        //[Test]
        //public void SetUpServices()
        //{
        //    var builder = WebApplication.CreateBuilder(new string[0]);
        //    var startupHelper = new Configuration(builder.Services);

        //    startupHelper.SetUpServices();

        //    var app = builder.Build();

        //    var myImplementation = app.Services.GetService<IEmployeeService>();
        //    Assert.That(myImplementation, Is.Not.Null);
        //    Assert.That(myImplementation is EmployeeService, Is.True);
        //}
    }
}
