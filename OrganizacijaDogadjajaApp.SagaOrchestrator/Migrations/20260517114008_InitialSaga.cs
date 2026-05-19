using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizacijaDogadjajaApp.SagaOrchestrator.Migrations
{
    /// <inheritdoc />
    public partial class InitialSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SagaInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DogadjajId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UcesnikId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RezervacijaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RasporedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrijavaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GreskaOpis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    KreiranaU = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AzuriranjaU = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaInstances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SagaInstances_DogadjajId",
                table: "SagaInstances",
                column: "DogadjajId");

            migrationBuilder.CreateIndex(
                name: "IX_SagaInstances_Status",
                table: "SagaInstances",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SagaInstances");
        }
    }
}
