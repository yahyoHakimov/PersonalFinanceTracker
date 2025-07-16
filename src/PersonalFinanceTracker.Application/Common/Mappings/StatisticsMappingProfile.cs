using PersonalFinanceTracker.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Mappings
{
    public class StatisticsMappingProfile : AutoMapper.Profile
    {
        public StatisticsMappingProfile()
        {
            // Add mappings for statistics DTOs if needed
            CreateMap<CategoryExpense, CategoryExpenseItemDto>()
                .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => "#FF6B6B")) // Default color
                .ForMember(dest => dest.AverageAmount, opt => opt.MapFrom(src => src.TransactionCount > 0 ? src.TotalAmount / src.TransactionCount : 0))
                .ForMember(dest => dest.MaxAmount, opt => opt.Ignore()) // You might want to calculate this properly
                .ForMember(dest => dest.MinAmount, opt => opt.Ignore()); // You might want to calculate this properly

            CreateMap<MonthlyBalance, MonthlyTrendDto>()
                .ForMember(dest => dest.MonthName, opt => opt.MapFrom(src => src.Month.ToString("MMMM yyyy")))
                .ForMember(dest => dest.Income, opt => opt.MapFrom(src => src.TotalIncome))
                .ForMember(dest => dest.Expenses, opt => opt.MapFrom(src => src.TotalExpense))
                .ForMember(dest => dest.IncomeGrowth, opt => opt.Ignore())
                .ForMember(dest => dest.ExpenseGrowth, opt => opt.Ignore())
                .ForMember(dest => dest.BalanceGrowth, opt => opt.Ignore())
                .ForMember(dest => dest.TransactionCount, opt => opt.Ignore());
        }
    }
}
