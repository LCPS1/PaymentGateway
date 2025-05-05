using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Cache.Options
{
    public class CacheSettings
    {
         public string ConnectionString { get; set; } = string.Empty;
        public string InstanceName { get; set; } = "PaymentGateway:";
        public int DefaultExpiryMinutes { get; set; } = 10;
    }
}