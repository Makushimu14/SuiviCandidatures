using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiviCandidatures.Coeur.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class Initiale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entreprises",
                columns: table => new
                {
                    Identifiant = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SiteWeb = table.Column<string>(type: "TEXT", nullable: true),
                    Secteur = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entreprises", x => x.Identifiant);
                });

            migrationBuilder.CreateTable(
                name: "Parametres",
                columns: table => new
                {
                    Identifiant = table.Column<int>(type: "INTEGER", nullable: false),
                    DelaiRelanceParDefautJours = table.Column<int>(type: "INTEGER", nullable: false),
                    TaillePage = table.Column<int>(type: "INTEGER", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametres", x => x.Identifiant);
                });

            migrationBuilder.CreateTable(
                name: "Adresses",
                columns: table => new
                {
                    Identifiant = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Adresse = table.Column<string>(type: "TEXT", nullable: true),
                    CodePostal = table.Column<string>(type: "TEXT", nullable: true),
                    Ville = table.Column<string>(type: "TEXT", nullable: true),
                    Mail = table.Column<string>(type: "TEXT", nullable: true),
                    Telephone = table.Column<string>(type: "TEXT", nullable: true),
                    IdentifiantEntreprise = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adresses", x => x.Identifiant);
                    table.ForeignKey(
                        name: "FK_Adresses_Entreprises_IdentifiantEntreprise",
                        column: x => x.IdentifiantEntreprise,
                        principalTable: "Entreprises",
                        principalColumn: "Identifiant",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Candidatures",
                columns: table => new
                {
                    Identifiant = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TitreOffre = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    DateCandidature = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Statut = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TypeCandidature = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TypeContrat = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Commentaires = table.Column<string>(type: "TEXT", nullable: true),
                    DelaiRelanceJours = table.Column<int>(type: "INTEGER", nullable: true),
                    RelancesDesactivees = table.Column<bool>(type: "INTEGER", nullable: false),
                    IdentifiantEntreprise = table.Column<int>(type: "INTEGER", nullable: false),
                    IdentifiantAdresse = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatures", x => x.Identifiant);
                    table.ForeignKey(
                        name: "FK_Candidatures_Adresses_IdentifiantAdresse",
                        column: x => x.IdentifiantAdresse,
                        principalTable: "Adresses",
                        principalColumn: "Identifiant",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Candidatures_Entreprises_IdentifiantEntreprise",
                        column: x => x.IdentifiantEntreprise,
                        principalTable: "Entreprises",
                        principalColumn: "Identifiant",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    Identifiant = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    AncienStatut = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    NouveauStatut = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IdentifiantCandidature = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.Identifiant);
                    table.ForeignKey(
                        name: "FK_Actions_Candidatures_IdentifiantCandidature",
                        column: x => x.IdentifiantCandidature,
                        principalTable: "Candidatures",
                        principalColumn: "Identifiant",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Identifiant = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    Mail = table.Column<string>(type: "TEXT", nullable: true),
                    Telephone = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    IdentifiantEntreprise = table.Column<int>(type: "INTEGER", nullable: false),
                    IdentifiantCandidature = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Identifiant);
                    table.ForeignKey(
                        name: "FK_Contacts_Candidatures_IdentifiantCandidature",
                        column: x => x.IdentifiantCandidature,
                        principalTable: "Candidatures",
                        principalColumn: "Identifiant",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Contacts_Entreprises_IdentifiantEntreprise",
                        column: x => x.IdentifiantEntreprise,
                        principalTable: "Entreprises",
                        principalColumn: "Identifiant",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Offres",
                columns: table => new
                {
                    Identifiant = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Lien = table.Column<string>(type: "TEXT", nullable: true),
                    NomSite = table.Column<string>(type: "TEXT", nullable: true),
                    Reference = table.Column<string>(type: "TEXT", nullable: true),
                    IdentifiantCandidature = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offres", x => x.Identifiant);
                    table.ForeignKey(
                        name: "FK_Offres_Candidatures_IdentifiantCandidature",
                        column: x => x.IdentifiantCandidature,
                        principalTable: "Candidatures",
                        principalColumn: "Identifiant",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actions_Date",
                table: "Actions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_IdentifiantCandidature",
                table: "Actions",
                column: "IdentifiantCandidature");

            migrationBuilder.CreateIndex(
                name: "IX_Adresses_IdentifiantEntreprise",
                table: "Adresses",
                column: "IdentifiantEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatures_DateCandidature",
                table: "Candidatures",
                column: "DateCandidature");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatures_IdentifiantAdresse",
                table: "Candidatures",
                column: "IdentifiantAdresse");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatures_IdentifiantEntreprise",
                table: "Candidatures",
                column: "IdentifiantEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatures_Statut",
                table: "Candidatures",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_IdentifiantCandidature",
                table: "Contacts",
                column: "IdentifiantCandidature");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_IdentifiantEntreprise",
                table: "Contacts",
                column: "IdentifiantEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_Entreprises_Nom",
                table: "Entreprises",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_Offres_IdentifiantCandidature",
                table: "Offres",
                column: "IdentifiantCandidature");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actions");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Offres");

            migrationBuilder.DropTable(
                name: "Parametres");

            migrationBuilder.DropTable(
                name: "Candidatures");

            migrationBuilder.DropTable(
                name: "Adresses");

            migrationBuilder.DropTable(
                name: "Entreprises");
        }
    }
}
