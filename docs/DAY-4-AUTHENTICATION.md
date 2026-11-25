# Day 4 — Registration, login, JWT and role policies

## Implemented
- ASP.NET Core Identity user with a display name.
- Public registration creates the `Student` role only.
- Password-based login with Identity lockout after repeated failed attempts.
- Short-lived signed JWT access tokens containing user identity and role claims.
- Opaque random refresh tokens; only SHA-256 hashes are stored in SQL Server.
- Refresh-token rotation and revocation. Refresh-token rows use SQL Server rowversion so concurrent reuse is rejected.
- Seeded `Student` and `Admin` roles.
- Authorization policies `StudentOrAdmin` and `AdminOnly`.
- An admin-only endpoint at `GET /api/admin/access` for checking role enforcement.

## Configure the signing key locally

The signing key is intentionally not committed. From the repository root, run:

```powershell
dotnet user-secrets set "Jwt:SigningKey" "replace-this-with-a-random-secret-at-least-32-bytes-long" --project src/StudyForge.API
```

Use a unique, randomly generated value for your local key. Never commit it or reuse a development key in production.

To create a local administrator account on first startup, optionally configure bootstrap credentials in User Secrets:

```powershell
dotnet user-secrets set "AdminBootstrap:Email" "admin@example.com" --project src/StudyForge.API
dotnet user-secrets set "AdminBootstrap:Password" "Choose-A-Strong-Admin-Password1" --project src/StudyForge.API
dotnet user-secrets set "AdminBootstrap:DisplayName" "StudyForge Admin" --project src/StudyForge.API
```

The seeder creates this account if it does not exist and assigns it the Admin role. If these values are not configured, only the Student and Admin roles are seeded. Never commit these credentials.

The issuer, audience, access-token lifetime, and refresh-token lifetime are in `src/StudyForge.API/appsettings.json`. The signing key must be supplied through User Secrets or a secure environment variable.

## Create and apply the database migration

The Identity tables and refresh-token table are introduced in this milestone. The Infrastructure project includes a design-time DbContext factory so EF tooling can create migrations without running API startup role seeding.

```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef migrations add AddIdentityAndRefreshTokens --project src/StudyForge.Infrastructure --startup-project src/StudyForge.API --output-dir Persistence/Migrations
dotnet ef database update --project src/StudyForge.Infrastructure --startup-project src/StudyForge.API
```

Do not start the API before applying the migration; Identity and refresh-token persistence require the database tables.

## Endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/auth/register` | Create a student account and return access/refresh tokens |
| POST | `/api/auth/login` | Validate credentials and issue tokens |
| POST | `/api/auth/refresh` | Rotate a refresh token and issue a new access token |
| POST | `/api/auth/logout` | Revoke the supplied refresh token |
| GET | `/api/admin/access` | Admin-only authorization check |

### Register request

```json
{
  "displayName": "Student One",
  "email": "student@example.com",
  "password": "StudyForge123"
}
```

### Login request

```json
{
  "email": "student@example.com",
  "password": "StudyForge123"
}
```

### Refresh and logout request

```json
{
  "refreshToken": "the-refresh-token-returned-by-register-or-login"
}
```

Send the access token on protected requests as `Authorization: Bearer <access-token>`.

## Role management

New public registrations are always assigned `Student`. The `Admin` role is created automatically, but no public registration endpoint can assign it. A first administrator can be provisioned through the optional User Secrets bootstrap described above. Never accept a role name from the public registration payload.

## Verification checklist

- [ ] Apply the EF Core migration successfully.
- [ ] Register a student and confirm the response includes a Student role.
- [ ] Log in with the correct password.
- [ ] Confirm an incorrect password returns 401.
- [ ] Call the admin endpoint with a Student token and confirm 403.
- [ ] Refresh a token and confirm the previous refresh token cannot be reused.
- [ ] Log out and confirm the revoked refresh token cannot be refreshed.
- [ ] Build the full solution and run automated tests.

## Security notes
- Passwords are stored and verified by ASP.NET Core Identity; the application never stores plaintext passwords.
- Refresh tokens are random 512-bit values; only their SHA-256 hashes are persisted.
- Refresh tokens are rotated on use and can be revoked at logout.
- SQL Server rowversion protects refresh-token rotation from concurrent reuse.
- JWT signing keys are not stored in source control.
- The access token is short-lived; refresh-token revocation does not retroactively invalidate an already-issued access token. It expires according to its JWT lifetime.
