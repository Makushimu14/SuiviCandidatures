using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Tests.Outils;

namespace SuiviCandidatures.Tests;

public sealed class ServiceSuiviCandidatureTests : IDisposable
{
    private readonly BaseDeDonneesTest baseDeDonnees = new();
    private readonly ServiceSuiviCandidature service;

    public ServiceSuiviCandidatureTests()
    {
        service = new ServiceSuiviCandidature(baseDeDonnees);
    }

    public void Dispose() => baseDeDonnees.Dispose();

    [Fact]
    public async Task Le_detail_regroupe_les_actions_les_offres_et_les_contacts()
    {
        int identifiant = await baseDeDonnees.SemerCandidatureCompleteAsync();

        DetailCandidature? detail = await service.ObtenirDetailAsync(identifiant);

        Assert.NotNull(detail);
        Assert.Equal("Acme", detail.NomEntreprise);
        Assert.Equal(StatutCandidature.EnCoursEtude, detail.Statut);
        Assert.Equal("1 rue de la Paix, 75002 Paris", detail.LibelleAdresse);
        Assert.Single(detail.Offres);
        Assert.Equal("Martin", Assert.Single(detail.Contacts).Nom);
        Assert.Equal(3, detail.Actions.Count);
        Assert.True(detail.Actions[0].Date >= detail.Actions[^1].Date);
    }

    [Fact]
    public async Task Un_detail_inexistant_retourne_null()
    {
        Assert.Null(await service.ObtenirDetailAsync(999));
    }

    [Fact]
    public async Task Changer_vers_le_meme_statut_n_ajoute_rien_et_vers_un_autre_ajoute_une_action()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        await service.ChangerStatutAsync(identifiant, StatutCandidature.EnAttenteReponse);
        await service.ChangerStatutAsync(identifiant, StatutCandidature.EntretienPlanifie);

        await using ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext();
        ActionCandidature action = await contexte.Actions.SingleAsync(a => a.Type == TypeAction.ChangementStatut);
        Assert.Equal(StatutCandidature.EnAttenteReponse, action.AncienStatut);
        Assert.Equal(StatutCandidature.EntretienPlanifie, action.NouveauStatut);
    }

    [Fact]
    public async Task Une_action_manuelle_peut_etre_creee_modifiee_et_supprimee()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        await service.EnregistrerActionAsync(new DonneesAction
        {
            IdentifiantCandidature = identifiant,
            Type = TypeAction.Entretien,
            Date = new DateTime(2026, 10, 20, 14, 0, 0),
            Note = "  Visio  "
        });

        int identifiantAction;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            ActionCandidature action = await contexte.Actions.SingleAsync(a => a.Type == TypeAction.Entretien);
            identifiantAction = action.Identifiant;
            Assert.Equal("Visio", action.Note);
        }

        await service.EnregistrerActionAsync(new DonneesAction
        {
            Identifiant = identifiantAction,
            IdentifiantCandidature = identifiant,
            Type = TypeAction.RelanceParTelephone,
            Date = new DateTime(2026, 10, 21),
            Note = null
        });

        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            Assert.Equal(TypeAction.RelanceParTelephone, (await contexte.Actions.SingleAsync(a => a.Identifiant == identifiantAction)).Type);
        }

        await service.SupprimerActionAsync(identifiantAction);

        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            Assert.False(await contexte.Actions.AnyAsync(a => a.Identifiant == identifiantAction));
        }
    }

    [Fact]
    public async Task Une_action_autre_exige_une_note_et_un_type_automatique_est_refuse()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => service.EnregistrerActionAsync(
            new DonneesAction { IdentifiantCandidature = identifiant, Type = TypeAction.Autre, Note = "  " }));
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => service.EnregistrerActionAsync(
            new DonneesAction { IdentifiantCandidature = identifiant, Type = TypeAction.ChangementStatut }));
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => service.EnregistrerActionAsync(
            new DonneesAction { IdentifiantCandidature = identifiant, Type = TypeAction.CandidatureEnvoyee }));
    }

    [Fact]
    public async Task Une_action_automatique_ne_peut_etre_ni_modifiee_ni_supprimee()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");
        int identifiantAction;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            identifiantAction = await contexte.Actions.Select(a => a.Identifiant).SingleAsync();
        }

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => service.EnregistrerActionAsync(new DonneesAction
        {
            Identifiant = identifiantAction,
            IdentifiantCandidature = identifiant,
            Type = TypeAction.Autre,
            Note = "Tentative"
        }));
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => service.SupprimerActionAsync(identifiantAction));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(91)]
    [InlineData(-3)]
    public async Task Un_delai_de_relance_hors_bornes_est_refuse(int delai)
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            service.EnregistrerParametresRelanceAsync(identifiant, delai, false));
    }

    [Fact]
    public async Task Les_parametres_de_relance_sont_enregistres()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        await service.EnregistrerParametresRelanceAsync(identifiant, 12, true);

        DetailCandidature detail = (await service.ObtenirDetailAsync(identifiant))!;
        Assert.Equal(12, detail.DelaiRelanceJours);
        Assert.True(detail.RelancesDesactivees);
    }

    [Fact]
    public async Task Supprimer_une_candidature_supprime_son_historique_et_ses_offres_mais_garde_les_contacts()
    {
        int identifiant = await baseDeDonnees.SemerCandidatureCompleteAsync();

        await service.SupprimerAsync(identifiant);

        await using ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext();
        Assert.Equal(0, await contexte.Candidatures.CountAsync());
        Assert.Equal(0, await contexte.Actions.CountAsync());
        Assert.Equal(0, await contexte.Offres.CountAsync());
        Assert.Equal(1, await contexte.Contacts.CountAsync());
        Assert.Equal(1, await contexte.Entreprises.CountAsync());
    }
}
