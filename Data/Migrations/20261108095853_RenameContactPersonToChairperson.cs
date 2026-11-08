using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FINALS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameContactPersonToChairperson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContactPerson",
                table: "Organization",
                newName: "Chairperson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Chairperson",
                table: "Organization",
                newName: "ContactPerson");
        }
    }
}
