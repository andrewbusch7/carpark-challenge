using System;

namespace Carpark.Domain.BillingStrategy
{
    public class EarlyBirdBillingStrategy : IBillingStrategy
    {
        public string Name => "Early Bird";

        private Session _session;

        public EarlyBirdBillingStrategy(Session session)
        {
            _session = session;
        }

        public bool IsApplicable()
        {
            var entry = _session.EntryDateTime.TimeOfDay;
            if (entry < new TimeSpan(6, 0, 0)) return false;
            if (entry > new TimeSpan(9, 0, 0)) return false;

            var exit = _session.ExitDateTime.TimeOfDay;
            if (exit < new TimeSpan(15, 30, 0)) return false;
            if (exit > new TimeSpan(23, 30, 0)) return false;

            // Not explicit in rules but seems logical
            if (_session.Duration() > new TimeSpan(24, 0, 0)) return false;

            return true;
        }

        public decimal CalculateCost() => 13m;
    }
}
