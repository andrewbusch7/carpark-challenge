using System;
using System.Linq;

namespace Carpark.Domain.BillingStrategy
{
    public class WeekendRateBillingStrategy : IBillingStrategy
    {
        public string Name => "Weekend Rate";

        private Session _session;

        public WeekendRateBillingStrategy(Session session)
        {
            _session = session;
        }

        public bool IsApplicable()
        {
            var entry = _session.EntryDateTime.TimeOfDay;
            var vaildDays = new DayOfWeek[] {
                DayOfWeek.Saturday,
                DayOfWeek.Sunday
            };
            if (!vaildDays.Contains(_session.EntryDateTime.DayOfWeek)) return false;
            if (!vaildDays.Contains(_session.ExitDateTime.DayOfWeek)) return false;

            // Not explicit in rules but seems logical
            if (_session.Duration() > new TimeSpan(48, 0, 0)) return false;

            return true;
        }

        public decimal CalculateCost() => 10m;
    }
}
