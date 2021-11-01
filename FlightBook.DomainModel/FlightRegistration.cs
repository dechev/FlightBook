using System.Collections.Generic;

namespace FlightBook.DomainModel
{
    public class FlightRegistration : BaseEntity
    {
        public long FlightID { get; set; }
        public long PassengerID { get; set; }
        public Flight Flight { get; set; }
        public Passenger Passenger { get; set; }
        public List<LuggagePiece> LuggagePieces { get; set; }
    }
}