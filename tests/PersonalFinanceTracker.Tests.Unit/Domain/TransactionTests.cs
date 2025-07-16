using FluentAssertions;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Tests.Unit.Domain
{
    public class TransactionTests
    {
        [Fact]
        public void IsValidAmount_PositiveAmount_ShouldReturnTrue()
        {
            // Arrange
            var transaction = new Transaction
            {
                Amount = 100m,
                Type = TransactionType.Expense
            };

            // Act
            var result = transaction.IsValidAmount();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValidAmount_ZeroAmount_ShouldReturnFalse()
        {
            // Arrange
            var transaction = new Transaction
            {
                Amount = 0m,
                Type = TransactionType.Expense
            };

            // Act
            var result = transaction.IsValidAmount();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsIncome_IncomeTransaction_ShouldReturnTrue()
        {
            // Arrange
            var transaction = new Transaction
            {
                Amount = 100m,
                Type = TransactionType.Income
            };

            // Act
            var result = transaction.IsIncome();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsExpense_ExpenseTransaction_ShouldReturnTrue()
        {
            // Arrange
            var transaction = new Transaction
            {
                Amount = 100m,
                Type = TransactionType.Expense
            };

            // Act
            var result = transaction.IsExpense();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetSignedAmount_IncomeTransaction_ShouldReturnPositiveAmount()
        {
            // Arrange
            var transaction = new Transaction
            {
                Amount = 100m,
                Type = TransactionType.Income
            };

            // Act
            var result = transaction.GetSignedAmount();

            // Assert
            result.Should().Be(100m);
        }

        [Fact]
        public void GetSignedAmount_ExpenseTransaction_ShouldReturnNegativeAmount()
        {
            // Arrange
            var transaction = new Transaction
            {
                Amount = 100m,
                Type = TransactionType.Expense
            };

            // Act
            var result = transaction.GetSignedAmount();

            // Assert
            result.Should().Be(-100m);
        }

        [Fact]
        public void BelongsToUser_SameUserId_ShouldReturnTrue()
        {
            // Arrange
            var userId = 1;
            var transaction = new Transaction
            {
                Amount = 100m,
                Type = TransactionType.Expense,
                UserId = userId
            };

            // Act
            var result = transaction.BelongsToUser(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void BelongsToUser_DifferentUserId_ShouldReturnFalse()
        {
            // Arrange
            var transaction = new Transaction
            {
                Amount = 100m,
                Type = TransactionType.Expense,
                UserId = 1
            };

            // Act
            var result = transaction.BelongsToUser(2);

            // Assert
            result.Should().BeFalse();
        }
    }
}
