# QuillingArtist API (.NET 10)

ASP.NET Core 10 minimal-API port of the Spring Boot backend in `../quillingartistv2`.
It reads and writes the same PostgreSQL database (`app_db`, schema `quillingartist`), and the
React UI calls it on the same routes. There are no EF migrations: the schema stays as it is.

## Layout

```
QuillingArtist.slnx
src/ApiService/
  Program.cs              DI, JSON options, /api route group, middleware
  Entities/               EF Core entities  (was entity/*.java, JPA)
  Data/AppDbContext.cs    DbContext + nextval() helper
  Dao/                    *Dao + *Filter    (was dao/*.java, dynamic JPQL)
  Services/               *Service, StripeCheckoutService, NightlyJob
  Services/Communications Brevo email/SMS over the REST API
  Endpoints/              Minimal-API route groups (was api/*Resource.java)
  Security/               AuthContext + JwtAuthMiddleware (was JwtAuthFilter)
  Dtos/  Utils/  Configuration/
```

## Configuration

Settings come from `appsettings.json`, and environment variables override them (use `__` for nesting).
Secrets are left empty on purpose. Supply them like this:

```sh
cd src/ApiService
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=....;Username=.....;Password=...;Search Path=quillingartist"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Email:Brevo:ApiKey" "xkeysib-..."
dotnet user-secrets set "Sms:Brevo:ApiKey" "xkeysib-..."
```

For Docker, use `ConnectionStrings__Default`, `Stripe__SecretKey`, `Email__Brevo__ApiKey`, `Sms__Brevo__ApiKey` and
`Jwt__SigningKey` (`run.sh` and `docker-compose.yml` read `DB_PASSWORD`, `STRIPE_SECRET_KEY`, `BREVO_API_KEY` and
`JWT_SIGNING_KEY` from the shell).

## Authentication

`selectAll` and `export` need `Authorization: Bearer <jwt>`. The token must be HS256-signed with `Jwt:SigningKey`
(at least 32 bytes), unexpired, issued by `Jwt:Issuer` for `Jwt:Audience`, and its `email` claim must match a row in
`quillingartist.users`. Development uses the dev key in `appsettings.Development.json`, the same key
`SyntheticTokenFactory.java` signs with. In any other environment the app won't start until `Jwt__SigningKey` is set.

```sh
cd test
python3 make_token.py            # print a 1-hour token for support@quillingartist.com
python3 make_token.py --update   # refresh the tokens in postman.txt, then: python3 test_postman.py
```

## Run

```sh
dotnet run --project src/ApiService      # http://localhost:9000/api/...  (needs the ASP.NET Core 10 runtime)
./run.sh                                 # Docker, host 9000 -> container 8003
```

`/openapi/v1.json` is served in Development. Example requests are in `src/ApiService/ApiService.http`.

## Docker (local test environment, by IP, plain HTTP)

`docker-compose.yml` (API) and `ui/docker-compose.yml` (UI + nginx) run side by side on the shared `proxy-net`
network. The UI's nginx proxies `/api/` to the `quillingartistdotnet` container, so the browser only talks to the UI.

| URL | What |
|---|---|
| `http://<host-ip>:8080/` | Storefront (`UI_PORT` overrides 8080) |
| `http://<host-ip>:8003/api/...` | API directly, for testing (`API_PORT` overrides 8003) |

One-time host setup:

```sh
docker network create proxy-net
```

The API reaches the host's Postgres at `host.docker.internal` (the Docker gateway, e.g. `172.17.0.1`), not
`127.0.0.1`. A default Postgres only listens on `localhost`, so allow Docker containers in (Arch paths shown):

```sh
# /etc/postgresql/16/main/pg_hba.conf
listen_addresses = '*'          # pg_hba.conf below still limits who can connect

# /var/lib/postgres/data/pg_hba.conf  (Docker bridge networks are inside 172.16.0.0/12) local
# /etc/postgresql/16/main/pg_hba.conf
host  app_db  app_user  172.16.0.0/12  scram-sha-256

sudo systemctl restart postgresql
```

Then start both (the API first, since the UI proxies to it):

```sh
export DB_PASSWORD=... STRIPE_SECRET_KEY=... BREVO_API_KEY=... JWT_SIGNING_KEY="$(openssl rand -base64 48)"
docker compose up -d --build
docker compose -f ui/docker-compose.yml up -d --build
```

Stripe redirects back to whatever address the browser used (the UI sends `window.location.origin`), and Stripe
test mode accepts plain `http`, so checkout works by IP. Tokens for `selectAll`/`export` must be signed with the
same `JWT_SIGNING_KEY`: `JWT_SIGNING_KEY=... python3 test/make_token.py`.

## Endpoints (all under `/api`)

| Route | Methods |
|---|---|
| `/products`, `/customers`, `/orders`, `/orderitems`, `/addresses`, `/users` | `GET ""` (hello), `POST ""` (update), `PUT ""` (create), `GET selectAll` (paged, auth), `GET export` (CSV, auth) |
| `/checkout/create-session` | `POST` creates a Stripe Checkout Session |
| `/checkout/confirm` | `POST` verifies payment, records customer, address, order and items, emails support, and decrements stock |
