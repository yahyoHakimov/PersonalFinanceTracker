using FluentValidation;
using PersonalFinanceTracker.Application.Common.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Transactions
{
    public class TransactionFilterRequestValidator : AbstractValidator<TransactionFilterRequest>
    {
        public TransactionFilterRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) ||
                               new[] { "amount", "type", "category", "created_at" }.Contains(sortBy.ToLower()))
                .WithMessage("Sort by must be one of: amount, type, category, created_at");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100).WithMessage("Search term must not exceed 100 characters");

            RuleFor(x => x.Type)
                .IsInEnum().When(x => x.Type.HasValue)
                .WithMessage("Transaction type must be Income or Expense");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).When(x => x.CategoryId.HasValue)
                .WithMessage("Category ID must be greater than 0");

            RuleFor(x => x.MinAmount)
                .GreaterThanOrEqualTo(0).When(x => x.MinAmount.HasValue)
                .WithMessage("Minimum amount must be greater than or equal to 0");

            RuleFor(x => x.MaxAmount)
                .GreaterThan(x => x.MinAmount).When(x => x.MaxAmount.HasValue && x.MinAmount.HasValue)
                .WithMessage("Maximum amount must be greater than minimum amount");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");
        }
    }
}
