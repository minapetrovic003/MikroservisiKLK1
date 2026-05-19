using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Migrations
{
    /// <inheritdoc />
    public partial class DodatSagaRaspored : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SagaRasporedi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DogadjajId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UcesnikId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KreiranaU = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Obrisan = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaRasporedi", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SagaRasporedi");
        }
    }
}
