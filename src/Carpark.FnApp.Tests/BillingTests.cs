using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Carpark.FnApp.Tests
{
    // Disclosing my preference here for integration tests (shallow/component where relevant) over unit tests
    // due to increased test confidence and minimised penalty to refactoring.
    // "Test behaviour, not implementation"
    //
    // Though init tests can sometimes have their place where there is deep business logic in pure functions with many
    // cases, and the implementation is proven to be essentially unchanged over a sufficiently long period.
    //
    // Basic contract tests (that is, testing the JSON output) would be a good addition to this to highlight when
    // API consumers/clients will likely face breaking changes.
    public class BillingTests
    {
        [Fact]
        public async Task Empty_Post_Should_Return_400_Bad_Request()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";
            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new BadRequestObjectResult("entryDateTime and exitDateTime is required"));
        }

        [Fact]
        public async Task Default_Exit_Should_Return_400_Bad_Request()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = DateTime.UtcNow
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new BadRequestObjectResult("exitDateTime must be greater than 2020/01/01"));
        }

        [Fact]
        public async Task Entry_9pm_Weekday_Exit_4pm_Should_Return_6_50()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 02, 21, 0, 0), // Thursday
                exitDateTime = new DateTime(2020, 9, 3, 16, 0, 0) // Friday
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(6.5m, "AUD", "Night Rate")));
        }

        [Fact]
        public async Task Entry_6am_Exit_11pm_Should_Return_13()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 2, 6, 0, 0), // Thursday
                exitDateTime = new DateTime(2020, 9, 2, 23, 0, 0) // Thursday
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(13m, "AUD", "Early Bird")));
        }

        [Fact]
        public async Task Entry_6am_Saturday_Exit_11pm_Sunday_Should_Return_13()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 5, 6, 0, 0), // Saturday
                exitDateTime = new DateTime(2020, 9, 6, 23, 0, 0) // Sunday
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(10m, "AUD", "Weekend Rate")));
        }

        [Fact]
        public async Task Entry_9pm_Friday_Exit_4pm_Saturday_Should_Return_6_50()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 4, 21, 0, 0), // Friday
                exitDateTime = new DateTime(2020, 9, 5, 16, 0, 0) // Saturday
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(6.5m, "AUD", "Night Rate")));
        }

        [Fact]
        public async Task Entry_11_30pm_Saturday_Exit_12_29am_Sunday_Should_Return_5()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 5, 23, 30, 0), // Saturday
                exitDateTime = new DateTime(2020, 9, 6, 0, 29, 0) // Sunday
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(5m, "AUD", "Standard Rate")));
        }

        [Fact]
        public async Task Entry_11_30pm_Saturday_Exit_1_29am_Sunday_Should_Return_10()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 5, 23, 30, 0), // Saturday
                exitDateTime = new DateTime(2020, 9, 6, 1, 29, 0) // Sunday
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(10m, "AUD", "Weekend Rate")));
        }

        [Fact]
        public async Task Entry_11_30pm_Wednesday_Exit_1_29am_Thursday_Should_Return_10()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 2, 23, 30, 0), // Wednesday
                exitDateTime = new DateTime(2020, 9, 3, 1, 29, 0) // Thursday
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(10m, "AUD", "Standard Rate")));
        }

        [Fact]
        public async Task Entry_11_30pm_Saturday_Exit_1_29am_Sunday_Next_Week_Should_Return_180()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 5, 23, 30, 0), // Saturday
                exitDateTime = new DateTime(2020, 9, 13, 1, 29, 0) // Sunday next week
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(180m, "AUD", "Standard Rate")));
        }

        [Fact]
        public async Task Entry_11_30pm_Saturday_Exit_11_31pm_Saturday_Next_Week_Should_Return_160()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 5, 23, 30, 0), // Saturday
                exitDateTime = new DateTime(2020, 9, 12, 23, 31, 0) // Saturday next week
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(160m, "AUD", "Standard Rate")));
        }

        [Fact]
        public async Task Entry_11_30pm_Saturday_Exit_11_30pm_Saturday_Next_Week_Should_Return_160()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 5, 23, 30, 0), // Saturday
                exitDateTime = new DateTime(2020, 9, 12, 23, 30, 0) // Saturday next week
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(160m, "AUD", "Standard Rate")));
        }

        [Fact]
        public async Task Entry_11_30pm_Saturday_Exit_11_29pm_Saturday_Next_Week_Should_Return_160()
        {
            var request = (new DefaultHttpContext()).Request;
            request.Method = "POST";

            var body = new StringContent(JsonSerializer.Serialize(new
            {
                entryDateTime = new DateTime(2020, 9, 5, 23, 30, 0), // Saturday
                exitDateTime = new DateTime(2020, 9, 12, 23, 29, 0) // Saturday next week
            }));
            request.Body = await body.ReadAsStreamAsync();

            var response = await Billing.Run(request);
            response.Should().BeEquivalentTo(new OkObjectResult(new Domain.Billing(160m, "AUD", "Standard Rate")));
        }
    }
}
