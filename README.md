# Casarme

Plataforma de organização de casamentos: os Noivos organizam Eventos, Convidados, Convites, RSVP e Recepção num só lugar.

## Como subir o ambiente

Pré-requisitos: [Bun](https://bun.sh) 1.4.4 (versão fixada em `package.json`), [.NET SDK](https://dotnet.microsoft.com) 10 e Docker (ou Podman) com Compose.

```sh
bun install
dotnet tool restore                      # instala o dotnet-ef fixado em dotnet-tools.json
cp apps/auth/.env.example apps/auth/.env
cp apps/web/.env.example apps/web/.env
docker compose up -d                     # só o Postgres
bun run db:migrate                       # migrações da api (schema app) e do auth (schema auth)
bun run dev                              # web, auth e api com recarregamento automático
```

| Serviço | Endereço | Health |
|---|---|---|
| `web` | http://localhost:3000, que repassa `/api/auth/*` para o `auth` | http://localhost:3000/api/health |
| `auth` | http://localhost:3001 | http://localhost:3001/health |
| `api` | http://localhost:5100 (OpenAPI em `/openapi/v1.json`) | http://localhost:5100/health |

A página inicial do `web` mostra o status da `api`. O TanStack Query busca o dado por uma *server function* que usa o `api-client`, e o cache já chega hidratado do servidor.

Para o login com Google funcionar, preencha `GOOGLE_CLIENT_ID` e `GOOGLE_CLIENT_SECRET` em `apps/auth/.env`, com `http://localhost:3000/api/auth/callback/google` como URI de redirecionamento.

### Contrato `api` → `web`

O build da `api` gera `apps/api/openapi.json` a partir do código, e `packages/api-client` gera o cliente e os tipos a partir dele. O Turborepo encadeia isso antes de `build`, `typecheck` e `dev`. Ao mudar um endpoint com o `dev` rodando, rode `bun run generate` para atualizar o cliente. O `openapi.json` e o `src/generated/` gerados ficam versionados.

### Formatação e hooks

O TypeScript é formatado com `bun fmt` e o C# com `dotnet format`: `bun run format` formata tudo e `bun run format:check` só confere. O [lefthook](https://lefthook.dev) é instalado pelo `bun install` e liga dois hooks: o `pre-commit` formata os arquivos que você vai commitar, e o `pre-push` confere a formatação, roda o `bun run typecheck` e compila a `api`.

### Tudo em container

```sh
docker compose --profile full up --build
```

Sobe Postgres, os containers de migração (`api-migrate` e `auth-migrate`, que rodam e terminam antes dos serviços), `api`, `auth`, `web` e um Traefik que faz o roteamento de produção: `/api/auth/*` para o `auth` e o resto para o `web`. Acesse em http://localhost:8080.

## Documentação

- [PRD.md](./docs/PRD.md): escopo do MVP, requisitos (FR/NFR) e regras de negócio (BR).
- [GLOSSARY.md](./docs/GLOSSARY.md): a linguagem do domínio. Use estes termos no código, nas telas e nos tickets.
- [docs/modelo-de-dominio.md](./docs/modelo-de-dominio.md): entidades, valores calculados e regras de apagar vs. cancelar.
- [docs/arquitetura.md](./docs/arquitetura.md): peças do sistema, monorepo, ambientes, testes e deploy, além das decisões em aberto.
- [docs/adr/](./docs/adr/): decisões de arquitetura difíceis de reverter.

## Plano do MVP

A spec do MVP está na issue [#9](https://github.com/patricks-js/Casarme/issues/9), e cada ticket abaixo é uma sub-issue dela. **O status de cada ticket é acompanhado no GitHub**; esta tabela registra apenas o plano e a ordem entre os tickets.

| Ticket | Issue | Depende de |
|---|---|---|
| 01 Base do monorepo | [#10](https://github.com/patricks-js/Casarme/issues/10) | — |
| 02 Login com Google (+ padrão de testes da `api`) | [#11](https://github.com/patricks-js/Casarme/issues/11) | #10 |
| 03 Cadastro do Casamento | [#12](https://github.com/patricks-js/Casarme/issues/12) | #11 |
| 04 Noivos e Link de acesso | [#13](https://github.com/patricks-js/Casarme/issues/13) | #12 |
| 05 Eventos | [#14](https://github.com/patricks-js/Casarme/issues/14) | #12 |
| 06 Convidados e Grupos | [#15](https://github.com/patricks-js/Casarme/issues/15) | #12 |
| 07 Importação de planilha | [#16](https://github.com/patricks-js/Casarme/issues/16) | #15 |
| 08 Convites e Membros elegíveis | [#17](https://github.com/patricks-js/Casarme/issues/17) | #14, #15 |
| 09 Página do Convite e RSVP (+ padrão Playwright) | [#18](https://github.com/patricks-js/Casarme/issues/18) | #17 |
| 10 Compartilhamento manual e WhatsApp | [#19](https://github.com/patricks-js/Casarme/issues/19) | #17 |
| 11 Envio por e-mail | [#20](https://github.com/patricks-js/Casarme/issues/20) | #19 |
| 12 Cancelamento e mudanças depois do envio | [#21](https://github.com/patricks-js/Casarme/issues/21) | #19 |
| 13 Acompanhamento dos Noivos | [#22](https://github.com/patricks-js/Casarme/issues/22) | #18 |
| 14 PIN de Recepção e entrada da Cerimonialista | [#23](https://github.com/patricks-js/Casarme/issues/23) | #14 |
| 15 Check-in | [#24](https://github.com/patricks-js/Casarme/issues/24) | #18, #22, #23 |
| 16 CI | [#25](https://github.com/patricks-js/Casarme/issues/25) | #11 |

Os testes obrigatórios de cada ticket são testes de integração pela API HTTP, contra Postgres de verdade, e, nos fluxos de RSVP e Check-in, testes de ponta a ponta com Playwright. Cobertura além disso é desejável, mas não faz parte dos critérios de aceite.
