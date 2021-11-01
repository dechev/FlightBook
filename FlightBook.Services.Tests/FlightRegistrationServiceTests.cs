using FlightBook.DomainModel;
using FlightBook.ServiceInterfaces;
using Medallion.Threading;
using Medallion.Threading.FileSystem;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FlightBook.Services.Tests
{
    [TestFixture]
    public class FlightRegistrationServiceTests
    {
        private static readonly string _lockFileDirectory = Path.Combine(Path.GetTempPath(), nameof(FlightRegistrationServiceTests));
        private static DirectoryInfo _lockFileDirectoryInfo => new(_lockFileDirectory);

        private readonly MockRepository _mockRepo;
        private readonly Mock<IEntityRepository<Flight>> _flights;
        private readonly Mock<IEntityRepository<Passenger>> _passengers;
        private readonly Mock<IEntityRepository<FlightRegistration>> _registrations;
        private readonly Mock<IEntityRepository<LuggagePiece>> _luggage;
        private readonly Mock<IFlightRegistrationCommands> _flightRegistrationCommands;
        private readonly IDistributedLockProvider _lockProvider;

        public FlightRegistrationServiceTests()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _flights = _mockRepo.Create<IEntityRepository<Flight>>();
            _passengers = _mockRepo.Create<IEntityRepository<Passenger>>();
            _registrations = _mockRepo.Create<IEntityRepository<FlightRegistration>>();
            _luggage = _mockRepo.Create<IEntityRepository<LuggagePiece>>();
            _flightRegistrationCommands = _mockRepo.Create<IFlightRegistrationCommands>();
            _lockProvider = new FileDistributedSynchronizationProvider(_lockFileDirectoryInfo);
        }

        [OneTimeSetUp]
        public void Setup()
        {
            if (Directory.Exists(_lockFileDirectory))
            {
                Directory.Delete(_lockFileDirectory, recursive: true);
            }
            var flights = new List<Flight>
            {
                new Flight{ ID = 1, FlightSeatLimit = 10, FlightTotalLuggageWeightLimit = 100, PerPassengerLuggageCountLimit = 2, PerPassengerLuggageWeightLimit = 20 },
                new Flight{ ID = 2, FlightSeatLimit = 5, FlightTotalLuggageWeightLimit = 100, PerPassengerLuggageCountLimit = 2, PerPassengerLuggageWeightLimit = 20 },
                new Flight{ ID = 3, FlightSeatLimit = 10, FlightTotalLuggageWeightLimit = 50, PerPassengerLuggageCountLimit = 2, PerPassengerLuggageWeightLimit = 50 },
            };
            var registrations = new List<FlightRegistration>
            {
                new FlightRegistration{ID = 1, FlightID = 1, PassengerID = 1},
                new FlightRegistration{ID = 2, FlightID = 2, PassengerID = 3},
                new FlightRegistration{ID = 3, FlightID = 2, PassengerID = 5},
                new FlightRegistration{ID = 4, FlightID = 2, PassengerID = 7},
                new FlightRegistration{ID = 5, FlightID = 2, PassengerID = 9},
                new FlightRegistration{ID = 6, FlightID = 2, PassengerID = 11},
                new FlightRegistration{ID = 7, FlightID = 3, PassengerID = 13},
            };
            var luggage = new List<LuggagePiece>
            {
                new LuggagePiece{ID = 1, FlightID = 1, FlightRegistrationID = 1, WeightInKg = 10},
                new LuggagePiece{ID = 2, FlightID = 1, FlightRegistrationID = 1, WeightInKg = 8},
                new LuggagePiece{ID = 3, FlightID = 3, FlightRegistrationID = 6, WeightInKg = 20},
                new LuggagePiece{ID = 4, FlightID = 3, FlightRegistrationID = 6, WeightInKg = 20},
            };

            _flights.Setup(_ => _.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((long id, CancellationToken _) => flights.FirstOrDefault(x => x.ID == id));
            _passengers.Setup(_ => _.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((long id, CancellationToken _) => id % 2 == 0 ? new Passenger { ID = id } : null);
            _registrations.Setup(_ => _.GetAsync(It.IsAny<Expression<Func<FlightRegistration, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expression<Func<FlightRegistration, bool>> filter, CancellationToken _) =>
                    filter == null ? registrations.ToList() : registrations.Where(filter.Compile()).ToList()
                );
            _luggage.Setup(_ => _.GetAsync(It.IsAny<Expression<Func<LuggagePiece, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expression<Func<LuggagePiece, bool>> filter, CancellationToken _) =>
                    filter == null ? luggage.ToList() : luggage.Where(filter.Compile()).ToList()
                );
            _flightRegistrationCommands.Setup(_ => _.BookFlightAsync(It.IsAny<FlightRegistration>(), It.IsAny<CancellationToken>()))
                .Returns(async (FlightRegistration fr, CancellationToken _) =>
                {
                    if (fr is null) return 0;
                    if (fr.FlightID == 3)
                    {
                        await Task.Delay(2000, _);
                        return 0;
                    }
                    var result = 1;
                    if (fr.LuggagePieces != null) result += fr.LuggagePieces.Count;
                    return result;
                });
        }

        private static readonly object[] _validationFailSourceLists =
        {
            new object[] {1, 2, new decimal[] { 5, -10 }, FlightRegistrationServiceErrorEnum.PassengerLuggageInvalidWeight},
            new object[] {4, 2, null, FlightRegistrationServiceErrorEnum.NonExistingFlight},
            new object[] {1, 1, null, FlightRegistrationServiceErrorEnum.NonExistingPassenger},
            new object[] {2, 2, null, FlightRegistrationServiceErrorEnum.FlightFull},
            new object[] {1, 2, new decimal[] { 10, 5, 5 }, FlightRegistrationServiceErrorEnum.PassengerLuggageCountLimitExceeded},
            new object[] {1, 2, new decimal[] { 10, 15 }, FlightRegistrationServiceErrorEnum.PassengerLuggageWeightLimitExceeded},
            new object[] {3, 2, new decimal[] { 15 }, FlightRegistrationServiceErrorEnum.FlightTotalLuggageWeightLimitExceeded},
            new object[] {3, 2, null, FlightRegistrationServiceErrorEnum.ProcessingError},
        };

        [Test]
        [TestCaseSource("_validationFailSourceLists")]
        public async Task ValidationFailTests(long flightId, long passengerId, decimal[] luggage, FlightRegistrationServiceErrorEnum expectedError)
        {
            var service = new FlightRegistrationService(_flights.Object, _passengers.Object, _registrations.Object, _luggage.Object, _flightRegistrationCommands.Object, _lockProvider);

            var result = await service.PlaceRegistrationAsync(flightId, passengerId, luggage);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(expectedError, result.ErrorCode);
        }

        private static readonly object[] _validationPassSourceLists =
        {
            new object[] {1, 2, null},
            new object[] {1, 2, new decimal[] { 19.5m }},
            new object[] {1, 2, new decimal[] { 5, 5 }},
        };

        [Test]
        [TestCaseSource("_validationPassSourceLists")]
        public async Task ValidationPassTests(long flightId, long passengerId, decimal[] luggage)
        {
            var service = new FlightRegistrationService(_flights.Object, _passengers.Object, _registrations.Object, _luggage.Object, _flightRegistrationCommands.Object, _lockProvider);

            var result = await service.PlaceRegistrationAsync(flightId, passengerId, luggage);
            //Assert.IsTrue(result.Success);
            Assert.AreEqual(FlightRegistrationServiceErrorEnum.Success, result.ErrorCode);
            Assert.IsNull(result.ErrorMessages);
        }

        [Test]
        public async Task SimulateTimeOutTest()
        {
            var service = new FlightRegistrationService(_flights.Object, _passengers.Object, _registrations.Object, _luggage.Object, _flightRegistrationCommands.Object, _lockProvider);

            var cts = new CancellationTokenSource(200);
            var result = await service.PlaceRegistrationAsync(3, 2, null, cts.Token);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(FlightRegistrationServiceErrorEnum.GeneralError, result.ErrorCode);
        }
    }
}