using System;
using System.Linq;

namespace Carpark.Domain.BillingStrategy
{
    public class NightRateBillingStrategy : IBillingStrategy
    {
        public string Name => "Night Rate";

        private Session _session;

        public NightRateBillingStrategy(Session session)
        {
            _session = session;
        }

        public bool IsApplicable()
        {
            var entry = _session.EntryDateTime.TimeOfDay;
            if (entry < new TimeSpan(18, 0, 0)) return false;

            var vaildEntryDays = new DayOfWeek[] {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            };
            if (!vaildEntryDays.Contains(_session.EntryDateTime.DayOfWeek)) return false;

            var exit = _session.ExitDateTime.TimeOfDay;
            if (exit < new TimeSpan(15, 30, 0)) return false;
            if (exit > new TimeSpan(23, 30, 0)) return false;

            // Not explicit in rules but seems logical
            if (_session.Duration() > new TimeSpan(24, 0, 0)) return false;

            return true;
        }

        public decimal CalculateCost() => 6.5m;
    }
}
