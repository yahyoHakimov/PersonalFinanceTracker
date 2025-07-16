// PersonalFinanceTracker.Infrastructure/Services/ExcelReportGenerator.cs
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using PersonalFinanceTracker.Application.Common.DTOs.Statistics;
using PersonalFinanceTracker.Application.Common.Interfaces;

namespace PersonalFinanceTracker.Infrastructure.Services
{
    public class ExcelReportGenerator
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ExcelReportGenerator(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<byte[]> GenerateReportAsync(int userId, ExcelReportRequestDto request)
        {
            // Fix: Use full namespace for LicenseContext
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            // Add different worksheets based on report types
            if (request.ReportTypes.Contains(ExcelReportType.Transactions) ||
                request.ReportTypes.Contains(ExcelReportType.Complete))
            {
                await AddTransactionsSheet(package, userId, request.StartDate, request.EndDate);
            }

            if (request.ReportTypes.Contains(ExcelReportType.Categories) ||
                request.ReportTypes.Contains(ExcelReportType.Complete))
            {
                await AddCategoriesSheet(package, userId, request.StartDate, request.EndDate);
            }

            if (request.ReportTypes.Contains(ExcelReportType.MonthlyBalance) ||
                request.ReportTypes.Contains(ExcelReportType.Complete))
            {
                await AddMonthlyBalanceSheet(package, userId, request.StartDate, request.EndDate);
            }

            if (request.ReportTypes.Contains(ExcelReportType.CategoryExpenses) ||
                request.ReportTypes.Contains(ExcelReportType.Complete))
            {
                await AddCategoryExpensesSheet(package, userId, request.StartDate, request.EndDate);
            }

            // Add summary dashboard
            AddSummarySheet(package, request);

            return package.GetAsByteArray();
        }

        private async Task AddTransactionsSheet(ExcelPackage package, int userId, DateTime startDate, DateTime endDate)
        {
            var worksheet = package.Workbook.Worksheets.Add("Transactions");
            var transactions = await _transactionRepository.GetByUserIdAndDateRangeAsync(userId, startDate, endDate);

            // Headers
            worksheet.Cells[1, 1].Value = "Date";
            worksheet.Cells[1, 2].Value = "Type";
            worksheet.Cells[1, 3].Value = "Category";
            worksheet.Cells[1, 4].Value = "Amount";
            worksheet.Cells[1, 5].Value = "Note";

            // Data
            for (int i = 0; i < transactions.Count; i++)
            {
                var transaction = transactions[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = transaction.CreatedAt.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 2].Value = transaction.Type.ToString();
                worksheet.Cells[row, 3].Value = transaction.Category?.Name ?? "Unknown";
                worksheet.Cells[row, 4].Value = transaction.Amount;
                worksheet.Cells[row, 5].Value = transaction.Note ?? "";
            }

            // Formatting
            worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private async Task AddCategoriesSheet(ExcelPackage package, int userId, DateTime startDate, DateTime endDate)
        {
            var worksheet = package.Workbook.Worksheets.Add("Category Analysis");
            var categoryExpenses = await _transactionRepository.GetCategoryExpensesAsync(userId, startDate, endDate);

            // Headers
            worksheet.Cells[1, 1].Value = "Category";
            worksheet.Cells[1, 2].Value = "Total Amount";
            worksheet.Cells[1, 3].Value = "Transaction Count";
            worksheet.Cells[1, 4].Value = "Percentage";
            worksheet.Cells[1, 5].Value = "Average Amount";

            // Data
            for (int i = 0; i < categoryExpenses.Count; i++)
            {
                var category = categoryExpenses[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = category.CategoryName;
                worksheet.Cells[row, 2].Value = category.TotalAmount;
                worksheet.Cells[row, 3].Value = category.TransactionCount;
                worksheet.Cells[row, 4].Value = category.Percentage / 100;
                worksheet.Cells[row, 5].Value = category.TransactionCount > 0 ? category.TotalAmount / category.TransactionCount : 0;
            }

            // Formatting
            worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;
            worksheet.Cells[2, 4, categoryExpenses.Count + 1, 4].Style.Numberformat.Format = "0.00%";
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Add chart if requested
            if (categoryExpenses.Any())
            {
                var chart = worksheet.Drawings.AddChart("CategoryChart", eChartType.Pie);
                chart.Title.Text = "Expenses by Category";
                chart.SetPosition(1, 0, 6, 0);
                chart.SetSize(400, 300);

                var range = worksheet.Cells[2, 1, categoryExpenses.Count + 1, 2];
                chart.Series.Add(range.Offset(0, 1, 0, 1), range.Offset(0, 0, 0, 1));
            }
        }

        private async Task AddMonthlyBalanceSheet(ExcelPackage package, int userId, DateTime startDate, DateTime endDate)
        {
            var worksheet = package.Workbook.Worksheets.Add("Monthly Balance");
            var monthlyBalances = await _transactionRepository.GetMonthlyBalancesAsync(userId, startDate, endDate);

            // Headers
            worksheet.Cells[1, 1].Value = "Month";
            worksheet.Cells[1, 2].Value = "Total Income";
            worksheet.Cells[1, 3].Value = "Total Expenses";
            worksheet.Cells[1, 4].Value = "Balance";

            // Data
            for (int i = 0; i < monthlyBalances.Count; i++)
            {
                var balance = monthlyBalances[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = balance.Month.ToString("MMMM yyyy");
                worksheet.Cells[row, 2].Value = balance.TotalIncome;
                worksheet.Cells[row, 3].Value = balance.TotalExpense;
                worksheet.Cells[row, 4].Value = balance.Balance;
            }

            // Formatting
            worksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Add trend chart
            if (monthlyBalances.Any())
            {
                var chart = worksheet.Drawings.AddChart("BalanceChart", eChartType.Line);
                chart.Title.Text = "Monthly Balance Trend";
                chart.SetPosition(1, 0, 5, 0);
                chart.SetSize(600, 300);

                var range = worksheet.Cells[2, 1, monthlyBalances.Count + 1, 4];
                chart.Series.Add(range.Offset(0, 1, 0, 1), range.Offset(0, 0, 0, 1)).Header = "Income";
                chart.Series.Add(range.Offset(0, 2, 0, 1), range.Offset(0, 0, 0, 1)).Header = "Expenses";
                chart.Series.Add(range.Offset(0, 3, 0, 1), range.Offset(0, 0, 0, 1)).Header = "Balance";
            }
        }

        private async Task AddCategoryExpensesSheet(ExcelPackage package, int userId, DateTime startDate, DateTime endDate)
        {
            var worksheet = package.Workbook.Worksheets.Add("Top Categories");
            var categoryExpenses = await _transactionRepository.GetCategoryExpensesAsync(userId, startDate, endDate);

            // Headers
            worksheet.Cells[1, 1].Value = "Rank";
            worksheet.Cells[1, 2].Value = "Category";
            worksheet.Cells[1, 3].Value = "Amount";
            worksheet.Cells[1, 4].Value = "Percentage";
            worksheet.Cells[1, 5].Value = "Transactions";

            // Data (top 10 categories)
            var topCategories = categoryExpenses.Take(10).ToList();
            for (int i = 0; i < topCategories.Count; i++)
            {
                var category = topCategories[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = i + 1;
                worksheet.Cells[row, 2].Value = category.CategoryName;
                worksheet.Cells[row, 3].Value = category.TotalAmount;
                worksheet.Cells[row, 4].Value = category.Percentage / 100;
                worksheet.Cells[row, 5].Value = category.TransactionCount;
            }

            // Formatting
            worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;
            worksheet.Cells[2, 4, topCategories.Count + 1, 4].Style.Numberformat.Format = "0.00%";
            worksheet.Cells[2, 3, topCategories.Count + 1, 3].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void AddSummarySheet(ExcelPackage package, ExcelReportRequestDto request)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary");

            // Report info
            worksheet.Cells[1, 1].Value = "Financial Report Summary";
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;

            worksheet.Cells[3, 1].Value = "Report Period:";
            worksheet.Cells[3, 2].Value = $"{request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}";

            worksheet.Cells[4, 1].Value = "Generated:";
            worksheet.Cells[4, 2].Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            worksheet.Cells[5, 1].Value = "Report Types:";
            worksheet.Cells[5, 2].Value = string.Join(", ", request.ReportTypes);

            // Instructions
            worksheet.Cells[7, 1].Value = "Report Contents:";
            worksheet.Cells[7, 1].Style.Font.Bold = true;

            var instructions = new[]
            {
                "• Transactions: Detailed list of all transactions in the period",
                "• Category Analysis: Breakdown of expenses by category",
                "• Monthly Balance: Income, expenses, and balance by month",
                "• Top Categories: Ranking of categories by spending amount"
            };

            for (int i = 0; i < instructions.Length; i++)
            {
                worksheet.Cells[8 + i, 1].Value = instructions[i];
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
    }
}