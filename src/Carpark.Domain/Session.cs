using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using Carpark.Domain.BillingStrategy;

namespace Carpark.Domain
{
    public class Session
    {
        public DateTime EntryDateTime { get; set; }
        public DateTime ExitDateTime { get; set; }

        public void Validate()
        {
            // Alternatively, the minimum date for the Exit could be set by a maximum duration in the past (e.g. one
            // day), and the start date rule could be replaced by a maximum duration rule.
            var minimumDate = new DateTime(2020, 1, 1);
            const string dateFormat = "yyyy/MM/dd";

            if (EntryDateTime < minimumDate)
            {
                throw new ValidationException($"entryDateTime must be greater than {minimumDate.ToString(dateFormat)}");
            }
            if (ExitDateTime < minimumDate)
            {
                throw new ValidationException($"exitDateTime must be greater than {minimumDate.ToString(dateFormat)}");
            }
        }

        public TimeSpan Duration() => ExitDateTime.Subtract(EntryDateTime);

        public Billing CalculateBilling()
        {
            // A nod to Gang of Four "Strategy" pattern as this provides full flexibility and easy readibility for
            // other developers.

            // In a real application, most likely these rules would need to be entirely translated into a
            // database-stored BillingStrategy model with predefined rule options and other properties. This would
            // enable the business to self-manage their costing options, perhaps even facilitate temporary periods where
            // certain strategies are allowed or not allowed (e.g. New Years Eve surcharge).
            var strategies = new List<IBillingStrategy>
            {
                new EarlyBirdBillingStrategy(this),
                new NightRateBillingStrategy(this),
                new WeekendRateBillingStrategy(this),
                new StandardRateBillingStrategy(this)
                // By listing Standard last, if the cost from this and another Strategy matches, it will pick one of the
                // others due to the more exciting titles.
            };

            var applicableStrategies = strategies.Where(s => s.IsApplicable());
            if (!applicableStrategies.Any())
            {
                // This should not happen due to the StandardRateBillingStrategy fallback but is good to include
                // nonetheless
                throw new ApplicationException(
                    "This entryDateTime & exitDateTime is not supported, please contact support"
                );
            }

            var cheapestStrategy = applicableStrategies
                .OrderBy(s => s.CalculateCost())
                .First();

            return new Billing(cheapestStrategy.CalculateCost(), "AUD", cheapestStrategy.Name);
        }
    }
}
