using FlightBook.DomainModel;
using System.Threading;
using System.Threading.Tasks;

namespace FlightBook.ServiceInterfaces
{
    public interface IFlightRegistrationCommands
    {
        Task<int> BookFlightAsync(FlightRegistration flightRegistration, CancellationToken token = default);
    }
}