using System.Collections.Generic;

namespace FlightBook.Models
{
    public class FlightRegistrationRequestModel
    {
        public long FlightID { get; set; }

        public long PassengerID { get; set; }

        public IEnumerable<decimal> LuggagePieces { get; set; }
    }
}
