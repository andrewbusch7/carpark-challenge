using Carpark.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;

namespace Carpark.FnApp
{
    public static class Billing
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        // Return type of IActionResult has been used as the default choice. HttpResponseMessage would likely be better
        // for larger, more complex projects where precision over the HTTP response is required.
        //
        // As these costs would be advertised publicly, there is no need for authorization tokens.
        [FunctionName("Billing")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "billing")] HttpRequest req
        )
        {
            try
            {
                // There is a conceptual overlap between a "session" at a carpark and the resulting billing. it may be
                // correct for a Session to have a Billing, therefore this endpoint could be POST /session and return a
                // Session object with Billing as a property.
                var session = await JsonSerializer.DeserializeAsync<Session>(req.Body, _jsonSerializerOptions);

                // If there ends up being DB / external service integration, this block should move to a service layer
                // with the required dependencies injected, and this Azure Function class taking the service as a
                // dependency injection. The Validate() and CalculateBilling() functions would remain on Session in
                // favour of a Rich Domain Model.
                //
                // Rich Domain Models (that is, not just properties but also methods) more distinctly express the domain
                // functionality by providing decoupling from the integration of the domain behaviour (e.g. database and
                // external service integration). They also make unit testing more simple.
                session.Validate();
                var cost = session.CalculateBilling();

                return new OkObjectResult(cost);
            }
            catch (JsonException)
            {
                return new BadRequestObjectResult("entryDateTime and exitDateTime is required");
            }
            catch (ValidationException e)
            {
                // At scale, as much of these exception handlers as possible should move to a middleware implementation.
                return new BadRequestObjectResult(e.Message);
            }
            catch (ApplicationException e)
            {
                return new UnprocessableEntityObjectResult(e.Message);
            }
        }
    }
}
