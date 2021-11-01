using System.Collections.Generic;

namespace FlightBook.DomainModel
{
    public class Passenger : BaseEntity
    {
        // TODO: more passenger info - out of scope
        public List<FlightRegistration> FlightRegistrations { get; set; }
    }
}