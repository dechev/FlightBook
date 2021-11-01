namespace FlightBook.DomainModel
{
    public class LuggagePiece : BaseEntity
    {
        public long FlightRegistrationID { get; set; }
        public long FlightID { get; set; }
        public decimal WeightInKg { get; set; }
        public FlightRegistration FlightRegistration { get; set; }
        public Flight Flight { get; set; }
    }
}