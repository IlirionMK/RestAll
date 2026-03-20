# RestAll - System Zarządzania Restauracją

RestAll to system do zarządzania restauracją. 

## Stos technologiczny
- **Backend:** Laravel 13 (PHP 8.4)
- **Frontend:** Vue 3 (Vite + TypeScript)
- **Baza danych:** PostgreSQL
- **Komunikacja w czasie rzeczywistym:** Laravel Reverb 
- **Cache & Queue:** Redis
- **Konteneryzacja:** Docker 

## Główne cechy bezpieczeństwa
- Laravel Fortify & Sanctum.
- Uwierzytelnianie dwuskładnikowe (2FA).
- Zarządzanie uprawnieniami (RBAC).

## Instrukcja uruchomienia środowiska
1. Skopiuj pliki `.env` dla backendu i frontendu.
2. Uruchom kontenery:
   ```bash
   docker compose up -d
