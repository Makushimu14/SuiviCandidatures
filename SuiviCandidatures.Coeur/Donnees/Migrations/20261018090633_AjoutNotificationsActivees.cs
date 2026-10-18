using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiviCandidatures.Coeur.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class AjoutNotificationsActivees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationsActivees",
                table: "Parametres",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationsActivees",
                table: "Parametres");
        }
    }
}
