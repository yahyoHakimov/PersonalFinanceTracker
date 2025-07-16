using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using PersonalFinanceTracker.Application.Common.DTOs.Transactions;
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
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TransactionService>> _loggerMock;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _auditServiceMock = new Mock<IAuditService>();
            _cacheMock = new Mock<IDistributedCache>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<TransactionService>>();

            _transactionService = new TransactionService(
                _transactionRepositoryMock.Object,
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
            var categoryId = 1;
            var request = new CreateTransactionRequest
            {
                Amount = 100m,
                Type = TransactionType.Expense,
                CategoryId = categoryId,
                Note = "Test transaction"
            };

            var category = new Category
            {
                Id = categoryId,
                Name = "Test Category",
                UserId = userId
            };

            var transaction = new Transaction
            {
                Id = 1,
                Amount = request.Amount,
                Type = request.Type,
                CategoryId = categoryId,
                UserId = userId,
                Note = request.Note,
                Category = category
            };

            var transactionDto = new TransactionDto
            {
                Id = 1,
                Amount = request.Amount,
                Type = request.Type,
                CategoryId = categoryId,
                CategoryName = category.Name,
                UserId = userId,
                Note = request.Note
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _transactionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);
            _mapperMock.Setup(x => x.Map<TransactionDto>(It.IsAny<Transaction>()))
                .Returns(transactionDto);

            // Act
            var result = await _transactionService.CreateAsync(userId, request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Amount.Should().Be(request.Amount);
            result.Data.Type.Should().Be(request.Type);
            result.Data.CategoryName.Should().Be(category.Name);
        }

        [Fact]
        public async Task CreateAsync_InvalidCategory_ShouldReturnFailureResult()
        {
            // Arrange
            var userId = 1;
            var request = new CreateTransactionRequest
            {
                Amount = 100m,
                Type = TransactionType.Expense,
                CategoryId = 999, // Non-existent category
                Note = "Test transaction"
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(999, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _transactionService.CreateAsync(userId, request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Invalid category");
        }
    }

}
