# CleanIdentityMvc

Projekt przykładowy ASP.NET Core MVC + Razor Views + Identity + Entity Framework Core + SQL Server w układzie Clean Architecture:

- `CleanIdentity.Core` — encje domenowe bez zależności od EF i MVC.
- `CleanIdentity.UseCases` — kontrakty i modele wejścia/wyjścia przypadków użycia.
- `CleanIdentity.Infrastructure` — EF Core, SQL Server, ASP.NET Core Identity, SMTP, implementacje serwisów.
- `CleanIdentity.Web` — MVC, kontrolery, widoki Razor, Swagger UI/OpenAPI.

## Wymagania

- .NET SDK 10 LTS albo zmiana `TargetFramework` na `net8.0` w plikach `.csproj`.
- SQL Server / SQL Server LocalDB.
- Visual Studio 2026/2022 z workloadem ASP.NET albo CLI `dotnet`.

## Uruchomienie

1. Ustaw connection string w `src/CleanIdentity.Web/appsettings.json`.
2. Zainstaluj narzędzie migracji, jeśli go nie masz:

```bash
dotnet tool install --global dotnet-ef
```

3. Przywróć pakiety i utwórz bazę:

```bash
dotnet restore
dotnet ef migrations add InitialCreate -p src/CleanIdentity.Infrastructure -s src/CleanIdentity.Web -o Data/Migrations
dotnet ef database update -p src/CleanIdentity.Infrastructure -s src/CleanIdentity.Web
```

4. Uruchom aplikację:

```bash
dotnet run --project src/CleanIdentity.Web
```

5. Otwórz:

- MVC: `https://localhost:xxxx/`
- Swagger UI: `https://localhost:xxxx/swagger`
- OpenAPI JSON: `https://localhost:xxxx/openapi/v1.json`

## Funkcjonalności z wymagań

- Rejestracja użytkownika: `AccountController.Register`.
- Logowanie/wylogowanie: `AccountController.Login`, `AccountController.Logout`.
- Reset hasła przez e-mail: `ForgotPassword`, `ResetPassword`.
- Zliczanie niepoprawnych logowań i blokada konta: `IdentityOptions.Lockout` + `PasswordSignInAsync(..., lockoutOnFailure: true)`.
- Historia ostatnich haseł: tabela `PasswordHistories` i `PasswordHistoryService`.
- Minimalny i maksymalny wiek hasła: `ApplicationSecurityOptions` i `IdentityAccountService`.
- Rejestracja czasu logowania/wylogowania: `LoginAuditLogs`.
- Lista czynności zalogowanego użytkownika: `UserActivities`, widok `Account/Activity`, endpoint `api/activity/me`.
- Opcjonalna lista dozwolonych IP: `Security:AllowedIpAddresses` + `IpAllowListMiddleware`.

## Git

```bash
git init
git add .
git commit -m "Initial clean architecture identity project"
git branch -M main
git remote add origin https://github.com/<login>/<repo>.git
git push -u origin main
```

## Dokumentacja do WIKAMP

W katalogu `docs/` znajdują się gotowe materiały:

- `use-case-diagram.puml` — diagram przypadków użycia.
- `login-activity-diagram.puml` — diagram czynności logowania.
- `er-diagram.md` — diagram ER w Mermaid oraz opis tabel.
- `database-outline.sql` — szkic tabel niestandardowych do pokazania w dokumentacji.

PlantUML można przepisać do StarUML, a ER można odtworzyć w SSMS Database Diagrams po wykonaniu migracji.
