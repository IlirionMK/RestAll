# RestAll — System Zarządzania Restauracją

RestAll to wieloplatformowy system do zarządzania restauracją, obejmujący obsługę zamówień, rezerwacji, kuchni i analityki. System składa się z backendu API, aplikacji webowej, klienta desktopowego oraz aplikacji mobilnej.

## Stos technologiczny

| Warstwa | Technologia |
| --- | --- |
| Backend API | Laravel 13 (PHP 8.4) |
| Aplikacja webowa | Vue 3 + TypeScript (Vite) |
| Aplikacja desktopowa | C# / .NET 10 (WPF) |
| Aplikacja mobilna | Flutter |
| Baza danych | PostgreSQL 18 |
| Cache & Queue | Redis 7 |
| Real-time | Laravel Reverb (WebSocket) |
| Konteneryzacja | Docker + Docker Compose |

## Architektura

```
RestAll/
├── backend/      # Laravel API (porty 8000 / 8080 Reverb)
├── web/          # Vue 3 SPA (port 5173)
├── desktop/      # WPF .NET 10
├── mobile/       # Flutter
└── docker/       # Konfiguracja Nginx, PHP
```

## Główne funkcjonalności

- Zarządzanie zamówieniami i pozycjami menu
- System rezerwacji stolików
- Panel kuchni z aktualizacjami w czasie rzeczywistym (Reverb)
- Analityka sprzedaży
- Zarządzanie personelem (RBAC)
- Uwierzytelnianie sesyjne (web) i tokenowe (desktop)
- Uwierzytelnianie dwuskładnikowe (2FA)

## Instrukcja uruchomienia

### Wymagania

- Docker i Docker Compose
- (desktop) .NET 10 SDK
- (mobile) Flutter SDK

### Backend + aplikacja webowa

```bash
# 1. Skopiuj plik konfiguracyjny backendu
cp backend/.env.example backend/.env

# 2. Uruchom kontenery
docker compose up -d

# 3. Zainstaluj zależności i przygotuj bazę danych
docker compose exec php composer install
docker compose exec php php artisan key:generate
docker compose exec php php artisan migrate --seed

# 4. Zainstaluj zależności frontendu (jeśli nie przez Docker)
cd web && npm install
```

Dostępne adresy po uruchomieniu:

| Serwis | Adres |
| --- | --- |
| API | http://localhost:8000/api |
| Aplikacja webowa | http://localhost:5173 |
| Reverb WebSocket | ws://localhost:8080 |
| Mailpit (e-mail dev) | http://localhost:8025 |
| PostgreSQL | localhost:5433 |

### Aplikacja desktopowa (.NET / WPF)

```bash
cd desktop/src/RestAll.Desktop.App
dotnet run
```

Konfiguracja połączenia znajduje się w `desktop/src/RestAll.Desktop.App/appsettings.json`.

### Aplikacja mobilna (Flutter)

```bash
cd mobile
flutter pub get
flutter run
```

### Testy backendu

```bash
docker compose exec php php artisan test
```

## Bezpieczeństwo

- Laravel Fortify & Sanctum (sesje webowe + tokeny API)
- Uwierzytelnianie dwuskładnikowe (2FA / TOTP)
- Kontrola dostępu oparta na rolach (RBAC): `admin`, `waiter`, `chef`, `cashier`
- CSRF protection, rate limiting
