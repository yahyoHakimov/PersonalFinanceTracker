using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.API.Extensions;
using PersonalFinanceTracker.Application.Common.DTOs.Auth;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.Entities;
using System.Security.Claims;

namespace PersonalFinanceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!, result.Errors));
            }

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<AuthResponse>.SuccessResponse(result.Data!, "User registered successfully"));
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(result.Error!, result.Errors));
            }

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result.Data!, "Login successful"));
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshTokenAsync(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(result.Error!, result.Errors));
            }

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result.Data!, "Token refreshed successfully"));
        }

        /// <summary>
        /// Logout user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _authService.LogoutAsync(userId, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(result.Data!, "Logout successful"));
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _authService.ChangePasswordAsync(userId, request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(result.Data!, "Password changed successfully"));
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var user = new UserDto
            {
                Id = User.GetUserId(),
                Username = User.GetUsername(),
                Email = User.GetEmail(),
                Role = User.GetRole()
            };

            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User information retrieved successfully"));
        }
    }
}
