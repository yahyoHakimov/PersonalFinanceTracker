using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Models
{
    public class FileSettings
    {
        public long MaxFileSize { get; set; } = 10485760; // 10MB
        public string[] AllowedExtensions { get; set; } = { ".xlsx", ".xls", ".csv" };
        public string UploadPath { get; set; } = "uploads";
        public string ReportPath { get; set; } = "reports";
    }
}
