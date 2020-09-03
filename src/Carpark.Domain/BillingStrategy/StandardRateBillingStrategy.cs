using System;

namespace Carpark.Domain.BillingStrategy
{
    public class StandardRateBillingStrategy : IBillingStrategy
    {
        public string Name => "Standard Rate";

        private Session _session;

        public StandardRateBillingStrategy(Session session)
        {
            _session = session;
        }

        public bool IsApplicable() => true;

        public decimal CalculateCost()
        {
            var duration = _session.Duration();

            if (duration >= new TimeSpan(3, 0, 0))
            {
                var days = duration.Days + 1;
                if (_session.EntryDateTime.TimeOfDay > _session.ExitDateTime.TimeOfDay) days += 1;
                return days * 20m;
            }
            if (duration >= new TimeSpan(2, 0, 0)) return 15m;
            if (duration >= new TimeSpan(1, 0, 0)) return 10m;
            return 5m;
        }
    }
}
