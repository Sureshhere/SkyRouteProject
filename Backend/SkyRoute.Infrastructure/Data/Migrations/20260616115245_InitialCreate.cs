using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SkyRoute.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Airlines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airlines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AirlineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlightNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OriginAirportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationAirportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartureTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ArrivalTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    CabinClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseFare = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flights_Airlines_AirlineId",
                        column: x => x.AirlineId,
                        principalTable: "Airlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_DestinationAirportId",
                        column: x => x.DestinationAirportId,
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_OriginAirportId",
                        column: x => x.OriginAirportId,
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingReferenceCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfPassengers = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PricePerPassenger = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PassengerDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PassengerIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassengerDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PassengerDetails_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Airlines",
                columns: new[] { "Id", "Code", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "GA", true, "GlobalAir" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "BW", true, "BudgetWings" }
                });

            migrationBuilder.InsertData(
                table: "Airports",
                columns: new[] { "Id", "City", "Code", "Country", "CountryCode", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("aa000001-0000-0000-0000-000000000001"), "New York", "JFK", "United States", "US", true, "John F. Kennedy International Airport" },
                    { new Guid("aa000001-0000-0000-0000-000000000002"), "Los Angeles", "LAX", "United States", "US", true, "Los Angeles International Airport" },
                    { new Guid("aa000001-0000-0000-0000-000000000003"), "Chicago", "ORD", "United States", "US", true, "O'Hare International Airport" },
                    { new Guid("aa000001-0000-0000-0000-000000000004"), "London", "LHR", "United Kingdom", "GB", true, "Heathrow Airport" },
                    { new Guid("aa000001-0000-0000-0000-000000000005"), "Mumbai", "BOM", "India", "IN", true, "Chhatrapati Shivaji Maharaj International Airport" },
                    { new Guid("aa000001-0000-0000-0000-000000000006"), "New Delhi", "DEL", "India", "IN", true, "Indira Gandhi International Airport" }
                });

            migrationBuilder.InsertData(
                table: "Flights",
                columns: new[] { "Id", "AirlineId", "ArrivalTime", "BaseFare", "CabinClass", "DepartureTime", "DestinationAirportId", "DurationMinutes", "FlightNumber", "IsActive", "OriginAirportId" },
                values: new object[,]
                {
                    { new Guid("00000001-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 11, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "GA001", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000002-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 16, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "BW002", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000003-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 19, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "GA003", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000004-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 0, 15, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "BW004", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000005-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 11, 30, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "GA005", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000006-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 16, 0, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "BW006", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000007-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 19, 30, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "GA007", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000008-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 0, 15, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "BW008", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000009-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 11, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "GA009", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000000a-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 16, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "BW010", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000000b-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 19, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "GA011", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000000c-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 0, 15, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000002"), 330, "BW012", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000000d-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 11, 15, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "GA013", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("0000000e-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 15, 45, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "BW014", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("0000000f-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 19, 15, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "GA015", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("00000010-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 0, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "BW016", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("00000011-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 11, 15, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "GA017", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("00000012-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 15, 45, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "BW018", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("00000013-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 19, 15, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "GA019", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("00000014-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 0, 0, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "BW020", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("00000015-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 11, 15, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "GA021", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("00000016-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 15, 45, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "BW022", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("00000017-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 19, 15, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "GA023", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("00000018-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 0, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 315, "BW024", true, new Guid("aa000001-0000-0000-0000-000000000002") },
                    { new Guid("00000019-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "GA025", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000001a-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 13, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "BW026", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000001b-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "GA027", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000001c-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 21, 15, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "BW028", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000001d-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 30, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "GA029", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000001e-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 13, 0, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "BW030", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000001f-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 30, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "GA031", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000020-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 21, 15, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "BW032", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000021-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "GA033", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000022-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 13, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "BW034", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000023-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "GA035", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000024-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 21, 15, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000003"), 150, "BW036", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000025-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 25, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "GA037", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("00000026-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 12, 55, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "BW038", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("00000027-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 25, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "GA039", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("00000028-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 21, 10, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "BW040", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("00000029-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 25, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "GA041", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("0000002a-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 12, 55, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "BW042", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("0000002b-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 25, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "GA043", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("0000002c-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 21, 10, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "BW044", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("0000002d-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 25, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "GA045", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("0000002e-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 12, 55, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "BW046", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("0000002f-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 25, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "GA047", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("00000030-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 21, 10, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 145, "BW048", true, new Guid("aa000001-0000-0000-0000-000000000003") },
                    { new Guid("00000031-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 10, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "GA049", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000032-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 12, 40, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "BW050", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000033-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 10, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "GA051", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000034-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 20, 55, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "BW052", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000035-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 10, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "GA053", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000036-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 12, 40, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "BW054", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000037-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 10, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "GA055", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000038-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 20, 55, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "BW056", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000039-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 10, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "GA057", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("0000003a-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 12, 40, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "BW058", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("0000003b-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 10, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "GA059", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("0000003c-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 20, 55, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 130, "BW060", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("0000003d-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 5, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "GA061", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("0000003e-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 12, 35, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "BW062", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("0000003f-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 5, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "GA063", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000040-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 20, 50, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "BW064", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000041-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 5, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "GA065", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000042-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 12, 35, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "BW066", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000043-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 5, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "GA067", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000044-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 20, 50, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "BW068", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000045-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 8, 5, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "GA069", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000046-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 12, 35, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "BW070", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000047-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 16, 5, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "GA071", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000048-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 20, 50, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 125, "BW072", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000049-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 13, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "GA073", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000004a-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 17, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "BW074", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000004b-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 21, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "GA075", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000004c-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 1, 45, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "BW076", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000004d-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 13, 0, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "GA077", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000004e-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 17, 30, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "BW078", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000004f-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 21, 0, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "GA079", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000050-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 1, 45, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "BW080", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000051-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 13, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "GA081", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000052-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 17, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "BW082", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000053-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 21, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "GA083", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000054-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 1, 45, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 420, "BW084", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000055-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 13, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "GA085", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000056-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 18, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "BW086", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000057-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 21, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "GA087", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000058-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 2, 15, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "BW088", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000059-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 13, 30, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "GA089", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000005a-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 18, 0, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "BW090", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000005b-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 21, 30, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "GA091", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000005c-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 2, 15, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "BW092", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000005d-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 13, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "GA093", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000005e-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 18, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "BW094", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000005f-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 21, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "GA095", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000060-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 2, 15, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 450, "BW096", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000061-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 20, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "GA097", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000062-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 1, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "BW098", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000063-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(1, 4, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "GA099", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000064-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 9, 15, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "BW100", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000065-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 20, 30, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "GA101", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000066-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 1, 0, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "BW102", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000067-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(1, 4, 30, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "GA103", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000068-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 9, 15, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "BW104", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("00000069-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 20, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "GA105", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000006a-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 1, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "BW106", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000006b-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(1, 4, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "GA107", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000006c-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 9, 15, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000005"), 870, "BW108", true, new Guid("aa000001-0000-0000-0000-000000000001") },
                    { new Guid("0000006d-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 21, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "GA109", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("0000006e-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 1, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "BW110", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("0000006f-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(1, 5, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "GA111", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000070-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 9, 45, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "BW112", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000071-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 21, 0, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "GA113", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000072-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 1, 30, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "BW114", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000073-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(1, 5, 0, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "GA115", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000074-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 9, 45, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "BW116", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000075-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 21, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "GA117", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000076-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 1, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "BW118", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000077-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(1, 5, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "GA119", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000078-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 9, 45, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000001"), 900, "BW120", true, new Guid("aa000001-0000-0000-0000-000000000005") },
                    { new Guid("00000079-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 15, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "GA121", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000007a-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 19, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "BW122", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000007b-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 23, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "GA123", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000007c-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 3, 45, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "BW124", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000007d-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 15, 0, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "GA125", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000007e-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 19, 30, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "BW126", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("0000007f-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 23, 0, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "GA127", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000080-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 3, 45, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "BW128", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000081-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 15, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "GA129", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000082-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 19, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "BW130", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000083-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 23, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "GA131", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000084-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 3, 45, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000006"), 540, "BW132", true, new Guid("aa000001-0000-0000-0000-000000000004") },
                    { new Guid("00000085-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 15, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "GA133", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000086-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 20, 0, 0, 0), 150m, "Economy", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "BW134", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000087-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 23, 30, 0, 0), 150m, "Economy", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "GA135", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000088-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 4, 15, 0, 0), 150m, "Economy", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "BW136", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000089-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 15, 30, 0, 0), 450m, "Business", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "GA137", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("0000008a-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 20, 0, 0, 0), 450m, "Business", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "BW138", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("0000008b-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 23, 30, 0, 0), 450m, "Business", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "GA139", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("0000008c-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 4, 15, 0, 0), 450m, "Business", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "BW140", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("0000008d-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 15, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 6, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "GA141", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("0000008e-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(0, 20, 0, 0, 0), 900m, "FirstClass", new TimeSpan(0, 10, 30, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "BW142", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("0000008f-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"), new TimeSpan(0, 23, 30, 0, 0), 900m, "FirstClass", new TimeSpan(0, 14, 0, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "GA143", true, new Guid("aa000001-0000-0000-0000-000000000006") },
                    { new Guid("00000090-0000-0000-0000-000000000000"), new Guid("22222222-2222-2222-2222-222222222222"), new TimeSpan(1, 4, 15, 0, 0), 900m, "FirstClass", new TimeSpan(0, 18, 45, 0, 0), new Guid("aa000001-0000-0000-0000-000000000004"), 570, "BW144", true, new Guid("aa000001-0000-0000-0000-000000000006") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Airlines_Name",
                table: "Airlines",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Airports_Code",
                table: "Airports",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingReferenceCode",
                table: "Bookings",
                column: "BookingReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FlightId",
                table: "Bookings",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AirlineId",
                table: "Flights",
                column: "AirlineId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DestinationAirportId",
                table: "Flights",
                column: "DestinationAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_OriginAirportId",
                table: "Flights",
                column: "OriginAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_PassengerDetails_BookingId",
                table: "PassengerDetails",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PassengerDetails");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Airlines");

            migrationBuilder.DropTable(
                name: "Airports");
        }
    }
}
