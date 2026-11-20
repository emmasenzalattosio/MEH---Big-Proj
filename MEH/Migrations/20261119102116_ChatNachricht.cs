using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEH.Migrations
{
    /// <inheritdoc />
    public partial class ChatNachricht : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatNachricht",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VonMitarbeiterID = table.Column<int>(type: "int", nullable: false),
                    AnMitarbeiterID = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    GesendetAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gelesen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatNachricht", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ChatNachricht_Mitarbeiter_AnMitarbeiterID",
                        column: x => x.AnMitarbeiterID,
                        principalTable: "Mitarbeiter",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatNachricht_Mitarbeiter_VonMitarbeiterID",
                        column: x => x.VonMitarbeiterID,
                        principalTable: "Mitarbeiter",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatNachricht_AnMitarbeiterID",
                table: "ChatNachricht",
                column: "AnMitarbeiterID");

            migrationBuilder.CreateIndex(
                name: "IX_ChatNachricht_VonMitarbeiterID",
                table: "ChatNachricht",
                column: "VonMitarbeiterID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatNachricht");
        }
    }
}
