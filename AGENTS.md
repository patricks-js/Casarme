# Defaults to use Bun

Default to using Bun instead of Node.js for all JavaScript/TypeScript operations.

## Core Commands

- Use bun <file> instead of node <file> or ts-node <file>
- Use bun test instead of jest or vitest
- Use bun install instead of npm install or yarn install or pnpm install
- Use bun run <script> instead of npm run <script> or yarn run <script> or pnpm run <script>
- Use bun x <command> instead of npx <command> or pnpm dlx <command>
- Bun automatically loads .env, so don't use dotenv.

## Built-in APIs

Prefer Bun's built-in APIs over external packages:

- Bun.serve() supports WebSockets, HTTPS, and routes. Don't use express.
- bun:sqlite for SQLite. Don't use better-sqlite3.
- Bun.redis for Redis. Don't use ioredis.
- Bun.sql for Postgres. Don't use pg or postgres.js.
- WebSocket is built-in. Don't use ws.
- Prefer Bun.file over node:fs's readFile/writeFile
- Bun.$ instead of execa.


## Testing

Use bun test to run tests.

```ts
import { describe, it, expect } from "bun:test";

describe("", () => {
  it("hello world", () => {
    expect(1).toBe(1);
  })
})
```

## Additional Resources

For more information, read the Bun API docs in node_modules/bun-types/docs/\*\*.md.

# Language of the code

The code is written in **English**: identifiers (variables, functions, types, classes, columns, routes), file names, folder names and code comments, in every app and package (TypeScript and .NET).

- **Only domain names may be in Portuguese**, for now: the glossary's terms (`Convite`, `Noivos`, `Recepção`...) used as names of domain concepts. The team may still move the domain names to English, so keep them isolated in domain code and don't spread Portuguese into anything else.
- **Everything that isn't explicitly domain is English.** `createApiClient`, not `criarApiClient`; `server/`, not `servidor/`; `Data/` and `Migrations/`, not `Dados/` and `Migracoes/`; `getStatus`, not `obterStatus`. User-facing text (UI strings) is not code and stays in Portuguese.

# Mics

- When grilling, ask one question at a time.
- Before working on a ticket, read README.md: it links the product docs (PRD, glossary, domain model, architecture, ADRs) and the MVP plan (spec issue #9 and its tickets). Use the glossary's vocabulary everywhere.
