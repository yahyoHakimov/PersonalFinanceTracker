using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.API.Extensions;
using PersonalFinanceTracker.Application.Common.DTOs.Transactions;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.Entities;

namespace PersonalFinanceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class SummaryController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<SummaryController> _logger;

        public SummaryController(
            ITransactionService transactionService,
            ICategoryService categoryService,
            ILogger<SummaryController> logger)
        {
            _transactionService = transactionService;
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get current month summary for user
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Current month financial summary</returns>
        [HttpGet("current-month")]
        [ProducesResponseType(typeof(ApiResponse<MonthlyBalanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentMonthSummary(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var currentMonth = DateTime.UtcNow;
            var result = await _transactionService.GetMonthlyBalanceAsync(userId, currentMonth, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<MonthlyBalanceDto>.SuccessResponse(result.Data!, "Current month summary retrieved successfully"));
        }

        /// <summary>
        /// Get last 6 months summary trend
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Last 6 months financial trend</returns>
        [HttpGet("trend")]
        [ProducesResponseType(typeof(ApiResponse<List<MonthlyBalanceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTrend(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-5); // Last 6 months including current

            var result = await _transactionService.GetMonthlyBalancesAsync(userId, startDate, endDate, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<List<MonthlyBalanceDto>>.SuccessResponse(result.Data!, "Trend data retrieved successfully"));
        }
    }
}
