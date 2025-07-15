using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.API.Extensions;
using PersonalFinanceTracker.Application.Common.DTOs.Categories;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.Entities;

namespace PersonalFinanceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Category details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _categoryService.GetByIdAsync(id, userId, cancellationToken);

            if (!result.IsSuccess)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<CategoryDto>.SuccessResponse(result.Data!, "Category retrieved successfully"));
        }

        /// <summary>
        /// Get paginated categories with filtering and sorting
        /// </summary>
        /// <param name="request">Filter and pagination parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of categories</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaged([FromQuery] CategoryFilterRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _categoryService.GetPagedAsync(userId, request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<PagedResult<CategoryDto>>.SuccessResponse(result.Data!, "Categories retrieved successfully"));
        }

        /// <summary>
        /// Get all categories for current user
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all user categories</returns>
        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _categoryService.GetByUserIdAsync(userId, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(result.Data!, "Categories retrieved successfully"));
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="request">Category creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created category</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _categoryService.CreateAsync(userId, request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error!, result.Errors));
            }

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<CategoryDto>.SuccessResponse(result.Data!, "Category created successfully"));
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="request">Category update details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated category</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            if (id != request.Id)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Category ID mismatch"));
            }

            var userId = User.GetUserId();
            var result = await _categoryService.UpdateAsync(userId, request, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Contains("not found")
                    ? NotFound(ApiResponse<object>.ErrorResponse(result.Error!))
                    : BadRequest(ApiResponse<object>.ErrorResponse(result.Error!, result.Errors));
            }

            return Ok(ApiResponse<CategoryDto>.SuccessResponse(result.Data!, "Category updated successfully"));
        }

        /// <summary>
        /// Delete a category (soft delete)
        /// </summary>
        /// <param name="id">Category ID</param>
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
            var result = await _categoryService.DeleteAsync(id, userId, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Contains("not found")
                    ? NotFound(ApiResponse<object>.ErrorResponse(result.Error!))
                    : BadRequest(ApiResponse<object>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(result.Data!, "Category deleted successfully"));
        }
    }
}
