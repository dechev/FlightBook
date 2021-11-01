using FlightBook.DomainModel;
using FlightBook.ServiceInterfaces;
using System.Threading;
using System.Threading.Tasks;

namespace FlightBook.Persistence.EFCore
{
    public class FlightRegistrationCommands : IFlightRegistrationCommands
    {
        private readonly FlightBookContext _context;

        public FlightRegistrationCommands(
            FlightBookContext context
            )
        {
            _context = context;
        }

        public async Task<int> BookFlightAsync(FlightRegistration flightRegistration, CancellationToken token = default)
        {
            await _context.FlightRegistrations.AddAsync(flightRegistration, token).ConfigureAwait(false);
            return await _context.SaveChangesAsync(token).ConfigureAwait(false);
        }
    }
}