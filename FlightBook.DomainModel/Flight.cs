using System.Collections.Generic;

namespace FlightBook.DomainModel
{
    public class Flight : BaseEntity
    {
        public int FlightSeatLimit { get; set; }
        public decimal FlightTotalLuggageWeightLimit { get; set; }
        public int PerPassengerLuggageCountLimit { get; set; }
        public decimal PerPassengerLuggageWeightLimit { get; set; }
        public List<FlightRegistration> FlightRegistrations { get; set; }
        public List<LuggagePiece> LuggagePieces { get; set; }
    }
}