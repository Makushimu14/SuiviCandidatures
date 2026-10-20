# Suivi des candidatures

<img src="docs/icone.png" alt="Icône de l'application" width="96" />

Ce projet est ma première expérience en vibecoding et a été réalisé à l'aide d'une IA en seulement quelques heures.

## Description

Application de bureau pour suivre ses candidatures à la recherche d'un emploi : liste des candidatures, historique,
relances avec notifications, entreprises, contacts et statistiques. Les données restent **sur votre ordinateur** (SQLite),
sans compte ni synchronisation. Interface en français.

Développée avec .NET 10, MAUI et le modèle MVVM, sous licence MIT.

## Fonctionnalités

- **Candidatures** : liste avec recherche (titre, entreprise, référence ou lien d'une offre : utile pour savoir si l'on a déjà
  postulé à une offre), filtre par statut, tri et pagination ; création et modification ; historique des actions
  (relances, entretiens, changements de statut) ; offres liées (site, référence, lien) ; contacts liés.
- **Relances** : délai par défaut réglable (7 jours), délai propre à chaque candidature, relances désactivables. Les relances dues
  sont affichées sur la page d'accueil et signalées par une notification Windows au démarrage, puis toutes les 30 minutes.
- **Entreprises** : coordonnées, plusieurs adresses, contacts, candidatures liées.
- **Statistiques** : taux de réponse, délai moyen de réponse, taux d'entretien, répartitions par statut, type de contrat, type de
  candidature et site, candidatures par semaine.
- **Paramètres** : délai de relance, taille des pages, thème clair, sombre ou système, notifications.
- **Sauvegarde** : export et import de toutes les données au format JSON.

## Installer l'application (Windows)

1. Téléchargez l'archive `SuiviCandidatures-<version>-win-x64.zip` depuis la page **Releases** du dépôt.
2. Décompressez-la dans un dossier de votre choix.
3. Lancez `SuiviCandidatures.exe`.

L'exécutable n'est pas signé : Windows SmartScreen peut afficher un avertissement au premier lancement
(« Informations complémentaires », puis « Exécuter quand même »). Aucune installation ni droit d'administrateur n'est nécessaire.
Ne lancez pas l'application en tant qu'administrateur : les notifications Windows n'y sont pas prises en charge.

## Compiler depuis les sources

### Prérequis

- SDK .NET 10
- Workload MAUI : `dotnet workload install maui`
- Pour modifier le modèle de données : l'outil EF Core, `dotnet tool install --global dotnet-ef`

### Lancer l'application

```
dotnet run --project SuiviCandidatures.Presentation -f net10.0-windows10.0.19041.0
```

### Publier un exécutable autonome

```
dotnet publish SuiviCandidatures.Presentation -f net10.0-windows10.0.19041.0 -c Release -r win-x64 --self-contained -o publication/win-x64
```

### Publier une version sur GitHub

Poussez une étiquette de version : le workflow `Publication` compile l'application, crée l'archive Windows et la joint à une release.

```
git tag v1.0.0
git push origin v1.0.0
```

### Lancer les tests

```
dotnet test --project SuiviCandidatures.Tests/SuiviCandidatures.Tests.csproj
```

Les tests couvrent le calcul des relances et des statistiques, la correspondance des noms d'entreprise, les règles de validation,
les services (candidatures, entreprises, offres, contacts, rappels, paramètres) sur une base SQLite en mémoire, et l'export puis
l'import des données. Ils sont aussi exécutés par l'intégration continue.

### Modifier le modèle de données

Après une modification des entités, créez une migration depuis la racine du dépôt :

```
dotnet ef migrations add NomDeLaMigration --project SuiviCandidatures.Coeur --startup-project SuiviCandidatures.Coeur --output-dir Donnees/Migrations
```

Elle est appliquée automatiquement au démarrage de l'application.

## Données

La base de données se trouve dans `%LOCALAPPDATA%\SuiviCandidatures`. Les exports sont écrits dans
`Documents\SuiviCandidatures\Sauvegardes`. Pensez à exporter régulièrement vos données : elles ne sont stockées nulle part ailleurs.

## Organisation du code

- `SuiviCandidatures.Coeur` : modèle de données, accès à la base (EF Core), règles métier et services, sans dépendance à l'interface.
- `SuiviCandidatures.Presentation` : application MAUI (vues XAML, modèles de vue, navigation, notifications).
- `SuiviCandidatures.Tests` : tests unitaires du cœur (xUnit).

## État du projet et limites connues

- **Windows** : plateforme développée et testée.
- **macOS (Mac Catalyst)** : la cible est déclarée mais **n'est pas testée**, et les notifications système n'y sont pas implémentées.
  Le workflow d'intégration continue tente de la compiler à titre indicatif ; un échec de ce contrôle est attendu tant que la
  version d'Xcode exigée par le SDK .NET diffère de celle des machines de GitHub.
- **Rappels** : ils ne fonctionnent que lorsque l'application est ouverte. Rien n'est signalé application fermée.
- Une sauvegarde importée **remplace** toutes les données (une sauvegarde automatique est créée avant).

## Licence

MIT, voir le fichier `LICENSE`. Pensez à y renseigner votre nom en cas de modification.
