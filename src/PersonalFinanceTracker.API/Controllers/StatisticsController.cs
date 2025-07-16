using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.API.Extensions;
using PersonalFinanceTracker.Application.Common.DTOs.Statistics;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.ValueObjects;

namespace PersonalFinanceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(IStatisticsService statisticsService, ILogger<StatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _logger = logger;
        }

        /// <summary>
        /// Get category expense statistics for a date range
        /// </summary>
        /// <param name="startDate">Start date (optional, defaults to current month start)</param>
        /// <param name="endDate">End date (optional, defaults to current month end)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Category expense statistics</returns>
        [HttpGet("category-expenses")]
        [ProducesResponseType(typeof(ApiResponse<CategoryExpenseStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategoryExpenses(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();

            // Default to current month if dates not provided
            var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var end = endDate ?? start.AddMonths(1).AddDays(-1);

            var result = await _statisticsService.GetCategoryExpenseStatisticsAsync(userId, start, end, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<CategoryExpenseStatisticsDto>.SuccessResponse(
                result.Data!, "Category expense statistics retrieved successfully"));
        }

        /// <summary>
        /// Get trend analysis comparing current period with previous period
        /// </summary>
        /// <param name="months">Number of months to analyze (default: 6)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Trend analysis data</returns>
        [HttpGet("trends")]
        [ProducesResponseType(typeof(ApiResponse<TrendAnalysisDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTrends(
            [FromQuery] int months = 6,
            CancellationToken cancellationToken = default)
        {
            if (months < 1 || months > 24)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Months must be between 1 and 24"));
            }

            var userId = User.GetUserId();
            var result = await _statisticsService.GetTrendAnalysisAsync(userId, months, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<TrendAnalysisDto>.SuccessResponse(
                result.Data!, "Trend analysis retrieved successfully"));
        }

        /// <summary>
        /// Request Excel report generation (async via background worker)
        /// </summary>
        /// <param name="request">Report request parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Report generation job ID</returns>
        [HttpPost("reports/excel")]
        [ProducesResponseType(typeof(ApiResponse<ReportJobDto>), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateExcelReport(
            [FromBody] ExcelReportRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            var result = await _statisticsService.RequestExcelReportAsync(userId, request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return StatusCode(StatusCodes.Status202Accepted,
                ApiResponse<ReportJobDto>.SuccessResponse(
                    result.Data!, "Excel report generation started"));
        }

        /// <summary>
        /// Get report generation status
        /// </summary>
        /// <param name="jobId">Report job ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Report status</returns>
        [HttpGet("reports/{jobId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<ReportStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReportStatus(
            Guid jobId,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            var result = await _statisticsService.GetReportStatusAsync(userId, jobId, cancellationToken);

            if (!result.IsSuccess)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<ReportStatusDto>.SuccessResponse(
                result.Data!, "Report status retrieved successfully"));
        }

        /// <summary>
        /// Download completed Excel report
        /// </summary>
        /// <param name="jobId">Report job ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Excel file download</returns>
        [HttpGet("reports/{jobId:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DownloadReport(
            Guid jobId,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            var result = await _statisticsService.DownloadReportAsync(userId, jobId, cancellationToken);

            if (!result.IsSuccess)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            var fileData = result.Data!;
            return File(fileData.Content, fileData.ContentType, fileData.FileName);
        }

        /// <summary>
        /// Get comprehensive dashboard statistics
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dashboard statistics</returns>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<DashboardStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboardStatistics(CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            var result = await _statisticsService.GetDashboardStatisticsAsync(userId, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<DashboardStatisticsDto>.SuccessResponse(
                result.Data!, "Dashboard statistics retrieved successfully"));
        }
    }
}