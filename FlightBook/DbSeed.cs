using FlightBook.DomainModel;
using System.Collections.Generic;

namespace FlightBook
{
    public static class DbSeed
    {
        public static IEnumerable<Flight> Flights = new List<Flight>
        {
            new Flight { FlightSeatLimit = 140, PerPassengerLuggageCountLimit = 2, PerPassengerLuggageWeightLimit = 60, FlightTotalLuggageWeightLimit = 2500 },
            new Flight { FlightSeatLimit = 50, PerPassengerLuggageCountLimit = 2, PerPassengerLuggageWeightLimit = 50, FlightTotalLuggageWeightLimit = 1500 },
            new Flight { FlightSeatLimit = 10, PerPassengerLuggageCountLimit = 1, PerPassengerLuggageWeightLimit = 40, FlightTotalLuggageWeightLimit = 250 },
        };
    }
}
