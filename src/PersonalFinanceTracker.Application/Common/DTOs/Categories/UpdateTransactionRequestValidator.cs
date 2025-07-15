using FluentValidation;
using PersonalFinanceTracker.Application.Common.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.DTOs.Categories
{
    public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
    {
        public UpdateTransactionRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Transaction ID must be greater than 0");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0")
                .LessThanOrEqualTo(999999999.99m).WithMessage("Amount must not exceed 999,999,999.99");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Transaction type must be Income or Expense");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than 0");

            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("Note must not exceed 500 characters");

            RuleFor(x => x.RowVersion)
                .NotEmpty().WithMessage("Row version is required for optimistic concurrency");
        }
    }

}
