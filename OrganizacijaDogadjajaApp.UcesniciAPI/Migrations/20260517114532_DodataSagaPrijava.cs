using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizacijaDogadjajaApp.UcesniciAPI.Migrations
{
    /// <inheritdoc />
    public partial class DodataSagaPrijava : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SagaPrijave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DogadjajId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UcesnikId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RezervacijaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DatumPrijave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Otkazana = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaPrijave", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SagaPrijave");
        }
    }
}
