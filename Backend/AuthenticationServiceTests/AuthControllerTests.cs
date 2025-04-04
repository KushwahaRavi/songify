using NUnit.Framework;
using Moq;
using AuthenticationService.Controllers;
using AuthenticationService.Model;
using AuthenticationService.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace AuthenticationService.Tests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IAuthRepo> _mockAuthRepo;
        private Mock<IConfiguration> _mockConfiguration;
        private AuthController _authController;

        [TearDown]
        public void TearDown()
        {
            // Dispose of _authController after each test if it implements IDisposable
            if (_authController is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        [SetUp]
        public void Setup()
        {
            _mockAuthRepo = new Mock<IAuthRepo>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setting up necessary JWT configuration
            _mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("your_jwt_secret_key");
            _mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("your_jwt_issuer");
            _mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("your_jwt_audience");

            _authController = new AuthController(_mockConfiguration.Object, _mockAuthRepo.Object);
        }

        [Test]
        public async Task ForgotPassword_ValidEmail_ReturnsOk()
        {
            // Arrange
            var request = new PasswordResetRequest
            {
                Email = "validuser@example.com"
            };

            _mockAuthRepo.Setup(repo => repo.GenerateOtpForPasswordReset(request.Email))
                         .ReturnsAsync("123456");

            // Act
            var result = await _authController.ForgotPassword(request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task ForgotPassword_InvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var request = new PasswordResetRequest
            {
                Email = "invaliduser@example.com"
            };

            _mockAuthRepo.Setup(repo => repo.GenerateOtpForPasswordReset(request.Email))
                         .ReturnsAsync((string?)null);

            // Act
            var result = await _authController.ForgotPassword(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }


        [Test]
        public async Task ResetPassword_InvalidOtp_ReturnsBadRequest()
        {
            // Arrange
            var confirmation = new PasswordResetConfirmation
            {
                Email = "validuser@example.com",
                Otp = "invalidotp",
                NewPassword = "newpassword123",
                ConfirmPassword = "newpassword123"
            };

            _mockAuthRepo.Setup(repo => repo.ValidateOtp(confirmation.Email, confirmation.Otp))
                         .ReturnsAsync(false);

            // Act
            var result = await _authController.ResetPassword(confirmation);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
    }
}
