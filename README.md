# SafeVault API

SafeVault is an ASP.NET Core Web API using ASP.NET Core Identity to register
users with a unique username, email address, and securely hashed password.

## Prerequisites

- .NET SDK 10.0 or later

## Run the API

From the solution directory:

```bash
dotnet run --project SafeVaultApi/SafeVaultApi.csproj
```

The API runs at `http://localhost:5157` by default. In the Development
environment, the interactive API reference is available at:

```text
http://localhost:5157/scalar/v1
```

The underlying OpenAPI document is available at
`http://localhost:5157/openapi/v1.json`.

### Configure JWT development secrets

The JWT signing key is intentionally not stored in source control. Before
running the API locally, configure a random signing key of at least 32
characters:

```bash
dotnet user-secrets set "Jwt:SigningKey" "<random-32-character-minimum-secret>" --project SafeVaultApi/SafeVaultApi.csproj
```

The default Admin account is also seeded from user secrets. Configure its
password before starting the API:

```bash
dotnet user-secrets set "SeedAdmin:Password" "<strong-admin-password>" --project SafeVaultApi/SafeVaultApi.csproj
```

## Register a user

Send a `POST` request to `/api/users/register`.

```http
POST http://localhost:5157/api/users/register
Content-Type: application/json

{
  "username": "harold.smith",
  "email": "harold.smith@example.com",
  "password": "Str0ng!Password"
}
```

### Validation rules

- `username` is required, must be 3–50 characters, and may contain only
  letters, numbers, periods, underscores, and hyphens.
- `email` is required, must be a valid email address, and may be at most
  254 characters. Markup, whitespace, and query characters (such as `'`, `"`,
  and `;`) are rejected.
- `password` is required, must be 12–100 characters, and must include
  uppercase, lowercase, numeric, and non-alphanumeric characters. Passwords
  are stored only as ASP.NET Core Identity hashes.
- Usernames and email addresses must be unique without regard to casing.
- Invalid input is rejected with a validation error; the API does not alter
  submitted usernames or email addresses.

### Responses

| Status | Meaning |
| --- | --- |
| `201 Created` | The user was registered successfully. |
| `400 Bad Request` | The request payload did not meet validation rules. |
| `409 Conflict` | The username or email address is already registered. |
| `429 Too Many Requests` | More than five registration requests were sent within one minute. |

Registered users are stored in an EF Core in-memory database and are cleared
whenever the API restarts.

To prevent automated registration abuse, the registration endpoint accepts at
most five requests per minute.

## Roles

The application seeds these ASP.NET Core Identity roles at startup:

- `Admin` — intended for users who manage the application.
- `Users` — automatically assigned to every user who registers.

The startup seed creates `Admin` (`admin@safevaulttest.com`) and assigns it
the `Admin` role. Its password is read only from `SeedAdmin:Password` user
secrets or environment variables.

## Administration API

Only authenticated users in the `Admin` role can manage users and roles:

- `GET /api/admin/dashboard` returns total user, Admin-user, and role counts.
- `GET /api/admin/users` lists users and their roles.
- `DELETE /api/admin/users/{userId}` deletes a user other than the current
  administrator.
- `GET /api/admin/roles` lists roles.
- `POST /api/admin/roles` creates a custom role.
- `PUT /api/admin/users/{userId}/roles/{roleName}` assigns a role.
- `DELETE /api/admin/users/{userId}/roles/{roleName}` removes a role.
- `DELETE /api/admin/roles/{roleName}` deletes an unused custom role.

The default `Admin` and `Users` roles cannot be deleted, and the final Admin
account cannot be deleted or stripped of its `Admin` role.

## Security summary

### Vulnerabilities assessed

- **SQL injection:** Registration, authentication, and administration inputs
  were assessed for query-like payloads. The application does not use raw SQL
  or dynamically concatenated query strings.
- **Cross-site scripting (XSS):** User-provided username and email values were
  assessed for script and HTML-markup payloads.
- **Broken access control:** Protected profile, dashboard, and administration
  endpoints were assessed for requests without tokens and with insufficient
  roles.
- **Credential exposure:** JWT signing and seeded Admin password configuration
  were assessed to ensure secrets are not stored in source control.

### Fixes and controls

- EF Core LINQ and ASP.NET Core Identity data stores prevent SQL query
  construction from user input.
- Request-model validation uses allow-lists and rejects markup, control
  characters, and query-like characters in registration fields.
- ASP.NET Core Identity hashes passwords; passwords and password hashes are
  never returned in API responses.
- JWT bearer authentication protects authenticated endpoints. The `Admin`
  role is required for dashboard and user/role management APIs.
- The registration endpoint is rate limited to reduce automated abuse.
- JWT signing keys and the seeded Admin password are supplied through user
  secrets or environment variables.

### Verification and Copilot assistance

Copilot assisted with designing the API contracts, configuring Identity and
JWT authorization, implementing validation and authorization controls, and
creating test payloads for SQL injection, XSS, invalid-login, and role-access
scenarios. The test suite verifies that malicious registration payloads return
`400 Bad Request`, invalid credentials return `401 Unauthorized`, and a
standard `Users` account receives `403 Forbidden` when accessing Admin-only
endpoints.

## Test requests

Use [SafeVaultApi.http](./SafeVaultApi/SafeVaultApi.http) in Visual Studio Code
with an HTTP client extension, or use it as a reference for your own HTTP
requests.

## Authentication

### Log in

Send a `POST` request to `/api/auth/login` with a registered username and
password. A successful response contains a JWT access token that expires after
60 minutes.

### Authenticated endpoint

Use the access token as a bearer token to call `GET /api/users/me`:

```http
Authorization: Bearer <access-token>
```

The endpoint returns `401 Unauthorized` when the token is missing, expired, or
invalid.

## Run tests

```bash
dotnet test SafeVaultApi.Tests/SafeVaultApi.Tests.csproj
```
