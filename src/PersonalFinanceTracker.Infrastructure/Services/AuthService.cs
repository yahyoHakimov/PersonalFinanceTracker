using AutoMapper;
using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.Application.Common.DTOs.Auth;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IAuditService auditService,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(createdUser);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Update user with refresh token
                createdUser.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
                await _userRepository.UpdateAsync(createdUser, cancellationToken);

                // Log audit
                await _auditService.LogAsync(createdUser.Id, "REGISTER", "User", createdUser.Id, null,
                    _mapper.Map<UserDto>(createdUser), cancellationToken);

                var response = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = _mapper.Map<UserDto>(createdUser)
                };

                _logger.LogInformation("User {Username} registered successfully", request.Username);
                return Result<AuthResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Username}", request.Username);
                return Result<AuthResponse>.Failure("Registration failed");
            }
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail, cancellationToken);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for {UsernameOrEmail}", request.UsernameOrEmail);
                    return Result<AuthResponse>.Failure("Invalid username/email or password");
                }

                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Update user with refresh token
                var expiryDays = request.RememberMe ? 30 : 7;
                user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(expiryDays));
                await _userRepository.UpdateAsync(user, cancellationToken);

                // Log audit
                await _auditService.LogAsync(user.Id, "LOGIN", "User", user.Id, null, null, cancellationToken);

                var response = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = _mapper.Map<UserDto>(user)
                };

                _logger.LogInformation("User {Username} logged in successfully", user.Username);
                return Result<AuthResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {UsernameOrEmail}", request.UsernameOrEmail);
                return Result<AuthResponse>.Failure("Login failed");
            }
        }

        public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal == null)
                {
                    return Result<AuthResponse>.Failure("Invalid access token");
                }

                var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
                if (user == null)
                {
                    return Result<AuthResponse>.Failure("Invalid refresh token");
                }

                // Generate new tokens
                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // Update user with new refresh token
                user.UpdateRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
                await _userRepository.UpdateAsync(user, cancellationToken);

                var response = new AuthResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = _mapper.Map<UserDto>(user)
                };

                return Result<AuthResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return Result<AuthResponse>.Failure("Token refresh failed");
            }
        }

        public async Task<Result<bool>> LogoutAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                // Revoke refresh token
                user.RevokeRefreshToken();
                await _userRepository.UpdateAsync(user, cancellationToken);

                // Log audit
                await _auditService.LogAsync(userId, "LOGOUT", "User", userId, null, null, cancellationToken);

                _logger.LogInformation("User {UserId} logged out successfully", userId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", userId);
                return Result<bool>.Failure("Logout failed");
            }
        }

        public async Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return Result<bool>.Failure("Current password is incorrect");
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                // Revoke all refresh tokens for security
                user.RevokeRefreshToken();

                await _userRepository.UpdateAsync(user, cancellationToken);

                // Log audit
                await _auditService.LogAsync(userId, "CHANGE_PASSWORD", "User", userId, null, null, cancellationToken);

                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return Result<bool>.Failure("Password change failed");
            }
        }

        public async Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);
                if (user == null)
                {
                    return Result<bool>.Failure("Invalid refresh token");
                }

                user.RevokeRefreshToken();
                await _userRepository.UpdateAsync(user, cancellationToken);

                _logger.LogInformation("Refresh token revoked for user {UserId}", user.Id);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                return Result<bool>.Failure("Token revocation failed");
            }
        }
    }
}
