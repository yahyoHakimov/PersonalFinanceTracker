using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Domain.Constants
{
    public static class AuditActions
    {
        public const string CREATE = "CREATE";
        public const string UPDATE = "UPDATE";
        public const string DELETE = "DELETE";
        public const string LOGIN = "LOGIN";
        public const string LOGOUT = "LOGOUT";
    }
}
