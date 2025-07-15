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
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        /// <summary>
        /// Get transaction by ID
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _transactionService.GetByIdAsync(id, userId, cancellationToken);

            if (!result.IsSuccess)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<TransactionDto>.SuccessResponse(result.Data!, "Transaction retrieved successfully"));
        }

        /// <summary>
        /// Get paginated transactions with filtering and sorting
        /// </summary>
        /// <param name="request">Filter and pagination parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of transactions</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaged([FromQuery] TransactionFilterRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _transactionService.GetPagedAsync(userId, request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<PagedResult<TransactionDto>>.SuccessResponse(result.Data!, "Transactions retrieved successfully"));
        }

        /// <summary>
        /// Get all transactions for current user
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all user transactions</returns>
        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResponse<List<TransactionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _transactionService.GetByUserIdAsync(userId, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<List<TransactionDto>>.SuccessResponse(result.Data!, "Transactions retrieved successfully"));
        }

        /// <summary>
        /// Create a new transaction
        /// </summary>
        /// <param name="request">Transaction creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created transaction</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _transactionService.CreateAsync(userId, request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!, result.Errors));
            }

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<TransactionDto>.SuccessResponse(result.Data!, "Transaction created successfully"));
        }

        /// <summary>
        /// Update an existing transaction
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <param name="request">Transaction update details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated transaction</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTransactionRequest request, CancellationToken cancellationToken)
        {
            if (id != request.Id)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Transaction ID mismatch"));
            }

            var userId = User.GetUserId();
            var result = await _transactionService.UpdateAsync(userId, request, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Error!.Contains("not found"))
                    return NotFound(ApiResponse<object>.ErrorResponse(result.Error!));

                if (result.Error!.Contains("modified by another user"))
                    return Conflict(ApiResponse<object>.ErrorResponse(result.Error!));

                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!, result.Errors));
            }

            return Ok(ApiResponse<TransactionDto>.SuccessResponse(result.Data!, "Transaction updated successfully"));
        }

        /// <summary>
        /// Delete a transaction (soft delete)
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _transactionService.DeleteAsync(id, userId, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Contains("not found")
                    ? NotFound(ApiResponse<object>.ErrorResponse(result.Error!))
                    : BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(result.Data!, "Transaction deleted successfully"));
        }

        /// <summary>
        /// Get monthly balance for specific month
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Monthly balance details</returns>
        [HttpGet("monthly-balance/{year:int}/{month:int}")]
        [ProducesResponseType(typeof(ApiResponse<MonthlyBalanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMonthlyBalance(int year, int month, CancellationToken cancellationToken)
        {
            if (month < 1 || month > 12)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Month must be between 1 and 12"));
            }

            var userId = User.GetUserId();
            var date = new DateTime(year, month, 1);
            var result = await _transactionService.GetMonthlyBalanceAsync(userId, date, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<MonthlyBalanceDto>.SuccessResponse(result.Data!, "Monthly balance retrieved successfully"));
        }

        /// <summary>
        /// Get monthly balances for a date range
        /// </summary>
        /// <param name="startYear">Start year</param>
        /// <param name="startMonth">Start month (1-12)</param>
        /// <param name="endYear">End year</param>
        /// <param name="endMonth">End month (1-12)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of monthly balances</returns>
        [HttpGet("monthly-balances/{startYear:int}/{startMonth:int}/{endYear:int}/{endMonth:int}")]
        [ProducesResponseType(typeof(ApiResponse<List<MonthlyBalanceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMonthlyBalances(int startYear, int startMonth, int endYear, int endMonth, CancellationToken cancellationToken)
        {
            if (startMonth < 1 || startMonth > 12 || endMonth < 1 || endMonth > 12)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Months must be between 1 and 12"));
            }

            var startDate = new DateTime(startYear, startMonth, 1);
            var endDate = new DateTime(endYear, endMonth, 1);

            if (startDate > endDate)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Start date must be before or equal to end date"));
            }

            var userId = User.GetUserId();
            var result = await _transactionService.GetMonthlyBalancesAsync(userId, startDate, endDate, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<List<MonthlyBalanceDto>>.SuccessResponse(result.Data!, "Monthly balances retrieved successfully"));
        }
    }

}
