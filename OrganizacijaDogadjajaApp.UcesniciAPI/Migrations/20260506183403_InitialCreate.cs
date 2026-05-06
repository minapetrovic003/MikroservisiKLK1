using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizacijaDogadjajaApp.UcesniciAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ucesnici",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ucesnici", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prijave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DatumPrijave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DogadjajId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UcesnikId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prijave", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prijave_Ucesnici_UcesnikId",
                        column: x => x.UcesnikId,
                        principalTable: "Ucesnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prijave_UcesnikId",
                table: "Prijave",
                column: "UcesnikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prijave");

            migrationBuilder.DropTable(
                name: "Ucesnici");
        }
    }
}
