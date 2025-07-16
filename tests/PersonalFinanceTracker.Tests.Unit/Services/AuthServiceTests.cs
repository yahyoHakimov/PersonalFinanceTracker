using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PersonalFinanceTracker.Application.Common.DTOs.Auth;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Domain.Enums;
using PersonalFinanceTracker.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtServiceMock = new Mock<IJwtService>();
            _auditServiceMock = new Mock<IAuditService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _jwtServiceMock.Object,
                _auditServiceMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_ValidRequest_ShouldReturnSuccessResult()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!"
            };

            var user = new User
            {
                Id = 1,
                Username = request.Username,
                Email = request.Email,
                Role = UserRole.User
            };

            var userDto = new UserDto
            {
                Id = 1,
                Username = request.Username,
                Email = request.Email,
                Role = UserRole.User
            };

            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("test-access-token");
            _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
                .Returns("test-refresh-token");
            _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(userDto);

            // Act
            var result = await _authService.RegisterAsync(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().Be("test-access-token");
            result.Data.RefreshToken.Should().Be("test-refresh-token");
            result.Data.User.Username.Should().Be(request.Username);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ShouldReturnSuccessResult()
        {
            // Arrange
            var request = new LoginRequest
            {
                UsernameOrEmail = "testuser",
                Password = "Test123!"
            };

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                Role = UserRole.User
            };

            var userDto = new UserDto
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Role = UserRole.User
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync(request.UsernameOrEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("test-access-token");
            _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
                .Returns("test-refresh-token");
            _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(userDto);

            // Act
            var result = await _authService.LoginAsync(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().Be("test-access-token");
            result.Data.User.Username.Should().Be(user.Username);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ShouldReturnFailureResult()
        {
            // Arrange
            var request = new LoginRequest
            {
                UsernameOrEmail = "testuser",
                Password = "WrongPassword"
            };

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync(request.UsernameOrEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Invalid username/email or password");
        }
    }
}
