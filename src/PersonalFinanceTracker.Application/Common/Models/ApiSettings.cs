using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Models
{
    public class ApiSettings
    {
        public string Version { get; set; } = "1.0";
        public string Title { get; set; } = "Personal Finance Tracker API";
        public string Description { get; set; } = "RESTful API for personal finance management";
        public string ContactName { get; set; } = "Development Team";
        public string ContactEmail { get; set; } = "dev@personalfinancetracker.com";
    }
}
