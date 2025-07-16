using AutoMapper;
using PersonalFinanceTracker.Application.Common.DTOs.Categories;
using PersonalFinanceTracker.Application.Common.DTOs.Transactions;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Mappings
{
    public class BusinessMappingProfile : Profile
    {
        public BusinessMappingProfile()
        {
            // Category mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.TransactionCount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());

            CreateMap<CreateCategoryRequest, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Transactions, opt => opt.Ignore());

            // Transaction mappings
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category.Color))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<CreateTransactionRequest, Transaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // MonthlyBalance mappings
            CreateMap<MonthlyBalance, MonthlyBalanceDto>()
                .ForMember(dest => dest.MonthName, opt => opt.MapFrom(src => src.Month.ToString("MMMM")))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Month.Year))
                .ForMember(dest => dest.BalancePercentage, opt => opt.MapFrom(src => src.GetBalancePercentage()))
                .ForMember(dest => dest.IncomeTransactionCount, opt => opt.Ignore())
                .ForMember(dest => dest.ExpenseTransactionCount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalTransactionCount, opt => opt.Ignore());
        }
    }

}
