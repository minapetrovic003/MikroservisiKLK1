using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Predavaci",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Titula = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OblastStrucnosti = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predavaci", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Predavanja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tema = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrajanjePredavanja = table.Column<int>(type: "int", nullable: false),
                    Pocetak = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DogadjajId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PredavacId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predavanja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predavanja_Predavaci_PredavacId",
                        column: x => x.PredavacId,
                        principalTable: "Predavaci",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Predavanja_PredavacId",
                table: "Predavanja",
                column: "PredavacId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Predavanja");

            migrationBuilder.DropTable(
                name: "Predavaci");
        }
    }
}
