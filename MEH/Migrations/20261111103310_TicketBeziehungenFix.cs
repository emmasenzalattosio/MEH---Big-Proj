using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEH.Migrations
{
    /// <inheritdoc />
    public partial class TicketBeziehungenFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Kategorie_KategorieID",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Mitarbeiter_MitarbeiterID",
                table: "Ticket");

            migrationBuilder.AlterColumn<int>(
                name: "MitarbeiterID",
                table: "Ticket",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Vorname",
                table: "Mitarbeiter",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nachname",
                table: "Mitarbeiter",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Kategorie",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Beschreibung",
                table: "Kategorie",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Kategorie_KategorieID",
                table: "Ticket",
                column: "KategorieID",
                principalTable: "Kategorie",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Mitarbeiter_MitarbeiterID",
                table: "Ticket",
                column: "MitarbeiterID",
                principalTable: "Mitarbeiter",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.AlterColumn<int>(
                name: "MitarbeiterID",
                table: "Ticket",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Vorname",
                table: "Mitarbeiter",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Nachname",
                table: "Mitarbeiter",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Kategorie",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Beschreibung",
                table: "Kategorie",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

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
    }
}
