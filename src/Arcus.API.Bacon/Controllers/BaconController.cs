using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Filters;

namespace Arcus.API.Bacon.Controllers
{
    /// <summary>
    /// API endpoint to get bacon.
    /// </summary>
    [ApiController]
    [Route("api/v1/bacon")]
    public class BaconController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaconController"/> class.
        /// </summary>
        public BaconController()
        {
        }

        /// <summary>
        ///     Get Bacon
        /// </summary>
        /// <remarks>Provides an overview of various bacon flavors.</remarks>
        /// <response code="200">API is healthy</response>
        /// <response code="503">API is unhealthy or in degraded state</response>
        [HttpGet(Name = "Bacon_Get")]
        [ProducesResponseType(typeof(HealthReport), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HealthReport), StatusCodes.Status503ServiceUnavailable)]
        [SwaggerResponseHeader(200, "RequestId", "string", "The header that has a request ID that uniquely identifies this operation call")]
        [SwaggerResponseHeader(200, "X-Transaction-Id", "string", "The header that has the transaction ID is used to correlate multiple operation calls.")]
        public IActionResult Get()
        {
            var baconFlavors = new List<string>
            {
                "Infamous Black Pepper Bacon",
                "Italian Bacon",
                "Raspberry Chipotle",
                "Pumpkin Pie Spiced",
                "Apple Cinnamon",
                "Jalapeño Bacon",
                "Cajun Style"
            };

            return Ok(baconFlavors);
        }
    }
}
