using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Api.Common.Extensions
{
    public static class DeclineReasonExtension
    {
        private static readonly Random _random = new Random();
        public static string GetRandomDeclineReason()
        {
            string[] reasons = new[]
            {
                "Insufficient funds",
                "Card expired",
                "Suspected fraud",
                "Card reported lost or stolen",
                "Card issuer declined transaction"
            };
            
            return reasons[_random.Next(reasons.Length)];
        }
    }
}