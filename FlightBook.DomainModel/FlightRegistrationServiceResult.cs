using System.Collections.Generic;

namespace FlightBook.DomainModel
{
    public record FlightRegistrationServiceResult(bool Success, FlightRegistrationServiceErrorEnum ErrorCode, IEnumerable<string> ErrorMessages)
    {
    }
}