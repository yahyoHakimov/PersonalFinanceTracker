

using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using PersonalFinanceTracker.Application.Common.DTOs.Auth;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Infrastructure.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PersonalFinanceTracker.Tests.Integration
{
    public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly ApplicationDbContext _context;

        public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            });

            _client = _factory.CreateClient();
            _context = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        [Fact]
        public async Task Register_ValidRequest_ShouldReturnCreatedResult()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test123!@#",
                ConfirmPassword = "Test123!@#"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().NotBeNullOrEmpty();
            result.Data.RefreshToken.Should().NotBeNullOrEmpty();
            result.Data.User.Username.Should().Be(request.Username);
            result.Data.User.Email.Should().Be(request.Email);
        }

        [Fact]
        public async Task Login_ValidCredentials_ShouldReturnOkResult()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "loginuser",
                Email = "login@example.com",
                Password = "Login123!@#",
                ConfirmPassword = "Login123!@#"
            };

            // First register a user
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            var loginRequest = new LoginRequest
            {
                UsernameOrEmail = "loginuser",
                Password = "Login123!@#"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().NotBeNullOrEmpty();
            result.Data.User.Username.Should().Be(registerRequest.Username);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                UsernameOrEmail = "nonexistent",
                Password = "WrongPassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCurrentUser_WithValidToken_ShouldReturnUserInfo()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "currentuser",
                Email = "current@example.com",
                Password = "Current123!@#",
                ConfirmPassword = "Current123!@#"
            };

            // Register and login to get token
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            var registerResult = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(registerContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var token = registerResult!.Data!.AccessToken;
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<UserDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Username.Should().Be(registerRequest.Username);
            result.Data.Email.Should().Be(registerRequest.Email);
        }

        [Fact]
        public async Task GetCurrentUser_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _client.Dispose();
        }
    }
}
