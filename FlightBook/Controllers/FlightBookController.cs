using FlightBook.DomainModel;
using FlightBook.Models;
using FlightBook.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlightBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json", "application/problem+json")]
    public class FlightBookController : ControllerBase
    {
        private readonly ILogger<FlightBookController> _logger;
        private readonly IFlightRegistrationService _flightRegistrationService;
        private readonly TimeSpan _requestTimeout;

        public FlightBookController(
            ILogger<FlightBookController> logger,
            IConfiguration configuration,
            IFlightRegistrationService flightRegistrationService
            )
        {
            _logger = logger;
            _flightRegistrationService = flightRegistrationService;
            _requestTimeout = configuration.GetValue("RequestTimeOut", TimeSpan.FromSeconds(10));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<FlightRegistrationResponseModel>> Post([FromBody]FlightRegistrationRequestModel requestModel)
        {
            if (requestModel == null) return BadRequest();

            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["ActionName"] = nameof(Post),
                [nameof(FlightRegistrationRequestModel)] = requestModel
            });

            var cts = new CancellationTokenSource(_requestTimeout);
            try
            {
                var serviceResult = await _flightRegistrationService.PlaceRegistrationAsync(requestModel.FlightID, requestModel.PassengerID, requestModel.LuggagePieces, cts.Token).ConfigureAwait(false);
                
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    using var loggerErrorScope = _logger.BeginScope(new Dictionary<string, object>
                    {
                        [nameof(FlightRegistrationServiceResult)] = serviceResult
                    });
                    _logger.LogDebug(new EventId((int)serviceResult.ErrorCode, serviceResult.ErrorCode.ToString()), "Flight registration service result.");
                }

                if (!serviceResult.Success)
                {
                    return Problem(((int)serviceResult.ErrorCode).ToString(), null, StatusCodes.Status406NotAcceptable, "Flight Registration Unsuccessful.");
                }

                return Ok(new FlightRegistrationResponseModel(serviceResult.Success, (int)serviceResult.ErrorCode));
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(new EventId(408, "Time Out"), ex, "Timed out.");
                return StatusCode(StatusCodes.Status408RequestTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(503, "Error"), ex, "Flight registration failed.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}