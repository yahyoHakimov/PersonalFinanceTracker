using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using PersonalFinanceTracker.Application.Common.DTOs.Categories;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Tests.Unit.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CategoryService>> _loggerMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _auditServiceMock = new Mock<IAuditService>();
            _cacheMock = new Mock<IDistributedCache>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CategoryService>>();

            _categoryService = new CategoryService(
                _categoryRepositoryMock.Object,
                _auditServiceMock.Object,
                _cacheMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_ShouldReturnSuccessResult()
        {
            // Arrange
            var userId = 1;
            var request = new CreateCategoryRequest
            {
                Name = "Test Category",
                Color = "#FF0000"
            };

            var category = new Category
            {
                Id = 1,
                Name = request.Name,
                Color = request.Color,
                UserId = userId
            };

            var categoryDto = new CategoryDto
            {
                Id = 1,
                Name = request.Name,
                Color = request.Color,
                UserId = userId
            };

            _categoryRepositoryMock.Setup(x => x.IsNameExistsAsync(request.Name, userId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _mapperMock.Setup(x => x.Map<CategoryDto>(It.IsAny<Category>()))
                .Returns(categoryDto);

            // Act
            var result = await _categoryService.CreateAsync(userId, request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Name.Should().Be(request.Name);
            result.Data.Color.Should().Be(request.Color);
        }

        [Fact]
        public async Task CreateAsync_DuplicateName_ShouldReturnFailureResult()
        {
            // Arrange
            var userId = 1;
            var request = new CreateCategoryRequest
            {
                Name = "Existing Category",
                Color = "#FF0000"
            };

            _categoryRepositoryMock.Setup(x => x.IsNameExistsAsync(request.Name, userId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _categoryService.CreateAsync(userId, request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Category with this name already exists");
        }

        [Fact]
        public async Task GetByIdAsync_ExistingCategory_ShouldReturnSuccessResult()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;
            var category = new Category
            {
                Id = categoryId,
                Name = "Test Category",
                Color = "#FF0000",
                UserId = userId
            };

            var categoryDto = new CategoryDto
            {
                Id = categoryId,
                Name = "Test Category",
                Color = "#FF0000",
                UserId = userId
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.GetTransactionCountAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);
            _categoryRepositoryMock.Setup(x => x.GetTotalAmountAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1000m);
            _mapperMock.Setup(x => x.Map<CategoryDto>(It.IsAny<Category>()))
                .Returns(categoryDto);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId, userId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(categoryId);
            result.Data.TransactionCount.Should().Be(5);
            result.Data.TotalAmount.Should().Be(1000m);
        }
    }
}
