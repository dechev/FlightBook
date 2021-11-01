using Microsoft.EntityFrameworkCore.Migrations;

namespace FlightBook.Persistence.EFCore.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightSeatLimit = table.Column<int>(type: "int", nullable: false),
                    FlightTotalLuggageWeightLimit = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    PerPassengerLuggageCountLimit = table.Column<int>(type: "int", nullable: false),
                    PerPassengerLuggageWeightLimit = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Passengers",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passengers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FlightRegistrations",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightID = table.Column<long>(type: "bigint", nullable: false),
                    PassengerID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightRegistrations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FlightRegistrations_Flights_FlightID",
                        column: x => x.FlightID,
                        principalTable: "Flights",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlightRegistrations_Passengers_PassengerID",
                        column: x => x.PassengerID,
                        principalTable: "Passengers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LuggagePieces",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightRegistrationID = table.Column<long>(type: "bigint", nullable: false),
                    FlightID = table.Column<long>(type: "bigint", nullable: false),
                    WeightInKg = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LuggagePieces", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LuggagePieces_FlightRegistrations_FlightRegistrationID",
                        column: x => x.FlightRegistrationID,
                        principalTable: "FlightRegistrations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LuggagePieces_Flights_FlightID",
                        column: x => x.FlightID,
                        principalTable: "Flights",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightRegistrations_FlightID",
                table: "FlightRegistrations",
                column: "FlightID");

            migrationBuilder.CreateIndex(
                name: "IX_FlightRegistrations_PassengerID",
                table: "FlightRegistrations",
                column: "PassengerID");

            migrationBuilder.CreateIndex(
                name: "IX_LuggagePieces_FlightID",
                table: "LuggagePieces",
                column: "FlightID");

            migrationBuilder.CreateIndex(
                name: "IX_LuggagePieces_FlightRegistrationID",
                table: "LuggagePieces",
                column: "FlightRegistrationID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LuggagePieces");

            migrationBuilder.DropTable(
                name: "FlightRegistrations");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Passengers");
        }
    }
}
