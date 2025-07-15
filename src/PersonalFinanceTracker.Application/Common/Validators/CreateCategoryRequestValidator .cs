using FluentValidation;
using PersonalFinanceTracker.Application.Common.DTOs.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Validators
{
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .Length(1, 100).WithMessage("Category name must be between 1 and 100 characters")
                .Matches("^[a-zA-Z0-9\\s\\-_]+$").WithMessage("Category name can only contain letters, numbers, spaces, hyphens and underscores");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Category color is required")
                .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Category color must be a valid hex color (e.g., #FF0000)");
        }
    }
}
