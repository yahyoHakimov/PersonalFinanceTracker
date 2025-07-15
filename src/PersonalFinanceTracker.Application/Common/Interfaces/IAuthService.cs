using PersonalFinanceTracker.Application.Common.DTOs.Auth;
using PersonalFinanceTracker.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
        Task<Result<bool>> LogoutAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
        Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
