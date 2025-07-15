using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Interfaces
{
    public interface ICacheableRequest
    {
        string CacheKey { get; }
        TimeSpan CacheExpiration { get; }
    }
}
