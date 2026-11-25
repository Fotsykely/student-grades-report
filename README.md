# student-grades-report

Application de **notes et de moyennes** pour un enseignant : il saisit les notes de sa classe et obtient un bulletin avec la moyenne pondérée, la mention et le rang de chaque étudiant, ainsi que la moyenne de la classe pour chaque matière.

Construite à partir du template [next-dotnet-template](https://github.com/Fotsykely/next-dotnet-template) : **Next.js 16** (front), **ASP.NET Core / .NET 10** (API) et **MySQL 8.4**.

- **Étudiants** : Hery, Voahangy, Tiana, Mamy, Lalaina, Fanja.
- **Matières (coefficient)** : Mathématiques (3), Informatique (3), Anglais (2), Gestion (1).

Ces deux listes sont des données de référence fixes, insérées par la migration : il n'y a pas d'écran pour les gérer.

## Pages

Un menu en en-tête donne accès à chaque page.

| Page | Route | Rôle |
|---|---|---|
| Bulletin | `/` | notes de chaque étudiant, moyenne pondérée, mention, rang, et moyenne de la classe par matière |
| Notes | `/grades` | liste des notes, filtre par matière (`?subjectId=`), modification et suppression |
| Saisir une note | `/grades/new` | étudiant, matière et note sur 20 (décimales autorisées) |
| Modifier une note | `/grades/{id}/edit` | changement de la valeur de la note |

## Règles de gestion

**Une seule note par étudiant et par matière : un doublon est refusé.**
Saisir une seconde note pour le même couple étudiant / matière renvoie une erreur affichée sous le champ « Matière » (« Une note existe déjà pour cet étudiant dans cette matière. Modifiez-la depuis la liste des notes. »). La note existante n'est pas touchée. Pourquoi refuser plutôt que remplacer : un remplacement silencieux écraserait une note sans que l'enseignant s'en aperçoive (une faute de frappe sur l'étudiant suffirait). Pour corriger une note, on utilise « Modifier » dans la liste. La règle est aussi garantie par un index unique en base (`StudentId`, `SubjectId`), y compris si deux requêtes arrivent en même temps.

**Modification.** Seule la valeur d'une note se modifie ; l'étudiant et la matière sont affichés en lecture seule. Pour changer de couple, on supprime la note et on en saisit une autre. Cela évite de créer un doublon par la modification.

**Saisie d'une note.** Entre 0 et 20 inclus, 2 décimales au plus. La virgule et le point sont acceptés (« 12,5 » ou « 12.5 »). Un texte qui n'est pas un nombre, une note hors de l'intervalle ou un champ vide sont refusés avec un message sous le champ.

**Moyenne pondérée.** `Σ (note × coefficient) / Σ coefficients` (la somme des coefficients vaut 9), arrondie à 2 décimales (0,005 arrondi vers le haut). Exemple : 12, 15, 10 et 8 donnent (36 + 45 + 20 + 8) / 9 = **12,11**.

**Mentions.** Elles s'appliquent à la moyenne **arrondie**, celle qui est affichée.

| Moyenne | Mention |
|---|---|
| moins de 10 | Ajourné |
| 10 à moins de 12 | Passable |
| 12 à moins de 14 | Assez bien |
| 14 à moins de 16 | Bien |
| 16 et plus | Très bien |

Une moyenne de 11,996 est arrondie à 12,00 : elle vaut donc « Assez bien ».

**Étudiant incomplet.** S'il manque au moins une note, il n'a **ni moyenne, ni mention, ni rang** : la colonne « Moyenne » affiche « Incomplet » (en toutes lettres, pas seulement une couleur), les cases de notes manquantes affichent « — », et il n'est pas classé. Il apparaît sous les étudiants classés, par ordre alphabétique. Il ne décale pas le rang des autres.

**Rang.** Classement « olympique » : à moyennes égales, les étudiants partagent le même rang et le rang suivant est sauté (1er, 2e ex æquo, 2e ex æquo, 4e).

**Moyenne de la classe par matière.** Moyenne des notes qui existent pour la matière : les notes manquantes sont ignorées, pas comptées comme des zéros. Le nombre de notes prises en compte est affiché (« 12,25 — 2 notes »). Sans aucune note, la case affiche « — ».

Le bulletin est **calculé à la demande** à partir des notes : aucune moyenne, mention ou rang n'est stocké, donc rien ne peut devenir périmé après une modification ou une suppression.

## Lancer le projet

Prérequis : .NET SDK 10 (voir `global.json`), Node.js 20.9 ou plus, Docker. Trois terminaux.

**1. Base de données** (à la racine) :

```bash
cp .env.example .env        # ajustez le mot de passe et le port si besoin
docker compose up -d
```

**2. API** :

```bash
cd backend/Api
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Port=3307;Database=grades;User=grades;Password=change-me"
dotnet run                  # http://localhost:5080
```

Au démarrage, l'API applique les migrations, qui créent les tables et insèrent les étudiants et les matières. La chaîne de connexion n'est jamais dans le code : elle vient des *user secrets* en développement, ou de la variable d'environnement `ConnectionStrings__Default` ailleurs. Le port et le mot de passe doivent correspondre à votre `.env`.

> Au tout premier démarrage, le journal affiche un `fail: … SELECT "MigrationId" … FROM "__EFMigrationsHistory"`. C'est normal : EF lit la table d'historique avant qu'elle existe, puis applique la migration.

**3. Front** :

```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev                 # http://localhost:3000
```

Documentation interactive de l'API (en développement) : <http://localhost:5080/scalar>.

## Tests

```bash
cd backend && dotnet test                  # Docker doit tourner : Testcontainers lance un MySQL jetable
cd frontend && npm run lint && npm test    # ESLint, puis Vitest
cd frontend && npm run build               # API_BASE_URL doit être définie (voir .env.local) et l'API doit tourner
```

- `ReportCalculatorTests` (sans base) : moyenne pondérée, bornes des mentions, étudiant incomplet, ex æquo, arrondi avant mention, moyennes de classe.
- `GradesEndpointsTests` et `ReportEndpointsTests` (MySQL réel) : validation des notes, doublon refusé, filtre par matière, modification, suppression, bulletin de bout en bout.
- Vitest : client d'API, formats d'affichage, lecture des décimales saisies.

Les tests de l'API utilisent un vrai MySQL et non SQLite ou le fournisseur en mémoire : les migrations sont propres au fournisseur MySQL.

## Architecture

```
Navigateur ──► Next.js (:3000) ──► API ASP.NET Core (:5080) ──► MySQL (:3307)
```

Seul le serveur Next.js parle à l'API : pas de CORS à configurer, et l'URL de l'API (`API_BASE_URL`, sans préfixe `NEXT_PUBLIC_`) n'arrive jamais dans le navigateur.

```
backend/
  Api/
    Entities/           Student, Subject, Grade
    Reporting/          ReportCalculator et MentionRules : calcul pur, sans base ni HTTP
    Controllers/        grades, report, étudiants et matières
    Dtos/               requêtes (avec validation) et réponses
    Data/               DbContext (index unique, données de référence)
    Migrations/
  Api.Tests/            xUnit + WebApplicationFactory + Testcontainers
frontend/
  lib/api-client/       client HTTP (server-only), un fichier par ressource, types des DTOs
  lib/                  formats d'affichage, lecture des formulaires
  components/           menu, formulaire de note, composants génériques
  app/                  pages et Server Actions
```

### Points d'entrée de l'API

| Méthode | Route | Rôle |
|---|---|---|
| GET | `/api/students` | liste des étudiants |
| GET | `/api/subjects` | liste des matières et coefficients |
| GET | `/api/grades?subjectId=` | liste des notes, filtre optionnel par matière |
| GET | `/api/grades/{id}` | détail (404 si inconnu) |
| POST | `/api/grades` | création (400 + erreurs par champ : valeur invalide, étudiant ou matière inconnus, doublon) |
| PUT | `/api/grades/{id}` | modification de la valeur |
| DELETE | `/api/grades/{id}` | suppression (204, ou 404) |
| GET | `/api/report` | bulletin et moyennes de classe |
| GET | `/health` | santé de l'API et de la base |

Les erreurs non gérées et les statuts nus (404, 405…) répondent en `ProblemDetails` (JSON).

## Pièges Next.js 16 déjà rencontrés

`next.config.ts` active `cacheComponents` et `partialPrefetching`. Conséquences :

- **Toute lecture de l'API (et de `params` ou `searchParams`) doit être sous une frontière `<Suspense>`.** Chaque page affiche son titre tout de suite et lit les données dans un composant enfant, dans son propre `<Suspense>`.
- `params` et `searchParams` sont des promesses.
- `error.tsx` reçoit la prop `retry` (et non `reset`).
- `redirect()` fonctionne en levant une exception : appelez-le **hors** des `try/catch`.
- Une page « introuvable » (identifiant inconnu) répond en HTTP 200, car la page est streamée avant que l'API réponde. C'est une limite connue du streaming.
- `next build` appelle l'API pendant le rendu : `API_BASE_URL` doit être définie et l'API doit répondre.
- Le fichier `frontend/AGENTS.md` est généré par `next dev` : gardez-le versionné.

## Choix et limites

- Pas d'authentification : n'importe qui ayant accès à l'application peut saisir des notes. Pas de CORS (inutile tant que seul le serveur Next appelle l'API).
- Étudiants et matières fixes : en ajouter demande une migration.
- Après une suppression, la liste revient sur toutes les matières (le filtre n'est pas conservé).
- Pas de CI ni de Dockerfiles : Docker ne sert qu'à MySQL.
- Fournisseur EF Core : le paquet officiel d'Oracle (`MySql.EntityFrameworkCore`), car Pomelo n'avait pas encore de version stable pour EF Core 10. Il fournit l'erreur MySQL 1062 (doublon) que l'API intercepte, voir `GradesController`.
- La base est passée de PostgreSQL à MySQL : si vous aviez un `.env` de l'ancienne version, recopiez `.env.example` (les variables sont désormais `MYSQL_*`). Les anciennes données PostgreSQL restent dans l'ancien volume Docker `grades-data`, elles ne sont pas migrées.
- Les types du front sont écrits à la main ; la réponse de l'API est typée par un cast `as T` à un seul endroit (`client.ts`), sans validation à l'exécution.
- Conventions : identifiants et commentaires en anglais, textes affichés à l'utilisateur en français, commits en [Conventional Commits](https://www.conventionalcommits.org/).
