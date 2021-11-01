namespace FlightBook.DomainModel
{
    public enum FlightRegistrationServiceErrorEnum : int
    {
        Success = 0,
        GeneralError,
        ProcessingError,
        NonExistingFlight,
        NonExistingPassenger,
        FlightFull,
        PassengerLuggageInvalidWeight,
        PassengerLuggageCountLimitExceeded,
        PassengerLuggageWeightLimitExceeded,
        FlightTotalLuggageWeightLimitExceeded,
    }
}