# Dokumentacja projektu CleanIdentityMvc

## 1. Informacje ogólne

Projekt **CleanIdentityMvc** jest aplikacją webową wykonaną w technologii **ASP.NET Core MVC** z wykorzystaniem **Razor Views**, **Entity Framework Core**, **ASP.NET Core Identity**, **SQL Server** oraz dokumentacji API przez **Swagger/OpenAPI**.

Głównym celem projektu było przygotowanie systemu obsługi kont użytkowników. Aplikacja umożliwia rejestrację, logowanie, wylogowanie, przypominanie i resetowanie hasła za pomocą poczty elektronicznej, zmianę hasła, blokowanie konta po nieudanych próbach logowania, rejestrowanie aktywności użytkowników oraz podstawowe zarządzanie użytkownikami z poziomu panelu administratora.

Projekt został wykonany zgodnie z podejściem **Clean Architecture**, dlatego kod został podzielony na warstwy: `Core`, `UseCases`, `Infrastructure` oraz `Web`.

---

## 2. Cel projektu

Celem projektu było stworzenie aplikacji ASP.NET Core MVC spełniającej wymagania dotyczące:

- obsługi użytkowników z wykorzystaniem modułu ASP.NET Core Identity,
- rejestracji użytkownika,
- logowania i wylogowania,
- przypominania hasła z użyciem poczty elektronicznej,
- blokowania konta po błędnych próbach logowania,
- prowadzenia historii haseł,
- rejestrowania czasu logowania i wylogowania,
- rejestrowania aktywności użytkowników,
- użycia bazy danych SQL Server,
- przygotowania diagramów UML,
- przygotowania struktury bazy danych,
- wykorzystania systemu kontroli wersji Git.

---

## 3. Wykorzystane technologie

W projekcie użyto następujących technologii i narzędzi:

- ASP.NET Core MVC,
- Razor Views,
- Entity Framework Core,
- ASP.NET Core Identity,
- SQL Server,
- Swagger/OpenAPI,
- Clean Architecture,
- Git,
- Mailpit do lokalnego testowania poczty elektronicznej,
- PlantUML lub StarUML do przygotowania diagramów.

---

## 4. Struktura rozwiązania

Projekt został podzielony na kilka osobnych projektów zgodnie z zasadami Clean Architecture.

```text
src/
├── CleanIdentity.Core
├── CleanIdentity.UseCases
├── CleanIdentity.Infrastructure
└── CleanIdentity.Web
```

### 4.1. CleanIdentity.Core

Warstwa `Core` zawiera podstawowe encje domenowe wykorzystywane w systemie. Są to obiekty niezależne od szczegółów technicznych, takich jak baza danych, Identity czy interfejs użytkownika.

Przykładowe encje:

- `UserActivity` — aktywność użytkownika,
- `PasswordHistory` — historia haseł użytkownika,
- `LoginAuditLog` — audyt prób logowania,
- `AllowedIpAddress` — dozwolone adresy IP.

### 4.2. CleanIdentity.UseCases

Warstwa `UseCases` zawiera kontrakty i interfejsy wykorzystywane przez aplikację. Dzięki temu warstwa webowa nie musi znać szczegółów implementacji bazy danych, SMTP ani mechanizmów Identity.

Przykładowe interfejsy:

- `IAccountService`,
- `IActivityLogger`,
- `IActivityQueryService`,
- `IAuthAuditService`,
- `IPasswordHistoryService`,
- `IEmailSender`,
- `IUserPreferencesService`.

### 4.3. CleanIdentity.Infrastructure

Warstwa `Infrastructure` zawiera implementacje techniczne. Odpowiada za komunikację z bazą danych, konfigurację Identity, wysyłkę wiadomości e-mail, historię haseł, audyt logowania oraz rejestrowanie aktywności.

Znajdują się tutaj między innymi:

- `ApplicationDbContext`,
- `ApplicationUser`,
- `IdentityAccountService`,
- `PasswordHistoryService`,
- `ActivityLogger`,
- `ActivityQueryService`,
- `AuthAuditService`,
- `SmtpEmailSender`,
- `InfrastructureServiceExtensions`,
- `DatabaseSeeder`.

### 4.4. CleanIdentity.Web

Warstwa `Web` jest aplikacją ASP.NET Core MVC. Zawiera kontrolery, modele widoków, widoki Razor, konfigurację aplikacji, obsługę Swaggera oraz panel administratora.

Przykładowe elementy:

- `AccountController`,
- `AdminController`,
- `ActivityApiController`,
- widoki Razor dla logowania, rejestracji, resetu hasła i panelu administratora,
- pliki konfiguracyjne `appsettings.json` i `appsettings.Development.json`.

---

## 5. Architektura Clean Architecture

W projekcie zastosowałem podejście Clean Architecture, aby rozdzielić odpowiedzialności poszczególnych części aplikacji.

Główne założenia:

- warstwa `Core` nie zależy od żadnej innej warstwy,
- warstwa `UseCases` opisuje operacje możliwe do wykonania w systemie,
- warstwa `Infrastructure` implementuje dostęp do bazy danych, Identity, e-mail i inne szczegóły techniczne,
- warstwa `Web` odpowiada za interfejs użytkownika i obsługę żądań HTTP.

Dzięki takiemu podziałowi kod jest bardziej uporządkowany, łatwiejszy w utrzymaniu i lepiej oddziela logikę aplikacji od szczegółów technicznych.

---

## 6. Moduł Identity

Aplikacja wykorzystuje **ASP.NET Core Identity** do obsługi użytkowników, ról, haseł, tokenów resetowania hasła oraz blokady konta.

Identity odpowiada za:

- przechowywanie użytkowników w tabeli `AspNetUsers`,
- obsługę ról w tabelach `AspNetRoles` i `AspNetUserRoles`,
- hashowanie haseł,
- walidację haseł,
- obsługę błędnych prób logowania,
- czasową blokadę konta,
- generowanie tokenów resetowania hasła,
- obsługę sesji logowania.

Projekt rozbudowuje standardowe funkcje Identity o dodatkowe mechanizmy:

- historię ostatnich haseł,
- minimalny i maksymalny wiek hasła,
- audyt logowania,
- tabelę aktywności użytkownika,
- panel administratora,
- zarządzanie dozwolonymi adresami IP.

---

## 7. Role użytkowników

W aplikacji występują dwie podstawowe role:

```text
Admin
User
```

### 7.1. Rola User

Rola `User` jest domyślną rolą zwykłego użytkownika. Użytkownik z tą rolą może:

- logować się do aplikacji,
- wylogować się,
- zmienić hasło,
- przeglądać własną historię aktywności,
- ustawić, czy po logowaniu ma być pokazywana tabela aktywności.

### 7.2. Rola Admin

Rola `Admin` daje dostęp do panelu administratora. Administrator może:

- przeglądać listę użytkowników,
- sprawdzać role użytkowników,
- blokować i odblokowywać konta,
- nadawać i odbierać rolę administratora,
- zarządzać dozwolonymi adresami IP,
- przeglądać ostatnie aktywności użytkowników.

Po odebraniu użytkownikowi roli `Admin` system powinien przypisać mu domyślną rolę `User`, aby konto nie pozostało bez roli.

---

## 8. Rejestracja użytkownika

Proces rejestracji umożliwia utworzenie nowego konta w systemie.

Przebieg procesu:

1. Użytkownik przechodzi do formularza rejestracji.
2. Podaje dane konta, takie jak e-mail, imię, nazwisko i hasło.
3. System sprawdza poprawność danych.
4. Identity waliduje hasło zgodnie z ustawionymi zasadami bezpieczeństwa.
5. Po poprawnej walidacji użytkownik zostaje zapisany w tabeli `AspNetUsers`.
6. Użytkownik otrzymuje domyślną rolę `User`.
7. System zapisuje aktywność `REGISTER` w tabeli `UserActivities`.
8. System zapisuje aktualne hasło w tabeli historii haseł.

---

## 9. Logowanie użytkownika

Proces logowania wykorzystuje mechanizmy ASP.NET Core Identity.

Przebieg procesu:

1. Użytkownik przechodzi do formularza logowania.
2. Wpisuje e-mail oraz hasło.
3. System sprawdza, czy użytkownik istnieje.
4. System sprawdza, czy konto nie jest zablokowane.
5. System sprawdza poprawność hasła.
6. W przypadku błędnego hasła zwiększany jest licznik błędnych prób logowania.
7. Po przekroczeniu limitu konto zostaje zablokowane.
8. W przypadku poprawnego hasła użytkownik zostaje zalogowany.
9. System zapisuje wpis `LOGIN` w tabeli aktywności.
10. System zapisuje audyt logowania w tabeli `LoginAuditLogs`.
11. Jeśli użytkownik ma włączone pokazywanie aktywności po logowaniu, zostaje przekierowany do strony „Moje czynności”.

---

## 10. Wylogowanie użytkownika

Proces wylogowania kończy sesję użytkownika.

Przebieg procesu:

1. Użytkownik klika przycisk wylogowania.
2. System zapisuje aktywność `LOGOUT`.
3. System zapisuje czas wylogowania.
4. Sesja użytkownika zostaje zakończona.
5. Użytkownik zostaje przekierowany na stronę główną lub do formularza logowania.

---

## 11. Przypominanie i resetowanie hasła

Aplikacja obsługuje przypominanie hasła z wykorzystaniem poczty elektronicznej.

Przebieg procesu:

1. Użytkownik przechodzi do strony logowania.
2. Klika link „Nie pamiętasz hasła?”.
3. Wpisuje swój adres e-mail.
4. System sprawdza, czy konto istnieje.
5. System generuje token resetowania hasła.
6. System tworzy link resetujący hasło.
7. Link zostaje wysłany na adres e-mail użytkownika.
8. Użytkownik otwiera link z wiadomości.
9. Użytkownik wpisuje nowe hasło.
10. System sprawdza wymagania złożoności hasła.
11. System sprawdza historię ostatnich haseł.
12. Jeśli hasło jest poprawne i nie było używane wcześniej, zostaje ustawione jako nowe hasło.
13. System zapisuje nowy hash hasła w historii haseł.

Do testowania wysyłki e-mail lokalnie można wykorzystać Mailpit.

Przykładowa konfiguracja SMTP w `appsettings.Development.json`:

```json
{
  "Smtp": {
    "Host": "localhost",
    "Port": 1025,
    "EnableSsl": false,
    "UserName": "",
    "Password": "",
    "From": "noreply@cleanidentity.local"
  }
}
```

Uruchomienie Mailpit przez Dockera:

```bash
docker run --rm -p 1025:1025 -p 8025:8025 axllent/mailpit
```

Panel Mailpit jest dostępny pod adresem:

```text
http://localhost:8025
```

---

## 12. Zasady haseł

W aplikacji skonfigurowano zasady bezpieczeństwa haseł.

Hasło musi spełniać następujące wymagania:

- minimum 12 znaków,
- co najmniej jedna mała litera,
- co najmniej jedna wielka litera,
- co najmniej jedna cyfra,
- co najmniej jeden znak specjalny,
- wymagana liczba unikalnych znaków.

Dodatkowe zasady:

- maksymalny wiek hasła: 90 dni,
- minimalny wiek hasła: 1 dzień,
- historia ostatnich 20 haseł.

Konfiguracja znajduje się w pliku `appsettings.json`:

```json
"Security": {
  "PasswordHistoryLimit": 20,
  "PasswordMaxAgeDays": 90,
  "PasswordMinAgeDays": 1,
  "AllowedIpAddresses": []
}
```

Historia haseł jest przechowywana w tabeli `PasswordHistories`. Podczas zmiany lub resetowania hasła system porównuje nowe hasło z ostatnimi hasłami użytkownika. Dzięki temu użytkownik nie może ponownie użyć jednego z ostatnich 20 haseł.

---

## 13. Blokada konta

Aplikacja wykorzystuje mechanizm blokady konta dostępny w ASP.NET Core Identity.

Konfiguracja:

```text
Limit błędnych prób logowania: 10
Czas blokady konta: 15 minut
```

Po przekroczeniu limitu błędnych prób logowania konto zostaje automatycznie zablokowane na 15 minut.

Administrator może odblokować konto z poziomu panelu administratora. Odblokowanie polega na:

- wyczyszczeniu daty końca blokady,
- wyzerowaniu licznika błędnych prób logowania.

Dane związane z blokadą konta są przechowywane w tabeli `AspNetUsers`, między innymi w kolumnach:

- `AccessFailedCount`,
- `LockoutEnabled`,
- `LockoutEnd`.

---

## 14. Rejestrowanie aktywności użytkowników

Aplikacja zapisuje aktywności wykonywane przez użytkowników.

Przykładowe rejestrowane akcje:

- `REGISTER`,
- `LOGIN`,
- `LOGOUT`,
- `PASSWORD_CHANGE`,
- `PASSWORD_RESET`,
- `ACTIVITY_TABLE_ENABLED`,
- `ACTIVITY_TABLE_DISABLED`.

Każdy wpis aktywności zawiera:

- identyfikator użytkownika,
- nazwę akcji,
- szczegóły akcji,
- adres IP,
- informacje o przeglądarce,
- datę i czas wykonania akcji.

Aktywności są zapisywane w tabeli `UserActivities`.

---

## 15. Tabela „Moje czynności”

Po zalogowaniu użytkownik może przejść do strony „Moje czynności”. Strona wyświetla historię aktywności aktualnie zalogowanego użytkownika.

Użytkownik może również ustawić preferencję, czy tabela aktywności ma być pokazywana automatycznie po logowaniu.

Działanie przycisku:

- `Pokazuj po logowaniu` — ustawia automatyczne przekierowanie do tabeli aktywności po zalogowaniu,
- `Nie pokazuj po logowaniu` — wyłącza automatyczne pokazywanie tabeli po zalogowaniu.

Ważne: ta preferencja nie usuwa historii aktywności. Strona „Moje czynności” powinna zawsze pokazywać historię, jeśli użytkownik sam ją otworzy.

---

## 16. Panel administratora

Panel administratora jest dostępny dla użytkowników z rolą `Admin`.

Adres panelu:

```text
/Admin
```

Funkcje panelu administratora:

- wyświetlenie listy użytkowników,
- wyświetlenie ról użytkowników,
- blokowanie konta użytkownika,
- odblokowanie konta użytkownika,
- nadanie roli `Admin`,
- odebranie roli `Admin`,
- automatyczne przypisanie roli `User` po odebraniu roli `Admin`,
- zarządzanie dozwolonymi adresami IP,
- podgląd ostatnich aktywności użytkowników.

Dostęp do panelu jest ograniczony za pomocą atrybutu:

```csharp
[Authorize(Roles = "Admin")]
```

---

## 17. Dozwolone adresy IP

Projekt zawiera możliwość przechowywania dozwolonych adresów IP.

Adresy IP są zapisywane w tabeli `AllowedIpAddresses`. Administrator może nimi zarządzać z poziomu panelu administratora.

Tabela zawiera między innymi:

- identyfikator wpisu,
- wartość adresu IP,
- opis,
- informację, czy wpis jest aktywny.

W aplikacji można wykorzystać middleware sprawdzający, czy żądanie pochodzi z dozwolonego adresu IP.

---

## 18. Swagger/OpenAPI

W projekcie skonfigurowano Swagger/OpenAPI do dokumentowania endpointów API.

Swagger jest dostępny pod adresem:

```text
/swagger
```

W Swaggerze widoczne są endpointy API. Klasyczne akcje MVC zwracające widoki Razor nie muszą być widoczne w Swaggerze, ponieważ nie są typowymi endpointami API.

Przykładowe endpointy API:

- `GET /api/activity/me`,
- endpointy związane z kontem użytkownika, jeśli zostały dodane w kontrolerze API.

---

## 19. Baza danych

Aplikacja korzysta z bazy danych SQL Server oraz Entity Framework Core.

Connection string znajduje się w pliku `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CleanIdentityMvcDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 19.1. Tabele ASP.NET Core Identity

Standardowe tabele Identity:

- `AspNetUsers`,
- `AspNetRoles`,
- `AspNetUserRoles`,
- `AspNetUserClaims`,
- `AspNetUserLogins`,
- `AspNetUserTokens`,
- `AspNetRoleClaims`.

### 19.2. Własne tabele aplikacji

W projekcie występują również własne tabele:

- `UserActivities`,
- `PasswordHistories`,
- `LoginAuditLogs`,
- `AllowedIpAddresses`.

### 19.3. Opis wybranych tabel

#### AspNetUsers

Tabela przechowuje użytkowników systemu. Zawiera standardowe pola Identity oraz dodatkowe pola aplikacji, takie jak imię, nazwisko, data zmiany hasła czy preferencja pokazywania aktywności po logowaniu.

#### AspNetRoles

Tabela przechowuje role użytkowników, np. `Admin` i `User`.

#### AspNetUserRoles

Tabela łączy użytkowników z rolami.

#### UserActivities

Tabela przechowuje historię czynności użytkowników.

#### PasswordHistories

Tabela przechowuje hashe poprzednich haseł użytkownika.

#### LoginAuditLogs

Tabela przechowuje informacje o próbach logowania, w tym próbach udanych, nieudanych i zablokowanych.

#### AllowedIpAddresses

Tabela przechowuje dozwolone adresy IP.

---

## 20. Seeder bazy danych

Projekt zawiera seeder bazy danych, który przy starcie aplikacji może:

- zastosować migracje,
- utworzyć role `Admin` i `User`,
- utworzyć konto administratora,
- przypisać administratora do roli `Admin`,
- dodać przykładowe dozwolone adresy IP.

Dane administratora znajdują się w pliku `appsettings.json`:

```json
"Seed": {
  "AdminEmail": "admin@example.com",
  "AdminPassword": "Admin123!Admin"
}
```

Domyślne konto administratora:

```text
Login: admin@example.com
Hasło: Admin123!Admin
```

---

## 21. Migracje bazy danych

Migracje Entity Framework Core służą do tworzenia i aktualizacji struktury bazy danych.

Utworzenie migracji:

```bash
dotnet ef migrations add InitialCreate -p src/CleanIdentity.Infrastructure -s src/CleanIdentity.Web -o Data/Migrations
```

Aktualizacja bazy danych:

```bash
dotnet ef database update -p src/CleanIdentity.Infrastructure -s src/CleanIdentity.Web
```

Jeśli w projekcie użyto automatycznego stosowania migracji, aplikacja może wykonać migracje przy starcie przez `Database.MigrateAsync()`.

---

## 22. Konfiguracja lokalna

### 22.1. Uruchomienie aplikacji

Przywrócenie paczek:

```bash
dotnet restore
```

Zbudowanie projektu:

```bash
dotnet build
```

Uruchomienie projektu webowego:

```bash
dotnet run --project src/CleanIdentity.Web
```

### 22.2. Konfiguracja uruchamiania

Plik `launchSettings.json` można dodać w lokalizacji:

```text
src/CleanIdentity.Web/Properties/launchSettings.json
```

Przykładowa konfiguracja może uruchamiać automatycznie stronę główną lub Swaggera.

---

## 23. Diagram przypadków użycia

Do projektu przygotowano diagram przypadków użycia. Diagram przedstawia aktorów systemu oraz dostępne funkcje.

Aktorzy:

- Gość,
- Użytkownik,
- Administrator,
- Serwer SMTP.

Najważniejsze przypadki użycia:

- rejestracja konta,
- logowanie,
- wylogowanie,
- przypomnienie hasła,
- reset hasła,
- zmiana hasła,
- przeglądanie własnych czynności,
- zarządzanie użytkownikami,
- blokowanie i odblokowanie kont,
- nadawanie i odbieranie roli administratora,
- zarządzanie dozwolonymi adresami IP.

Relacje `include` oznaczają czynności wykonywane zawsze jako część innego przypadku użycia. Relacje `extend` oznaczają zachowanie opcjonalne lub warunkowe.

---

## 24. Diagram czynności

Diagram czynności przedstawia proces logowania użytkownika.

Proces obejmuje:

- otwarcie formularza logowania,
- wpisanie e-maila i hasła,
- sprawdzenie istnienia konta,
- sprawdzenie dozwolonego adresu IP,
- sprawdzenie blokady konta,
- weryfikację hasła,
- zwiększenie licznika błędnych logowań przy błędnym haśle,
- zablokowanie konta po przekroczeniu limitu,
- zapis udanego logowania,
- przekierowanie użytkownika do strony głównej lub do tabeli aktywności.

---

## 25. Scenariusze testowe

### 25.1. Rejestracja użytkownika

1. Otwieram stronę rejestracji.
2. Wpisuję dane użytkownika.
3. Podaję hasło spełniające wymagania.
4. Zatwierdzam formularz.
5. Sprawdzam, czy użytkownik został zapisany w bazie.
6. Sprawdzam, czy użytkownik otrzymał rolę `User`.

### 25.2. Logowanie poprawne

1. Otwieram stronę logowania.
2. Wpisuję poprawny e-mail i hasło.
3. Zatwierdzam formularz.
4. Sprawdzam, czy użytkownik został zalogowany.
5. Sprawdzam, czy w tabeli `UserActivities` pojawił się wpis `LOGIN`.

### 25.3. Logowanie błędne i blokada konta

1. Otwieram stronę logowania.
2. Wpisuję poprawny e-mail i błędne hasło.
3. Powtarzam próbę logowania do przekroczenia limitu.
4. Sprawdzam, czy konto zostało zablokowane.
5. Sprawdzam wartości `AccessFailedCount` i `LockoutEnd` w tabeli `AspNetUsers`.

### 25.4. Reset hasła

1. Otwieram stronę logowania.
2. Klikam „Nie pamiętasz hasła?”.
3. Podaję e-mail użytkownika.
4. Sprawdzam wiadomość w Mailpit.
5. Klikam link resetujący.
6. Wpisuję nowe hasło.
7. Loguję się nowym hasłem.

### 25.5. Historia haseł

1. Zmieniam hasło użytkownika.
2. Próbuję ustawić jedno z poprzednich haseł.
3. Sprawdzam, czy system blokuje ponowne użycie hasła.
4. Sprawdzam wpisy w tabeli `PasswordHistories`.

### 25.6. Panel administratora

1. Loguję się jako administrator.
2. Przechodzę do `/Admin`.
3. Sprawdzam listę użytkowników.
4. Nadaję rolę `Admin` wybranemu użytkownikowi.
5. Odbieram rolę `Admin` i sprawdzam, czy użytkownik otrzymał rolę `User`.
6. Blokuję i odblokowuję konto użytkownika.

---

## 26. Git i repozytorium

Projekt był rozwijany z wykorzystaniem systemu kontroli wersji Git.

Przykładowe komendy:

```bash
git init
git add .
git commit -m "Initial project version"
git branch -M main
git remote add origin <adres-repozytorium>
git push -u origin main
```

Do repozytorium powinny zostać dodane:

- kod źródłowy,
- pliki konfiguracyjne bez danych wrażliwych,
- migracje bazy danych,
- diagramy UML,
- opis projektu,
- dokumentacja projektu.

---

## 27. Bezpieczeństwo

W projekcie zastosowano kilka mechanizmów bezpieczeństwa:

- hashowanie haseł przez ASP.NET Core Identity,
- wymagania złożoności hasła,
- historia ostatnich haseł,
- minimalny i maksymalny wiek hasła,
- blokada konta po błędnych próbach logowania,
- role użytkowników,
- ograniczenie panelu administratora tylko do roli `Admin`,
- tokeny resetowania hasła,
- ochrona formularzy przez token anty-CSRF,
- rejestrowanie prób logowania i aktywności użytkowników.

---

## 28. Podsumowanie

Projekt spełnia wymagania dotyczące aplikacji ASP.NET Core MVC z wykorzystaniem Entity Framework Core, SQL Server oraz ASP.NET Core Identity. Aplikacja obsługuje rejestrację, logowanie, wylogowanie, resetowanie hasła przez e-mail, blokadę konta, historię haseł, rejestrowanie aktywności użytkowników, panel administratora, role użytkowników oraz dokumentację API przez Swagger.

Dzięki podziałowi na warstwy zgodnie z Clean Architecture projekt ma uporządkowaną strukturę i oddziela logikę domenową od szczegółów technicznych oraz interfejsu użytkownika.

Projekt znajduję się na repozytorium GitHub pod adresem https://github.com/LuckyHomie/CleanIdentityMvc
