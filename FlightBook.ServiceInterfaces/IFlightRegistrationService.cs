using FlightBook.DomainModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlightBook.ServiceInterfaces
{
    public interface IFlightRegistrationService
    {
        Task<FlightRegistrationServiceResult> PlaceRegistrationAsync(long flightID, long passengerID, IEnumerable<decimal> luggagePieces, CancellationToken cancellationToken = default);
    }
}