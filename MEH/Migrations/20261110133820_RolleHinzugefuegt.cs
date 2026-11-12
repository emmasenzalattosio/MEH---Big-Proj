using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEH.Migrations
{
    /// <inheritdoc />
    public partial class RolleHinzugefuegt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Titel",
                table: "Ticket",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Beschreibung",
                table: "Ticket",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "HatNeuGestartet",
                table: "Ticket",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "KategorieID",
                table: "Ticket",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MitarbeiterID",
                table: "Ticket",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Prioritaet",
                table: "Ticket",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Ticket",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Rolle",
                table: "Mitarbeiter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_KategorieID",
                table: "Ticket",
                column: "KategorieID");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_MitarbeiterID",
                table: "Ticket",
                column: "MitarbeiterID");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Kategorie_KategorieID",
                table: "Ticket",
                column: "KategorieID",
                principalTable: "Kategorie",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Mitarbeiter_MitarbeiterID",
                table: "Ticket",
                column: "MitarbeiterID",
                principalTable: "Mitarbeiter",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Kategorie_KategorieID",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Mitarbeiter_MitarbeiterID",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_KategorieID",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_MitarbeiterID",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "HatNeuGestartet",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "KategorieID",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "MitarbeiterID",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "Prioritaet",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "Rolle",
                table: "Mitarbeiter");

            migrationBuilder.AlterColumn<string>(
                name: "Titel",
                table: "Ticket",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Beschreibung",
                table: "Ticket",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
