# Rise.Services.Tests - Test database setup (Docker Compose only)

Deze README beschrijft hoe je lokaal en in CI een geïsoleerde SQL Server gebruikt voor de integratietests in
`tests/Rise.Services.Tests` — uitsluitend via Docker Compose. We gebruiken alleen Docker en enkel de meegeleverde
`docker-compose.test.yml`.

Belangrijk: de tests vertrouwen uitsluitend op user‑secrets voor hun connection string. Gebruik géén environment
variables om de test‑connection in te stellen — maak altijd user‑secrets aan voor `tests/Rise.Services.Tests`.

Kort overzicht

- De test‑suite maakt gebruik van een xUnit fixture (`SqlServerTestDatabaseFixture`) die:
	- een unieke database aanmaakt per test‑run (naam `RiseTests_{GUID}`),
	- EF Core‑migraties uit `Rise.Persistence` toepast,
	- de database dropt na afloop.
- De fixture leest de connection template uit user‑secrets en vereist:
	1. `ConnectionStrings:TestDatabaseConnectionDocker` (verplicht).

Prerequisites

- .NET 9 SDK
- Docker Engine en Docker Compose (vereist — gebruik alleen de repository's Compose file)

Aanbevolen lokale workflow (Docker Compose + user‑secrets)

1. Zet het SA-wachtwoord en start de SQL Server test‑service met het meegeleverde Compose‑bestand (
   `docker-compose.test.yml`) uit de repository root. De compose-file vereist nu expliciet dat `SA_PASSWORD` is gezet
   voor veiligheidsredenen.

- macOS / Linux (zsh / bash):

```bash
# vanuit repo root: zet eerst SA_PASSWORD (vervang door een sterk wachtwoord)
export SA_PASSWORD="ChangeMeStrong!"
# start daarna de SQL Server test container
docker compose -f docker-compose.test.yml up -d
# Wacht tot de healthcheck klaar is en de container draait
```

- Windows PowerShell:

```powershell
$env:SA_PASSWORD = "ChangeMeStrong!"

docker compose -f docker-compose.test.yml up -d
```

2. Maak user‑secrets voor het tests‑project en zet daar de connection string in. Je moet
   `ConnectionStrings:TestDatabaseConnectionDocker` invullen. Zorg dat het wachtwoord in de connection string
   overeenkomt met de `SA_PASSWORD` die je aan de container doorgeeft.

User secrets voor `tests/Rise.Services.Tests` (verplicht)

- macOS / Linux (zsh / bash):

```bash
# ga naar het tests project
cd tests/Rise.Services.Tests
# initialiseert user-secrets (maakt UserSecretsId in csproj als dat nog niet bestaat)
dotnet user-secrets init
# optioneel: wis bestaande secrets
dotnet user-secrets clear
# verplicht: zet de ConnectionStrings key
dotnet user-secrets set "ConnectionStrings:TestDatabaseConnectionDocker" "Server=localhost,1434;Database=test_vmm;User Id=sa;Password=Your_password123;TrustServerCertificate=true;"
```

- Windows PowerShell:

```powershell
Set-Location tests/Rise.Services.Tests

dotnet user-secrets init

dotnet user-secrets clear

dotnet user-secrets set "ConnectionStrings:TestDatabaseConnectionDocker" "Server=localhost,1434;Database=test_vmm;User Id=sa;Password=Your_password123;TrustServerCertificate=true;"
```

Let op: vervang `Your_password123` door het wachtwoord dat jouw lokale Docker Compose gebruikt. Wij adviseren:

- pas je lokale copy van `docker-compose.test.yml` aan en zet daar een sterk wachtwoord OF gebruik de env `SA_PASSWORD`,
  en
- zet exact dat wachtwoord in je user‑secrets zodat de tests de juiste connectie kunnen maken.

Run de tests

```bash
# vanuit repo root
dotnet test tests/Rise.Services.Tests -v minimal
```

Wat doet de fixture precies

- Maakt verbinding met `master` op basis van de template connection string (uit user‑secrets) en voert:
	- `CREATE DATABASE [RiseTests_{GUID}]`
- Verbindt met de nieuwe DB en roept `context.Database.Migrate()` aan (migraties uit `Rise.Persistence`)
- Exposeert `ConnectionString` en helper `CreateDbContextOptions<TContext>()` voor tests
- Bij teardown zet het de database in SINGLE_USER met ROLLBACK IMMEDIATE en dropt de DB

Hoe je tests moet schrijven

- Markeer testklassen die DB nodig hebben met:

```csharp
[Collection("SqlServer")]
public class MyRepoTests {
    private readonly SqlServerTestDatabaseFixture _fixture;
    public MyRepoTests(SqlServerTestDatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Example() {
        var options = _fixture.CreateDbContextOptions<ApplicationDbContext>();
        await using var ctx = new ApplicationDbContext(options);
        // ... arrange / act / assert
    }
}
```

CI tips

- In CI zet je de ConnectionStrings key via de pipeline als secret en vul je die in user‑secrets vóór het draaien van de
  tests. Pseudocode voor een job‑stap:

```bash
# voorbeeld CI-steps (run in the workspace root or cd into tests/Rise.Services.Tests)
cd tests/Rise.Services.Tests
# init user-secrets if needed
dotnet user-secrets init || true
# gebruik je CI‑secret (bijv. TEST_DB_CONNECTION_VALUE) om de ConnectionStrings key te vullen
dotnet user-secrets set "ConnectionStrings:TestDatabaseConnectionDocker" "$TEST_DB_CONNECTION_VALUE"
# run tests
dotnet test ../../tests/Rise.Services.Tests -v minimal
```

- De fixture maakt een unieke DB per job, waardoor parallelle jobs niet conflicteren.

Veelvoorkomende problemen

- "No SQL Server connection template found" — zorg dat je user‑secrets voor `tests/Rise.Services.Tests` de
  sleutel `ConnectionStrings:TestDatabaseConnectionDocker` bevat.
- TLS/cert errors op macOS/CI — zorg dat `TrustServerCertificate=true` in de connectiestring staat.
- Pakketversies — als de build fouten geeft over pakketdowngrade, update `Microsoft.Data.SqlClient` naar minimaal
  `5.1.6`.
