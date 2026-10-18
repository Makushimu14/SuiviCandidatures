using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiviCandidatures.Coeur.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class LiaisonContactsCandidatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Candidatures_IdentifiantCandidature",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_IdentifiantCandidature",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "IdentifiantCandidature",
                table: "Contacts");

            migrationBuilder.CreateTable(
                name: "ContactsCandidatures",
                columns: table => new
                {
                    CandidaturesIdentifiant = table.Column<int>(type: "INTEGER", nullable: false),
                    ContactsIdentifiant = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactsCandidatures", x => new { x.CandidaturesIdentifiant, x.ContactsIdentifiant });
                    table.ForeignKey(
                        name: "FK_ContactsCandidatures_Candidatures_CandidaturesIdentifiant",
                        column: x => x.CandidaturesIdentifiant,
                        principalTable: "Candidatures",
                        principalColumn: "Identifiant",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactsCandidatures_Contacts_ContactsIdentifiant",
                        column: x => x.ContactsIdentifiant,
                        principalTable: "Contacts",
                        principalColumn: "Identifiant",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactsCandidatures_ContactsIdentifiant",
                table: "ContactsCandidatures",
                column: "ContactsIdentifiant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactsCandidatures");

            migrationBuilder.AddColumn<int>(
                name: "IdentifiantCandidature",
                table: "Contacts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_IdentifiantCandidature",
                table: "Contacts",
                column: "IdentifiantCandidature");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Candidatures_IdentifiantCandidature",
                table: "Contacts",
                column: "IdentifiantCandidature",
                principalTable: "Candidatures",
                principalColumn: "Identifiant",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
