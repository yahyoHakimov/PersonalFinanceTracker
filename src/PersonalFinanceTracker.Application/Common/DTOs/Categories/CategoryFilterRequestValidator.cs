using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.DTOs.Categories
{
    public class CategoryFilterRequestValidator : AbstractValidator<CategoryFilterRequest>
    {
        public CategoryFilterRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) ||
                               new[] { "name", "created_at", "transaction_count", "total_amount" }.Contains(sortBy.ToLower()))
                .WithMessage("Sort by must be one of: name, created_at, transaction_count, total_amount");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100).WithMessage("Search term must not exceed 100 characters");
        }
    }
}
