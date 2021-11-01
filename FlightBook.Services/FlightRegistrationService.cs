using FlightBook.DomainModel;
using FlightBook.ServiceInterfaces;
using Medallion.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlightBook.Services
{
    public class FlightRegistrationService : IFlightRegistrationService
    {
        private readonly IEntityRepository<Flight> _flights;
        private readonly IEntityRepository<Passenger> _passengers;
        private readonly IEntityRepository<FlightRegistration> _registrations;
        private readonly IEntityRepository<LuggagePiece> _luggage;
        private readonly IFlightRegistrationCommands _flightRegistrationCommands;
        private readonly IDistributedLockProvider _lockProvider;

        public FlightRegistrationService(
            IEntityRepository<Flight> flights,
            IEntityRepository<Passenger> passengers,
            IEntityRepository<FlightRegistration> registrations,
            IEntityRepository<LuggagePiece> luggage,
            IFlightRegistrationCommands flightRegistrationCommands,
            IDistributedLockProvider lockProvider
            )
        {
            _flights = flights;
            _passengers = passengers;
            _registrations = registrations;
            _luggage = luggage;
            _flightRegistrationCommands = flightRegistrationCommands;
            _lockProvider = lockProvider;
        }

        public async Task<FlightRegistrationServiceResult> PlaceRegistrationAsync(long flightID, long passengerID, IEnumerable<decimal> luggagePieces, CancellationToken cancellationToken = default)
        {
            // validate luggage pieces
            // validate flightID
            // validate luggage limits
            // validate passengerID
            // lock flight
            //   get flight reservations
            //   check overbooking
            //   if checking in any luggage
            //     get flight luggage
            //     check total weight limit
            //   call command to book
            // return success

            if (luggagePieces != null)
            {
                var luggageValidation = luggagePieces.Select((w, i) => w < 0 ? $"Invalid weight {w:f3} kg for luggage piece #{i + 1}." : null).Where(s => s != null).ToArray();
                if (luggageValidation.Length > 0)
                {
                    return new(false, FlightRegistrationServiceErrorEnum.PassengerLuggageInvalidWeight, luggageValidation);
                }
            }
            try
            {
                var flight = await _flights.GetByIdAsync(flightID, cancellationToken).ConfigureAwait(false);
                if (flight == null)
                {
                    return new(false, FlightRegistrationServiceErrorEnum.NonExistingFlight, new string[] { $"Flight with ID {flightID} not found." });
                }
                
                if (luggagePieces != null)
                {
                    if (luggagePieces.Count() > flight.PerPassengerLuggageCountLimit)
                    {
                        return new(false, FlightRegistrationServiceErrorEnum.PassengerLuggageCountLimitExceeded, new string[] { $"Number of luggage pieces exceeds the flight limit of {flight.PerPassengerLuggageCountLimit} pieces per passenger." });
                    }
                    if (luggagePieces.Sum() > flight.PerPassengerLuggageWeightLimit)
                    {
                        return new(false, FlightRegistrationServiceErrorEnum.PassengerLuggageWeightLimitExceeded, new string[] { $"The total weight of luggage ({luggagePieces.Sum():f3}kg) exceeds the flight limit of {flight.PerPassengerLuggageWeightLimit} kg per passenger." });
                    }
                }

                var passenger = await _passengers.GetByIdAsync(passengerID, cancellationToken).ConfigureAwait(false);
                if (passenger == null)
                {
                    return new(false, FlightRegistrationServiceErrorEnum.NonExistingPassenger, new string[] { $"Passenger with ID {passengerID} not found." });
                }

                var @lock = _lockProvider.CreateLock($"FlightLock_{flightID}");
                await using (await @lock.AcquireAsync(null, cancellationToken))
                {
                    var passengerManifest = await _registrations.GetAsync(x => x.FlightID == flightID, cancellationToken).ConfigureAwait(false);
                    if (passengerManifest.Count() >= flight.FlightSeatLimit)
                    {
                        return new(false, FlightRegistrationServiceErrorEnum.FlightFull, new string[] { $"Flight with ID {flightID} is fully booked." });
                    }

                    if (luggagePieces?.Any() == true)
                    {
                        var flightLuggagePieces = await _luggage.GetAsync(x => x.FlightID == flightID, cancellationToken).ConfigureAwait(false);
                        if (luggagePieces.Sum() + flightLuggagePieces.Sum(x => x.WeightInKg) > flight.FlightTotalLuggageWeightLimit)
                        {
                            return new(false, FlightRegistrationServiceErrorEnum.FlightTotalLuggageWeightLimitExceeded, new string[] { $"Total luggage weight limit for flight with ID {flightID} is exceeded." });
                        }
                    }

                    var rowsInserted = await _flightRegistrationCommands.BookFlightAsync(
                        new FlightRegistration
                        {
                            FlightID = flightID,
                            PassengerID = passengerID,
                            LuggagePieces = luggagePieces?.Select(x => new LuggagePiece
                            {
                                FlightID = flightID,
                                WeightInKg = x
                            }).ToList()
                        },
                        cancellationToken)
                        .ConfigureAwait(false);

                    var expectedRows = 1;
                    if (luggagePieces != null) expectedRows += luggagePieces.Count();
                    if (rowsInserted != expectedRows)
                    {
                        return new(false, FlightRegistrationServiceErrorEnum.ProcessingError, new string[] { "An error occurred." });
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: log exception
                return new(false, FlightRegistrationServiceErrorEnum.GeneralError, new string[] { "An error occurred.", /* TODO: remove:*/ ex.Message });
            }

            return new(true, FlightRegistrationServiceErrorEnum.Success, null);
        }
    }
}