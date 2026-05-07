using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizacijaDogadjajaApp.UcesniciAPI.Migrations
{
    /// <inheritdoc />
    public partial class DodatProcessedMessagesIDogadjajiRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DogadjajiReference",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DogadjajId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NazivDogadjaja = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgendaDogadjaja = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumIVreme = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WelcomeMessage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogadjajiReference", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DogadjajiReference_DogadjajId",
                table: "DogadjajiReference",
                column: "DogadjajId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessages_EventId",
                table: "ProcessedMessages",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DogadjajiReference");

            migrationBuilder.DropTable(
                name: "ProcessedMessages");
        }
    }
}
