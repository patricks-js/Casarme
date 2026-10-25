# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> NEVER MAKE COMMIT MESSAGES WITH CLAUDE AS CO-AUTHOR; KEEP COMMIT MESSAGES SIMPLE

## Project

Casarme is a wedding-organization platform: couples manage weddings, multiple events per wedding, guests grouped into invitations, per-event RSVP, and QR-code check-in. The product spec lives in `docs/PRD.md` (in Portuguese). It defines the MVP scope checklist, functional requirements (`FR-xx`), and business rules (`BR-xx`). Check it before implementing domain features, and keep the scope checklist in sync as features land.

Monorepo layout:

- `apps/backend`: ASP.NET Core (.NET 10) minimal API with EF Core + PostgreSQL (`Casarme.slnx` → `src/Casarme.Api`)
- `apps/web`: TanStack Start (React 19, Vite, Tailwind v4, shadcn/base-ui) app using Bun, with better-auth + Drizzle for sign-in
- `docker-compose.yaml`: local Postgres 18 (`casarme_db`, user/password `postgres`, port 5432)

## Commands

Common commands are wrapped in the root `Taskfile.yml` ([go-task](https://taskfile.dev)); run `task` to list them all.

```sh
task setup      # lefthook install, bun install, dotnet restore, start db, apply all migrations
task dev        # start db, then API (dotnet watch, http://localhost:5264) + web (http://localhost:3000) in parallel
task migrate    # apply EF Core (public schema) + Drizzle (auth schema) migrations
task fmt        # format backend and web
task check      # same checks as the pre-push hook

task db:up | db:down | db:logs | db:psql | db:reset   # db:reset drops the volume (asks first)
task api:migration NAME=<Name>                        # dotnet ef migrations add, output in Data/Migrations
task api:db:update                                    # dotnet ef database update
task web:db:generate                                  # drizzle-kit: SQL migration from src/db/schema.ts into drizzle/
task web:db:migrate                                   # apply drizzle migrations
task web:ui:add -- <component...>                     # add shadcn components
```

Compose defaults to `podman-compose`; override with `COMPOSE="docker compose" task ...`. When adding a recurring dev command, add a task for it too. The underlying commands: `dotnet build|run|watch --project src/Casarme.Api` from `apps/backend` (https profile: 7222), `bun run dev|build|db:generate|db:migrate` from `apps/web`.

Formatting: `bun fmt` in `apps/web` (config in `bunfig.toml` `[fmt]`), `dotnet format` in `apps/backend` (rules in `apps/backend/.editorconfig`). Git hooks run via lefthook (`lefthook.yml`, run `lefthook install` once): pre-commit formats staged files, pre-push runs format check + typecheck/build. Each command only runs for the app whose files changed.

In Development the OpenAPI doc and Scalar UI are served at `/openapi`. No test projects or lint config exist yet.

## Backend architecture

- **Composition**: `Program.cs` wires everything. Services are registered by hand (`AddScoped<WeddingService>()`), so a new service needs a registration line there. Endpoint groups are extension methods (`MapWeddingEndpoint`) in `Endpoints/`, called from `Program.cs`.
- **Layering**: Endpoint → Service → `AppDbContext`, with no repository layer.
  - Endpoints map request records (`CreateWeddingRequest`) to command records (`CreateWeddingCommand`) and add the caller's owner id.
  - Services take commands and query `AppDbContext` directly. Request and command records are declared at the bottom of the endpoint and service files.
- **Error convention**: services signal outcomes with exceptions, and endpoints translate them to HTTP results:
  - `ArgumentException`/`ArgumentOutOfRangeException` → 400, body `{ error, field }` with `field` = the exception's `ParamName` so the web form can flag the input
  - `KeyNotFoundException` → 404
  - `InvalidOperationException` (e.g. duplicate slug) → 409
  - anything else bubbles up to `UseExceptionHandler` + ProblemDetails (bare 500, no details); endpoints don't catch `Exception`
- **Auth / multi-tenancy**: JWT bearer; tokens are meant to be issued by the web app's better-auth `jwt` plugin (dev issuer `http://localhost:3000`, audience `http://localhost:5264`). `ICurrentUser` (`Common/`) reads `sub`/`email` claims. Every endpoint parses `currentUser.Id` as a `Guid` owner id and scopes queries by `OwnerId`, so access control is ownership filtering in the service queries. JWT issuer/audience and `Authentication:JwksUrl` come from `appsettings*.json` (`casarme.app` in prod). better-auth has no OIDC discovery document, so `Common/JwksRetriever.cs` loads signing keys straight from its JWKS endpoint. Keys must be ES256: better-auth defaults to EdDSA, which Microsoft.IdentityModel can't verify.
- **Entities** (`Entities/`): constructor-initialized classes with `Guid.CreateVersion7()` ids. Mutations go through methods like `Wedding.Update(...)`. `Slug` has a private setter. Shared limits/format live in `WeddingRules` (mirrored in web `src/lib/wedding.ts`). Response shapes are records in `DTOs/`; services return those, never entities. Entities implementing `IHasTimestamps` get `CreatedAt`/`UpdatedAt` set in `AppDbContext.SaveChangesAsync` (NFR-03). Fluent config in `OnModelCreating` adds unique indexes on `Weddings.Slug` (BR-01) and `Weddings.OwnerId` (MVP rule: one wedding per user); services pre-check both and also map Postgres unique violations to 409.

## Web architecture

- TanStack Start file-based routing in `src/routes/`, with the route tree generated into `src/routeTree.gen.ts` (don't edit it by hand). The Vite plugin uses `routeToken: "layout"`, so a folder's `layout.tsx` is its layout route (the token is matched against the file name without extension).
- Import alias `#*` → `src/*` (`package.json` `imports`); imports include the extension, e.g. `#lib/env/server.ts`, `#components/ui/button.tsx`. shadcn aliases in `components.json` follow the same scheme. Icons: Tabler.
- **Env**: validated with `@t3-oss/env-core` + zod. `src/lib/env/server.ts` (`DATABASE_URL`, `BETTER_AUTH_URL`, `BETTER_AUTH_SECRET`, `API_URL`, read from `Bun.env`) and `src/lib/env/client.ts` (`VITE_API_URL`). Values live in `apps/web/.env` (gitignored).
- **Database**: Drizzle ORM (`drizzle-orm/bun-sql`, v1 RC with `defineRelations`) on the same Postgres as the backend. Schema in `src/db/schema.ts` uses `snakeCase.schema("auth")`, so web-owned tables live in the `auth` Postgres schema while EF Core owns the `public` tables. Drizzle migrations go to `drizzle/`.
- **Auth**: `src/lib/auth/server.ts` configures better-auth with the Drizzle adapter (table mapping to `schema.ts`), email/password using `Bun.password`, UUIDv7 ids, and plugins `jwt` (issues tokens for the .NET API), `openAPI`, `tanstackStartCookies`. `src/routes/api/auth/$.ts` forwards GET/POST to `auth.handler`. `src/lib/auth/client.ts` is the browser client (with `jwtClient`).
- **Server functions** (`createServerFn`) live in `src/functions/`, one file per domain named `<domain>.functions.ts` (e.g. `auth.functions.ts` with `getSession`/`ensureSession`).
- **Calling the .NET API**: done from server functions (the API has no CORS). Mint a JWT with `auth.api.getToken({ headers })` and send it as `Authorization: Bearer` to `env.API_URL` (see `user.functions.ts`).
- **Route groups**: `src/routes/_auth/` is a pathless group for sign-in/sign-up; its `layout.tsx` redirects to `/` when a session already exists (`beforeLoad` + `getSession`). `src/routes/_app/` is the signed-in area: its layout redirects to `/sign-in` without a session, and `/` sends the user to `/weddings/new` or to their wedding space `/weddings/$weddingId`. Toasts use `sonner` (`toast` from `sonner`, `<Toaster>` in `__root.tsx`). Forms use TanStack Form with zod schemas and the shadcn `Field` components.
- Bun installs exact versions (`bunfig.toml`: `exact = true`).

## Project management

Work is tracked as GitHub issues in `patricks-js/Casarme`, linked to the "Casarme Project" board (user project #7) with Priority and Size fields. Use the `creating-github-issues` skill to open new tickets.

## Current state / known gaps

- Web UI so far: sign-in/sign-up, and wedding registration/space/edit (`_app/weddings/`). The `_app` guard is minimal: no redirect-back or logout yet (issue #4).
- `WeddingEventService` exists but isn't registered in DI and has no endpoints. Its entity file is misnamed `WeedingEvent.cs`.
