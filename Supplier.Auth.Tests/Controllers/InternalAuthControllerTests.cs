using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Auth.Controllers.Internal;
using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Dto.Responses;
using Supplier.Auth.Services.Interfaces;
using System.Security.Claims;

namespace Supplier.Auth.Tests.Controllers
{
    public class InternalAuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ILogger<InternalAuthController>> _loggerMock;
        private readonly InternalAuthController _controller;

        public InternalAuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<InternalAuthController>>();
            _controller = new InternalAuthController(_authServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task RegisterAdmin_ReturnsOkResult_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var request = new RegisterAdminRequestDto { Email = "admin@example.com", Password = "Password123" };
            var response = new RegisterResponseDto(Guid.NewGuid(), "Success");
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "testuser") }));

            _authServiceMock.Setup(s => s.RegisterAdminUser(request, It.IsAny<ClaimsPrincipal>())).ReturnsAsync(response);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.RegisterAdmin(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task RegisterAdmin_ReturnsBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var request = new RegisterAdminRequestDto { Email = "admin@example.com", Password = "Password123" };
            var response = new RegisterResponseDto(Guid.Empty, "Failure");
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "testuser") }));

            _authServiceMock.Setup(s => s.RegisterAdminUser(request, It.IsAny<ClaimsPrincipal>())).ReturnsAsync(response);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.RegisterAdmin(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(response.Message, badRequestResult.Value);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOkResult_WithListOfUsers()
        {
            // Arrange
            var users = new List<UserResponseDto>
                    {
                        new UserResponseDto { Id = Guid.NewGuid(), Email = "user1@example.com" },
                        new UserResponseDto { Id = Guid.NewGuid(), Email = "user2@example.com" }
                    };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "testuser") }));

            _authServiceMock.Setup(s => s.GetAllUsers(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(users);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(users, okResult.Value);
        }
    }
}
