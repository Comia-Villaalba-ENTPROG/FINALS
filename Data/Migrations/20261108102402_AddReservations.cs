using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FINALS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackingNumber = table.Column<string>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    VenueId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActivityName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ReservationDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    ExpectedAttendees = table.Column<int>(type: "INTEGER", nullable: false),
                    RequesterName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RequesterEmail = table.Column<string>(type: "TEXT", nullable: false),
                    RequesterContact = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentStep = table.Column<int>(type: "INTEGER", nullable: false),
                    DateSubmitted = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Venue_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_OrganizationId",
                table: "Reservations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_VenueId",
                table: "Reservations",
                column: "VenueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");
        }
    }
}
