# LIN-15 + LIN-17 + LIN-18 — validação conjunta local

## Objetivo, menor escopo útil e aceite

Reunir o cadastro do Casamento, a manutenção de Eventos e a lista de Convidados por Grupos numa branch local revisável. O aceite é executar as três entregas no mesmo modelo de banco e contrato API, preservar a data/horário da cerimônia entre os dois formulários e comprovar isolamento entre Contas. O login Google e o Link de acesso entre Noivos continuam adiados para LIN-14; efeitos sobre Convites enviados pertencem ao ticket posterior.

Fontes reutilizadas: issues GitHub #14 e #15, README, PRD, glossário, modelo de domínio, arquitetura e os documentos de validação LIN-15, LIN-17 e LIN-18. A skill `entregas-pequenas-e-verificaveis` v1 foi fornecida integralmente por Patrick nesta sessão.

## Localização e histórico

O `.git` de `/home/patrick/Documents/casarme-docs` está montado somente para leitura neste job. Um clone exclusivamente local, `/home/patrick/agentes/shared/casarme-local`, preserva o histórico até `068efe2e0533686bedc649bb226db5364f4c42f0` e permite os commits autorizados. Os hooks originais são executados por `core.hooksPath`. Nenhum remoto foi atualizado.

| Entrega | Branch | Worktree |
|---|---|---|
| LIN-15 | `feature/lin-15-cadastro-do-casamento` | `/home/patrick/agentes/shared/casarme-local` |
| LIN-17 | `feature/lin-17-eventos` | `/home/patrick/agentes/shared/casarme-local-lin17` |
| LIN-18 | `feature/lin-18-convidados-grupos` | `/home/patrick/agentes/shared/casarme-local-lin18` |
| Conjunto | `feature/lin-15-17-18-integracao` | `/home/patrick/agentes/shared/casarme-integracao` |

As duas worktrees antigas permanecem no baseline e limpas. A integração usa merges locais das branches de entrega, preservando os commits individuais. Consulte `git log --graph --oneline` para os hashes da integração e das entregas.

Commits das entregas: LIN-15 `068efe2`; LIN-17 `093598c` e `c0efbca`; LIN-18 `3bb7953`.

## Forma de validação automatizada

Na worktree de integração:

```sh
BUN_TMPDIR=/tmp BUN_INSTALL_CACHE_DIR=/tmp/casarme-bun-cache bun install --frozen-lockfile
NUGET_HTTP_CACHE_PATH=/tmp/casarme-nuget-http dotnet restore apps/api/Casarme.slnx --force
APPDATA=/tmp/casarme-tests-integracao \
  NUGET_HTTP_CACHE_PATH=/tmp/casarme-nuget-http \
  DOCKER_HOST=unix:///run/user/1000/podman/podman.sock \
  TESTCONTAINERS_RYUK_DISABLED=true \
  dotnet test apps/api/Casarme.slnx --no-restore
bun run generate
bun run typecheck
bun run --filter @casarme/web build
bun fmt --check
dotnet format apps/api/Casarme.slnx --verify-no-changes --no-restore
dotnet ef migrations has-pending-model-changes --project apps/api/src/Casarme.Api
```

O caminho do socket Podman depende do UID; neste ambiente é 1000. O socket já estava ativo. `APPDATA` gravável permite que o SDK grave as chaves de teste; não muda `HOME`. A fixture exige token não vazio, valida assinatura e apaga a chave temporária. O cache NuGet gravável e restore forçado eliminam o aviso NU1900 causado pelo cache original somente leitura.

Os testes criam Postgres 18 descartável via Testcontainers e aplicam as migrações somente nele. Nenhum comando desta validação migra um banco de uso. Os testes são HTTP com JWTs assinados pelo `dotnet user-jwts`, sem substituir autenticação por um handler falso.

## Roteiro manual conjunto

1. Com sessão autenticada disponível, abra `/`, cadastre o Casamento e confira a cerimônia criada e identificada como Evento principal.
2. Edite a cerimônia em Eventos: defina horário, local, Recepção e data. Confira que a data no formulário do Casamento acompanha a mudança. Edite a data pelo formulário do Casamento e confira que o horário foi preservado na lista de Eventos.
3. Crie um Evento anterior à cerimônia, sem prazo explícito. Confira ordem por data, fuso do Casamento e prazo efetivo igual ao início. Defina prazo e alterne Recepção.
4. Cadastre um Convidado sem Grupo nem contatos: confira o Grupo individual e o representante. Edite o contato com telefone internacional. Crie Grupo vazio sem nome, adicione dois Convidados e confira o nome do primeiro representante.
5. Troque o representante, mova-o para outro Grupo e confira a sucessão pelo membro mais antigo que ficou. Renomeie o Grupo; tente nome repetido e confira erro. Tente apagar Grupo não vazio; depois mova/apague seus membros e apague o Grupo vazio.
6. Cancele o Evento secundário, edite Recepção e confira que ele permanece cancelado e visível. Apague-o. Confira que Grupos e Convidados continuam intactos e que o principal não oferece cancelamento nem exclusão.
7. Em outra Conta com outro Casamento, confira listas independentes e rejeição de IDs da primeira Conta. As tentativas HTTP entre Contas e entre URLs de Casamentos distintos estão automatizadas.

## Evidência e limites

Execução final em 15/11/2026:

| Verificação | Evidência observada |
|---|---|
| HTTP + JWT .NET + Postgres 18 | **7 aprovados, 0 falhas, 0 ignorados**, 18 s |
| Build API | Aprovado, **0 warnings / 0 erros** após restore com cache gravável |
| Modelo EF | `No changes have been made to the model since the last migration.` |
| OpenAPI / cliente | Regenerados a partir dos dois módulos combinados |
| Typecheck monorepo | **6 tarefas aprovadas** |
| Build web | Cliente, SSR e Nitro aprovados, saída 0 |
| `bun fmt --check` | Aprovado |
| `dotnet format --verify-no-changes --no-restore` | Aprovado |

Os sete testes são os três da LIN-15, o fluxo de Eventos, dois fluxos de Grupos/Convidados e `Wedding_events_and_guests_coexist_with_account_isolation`. A infraestrutura foi extraída para `ApiTestBase`, evitando executar os testes herdados da LIN-15 novamente em cada classe. O fluxo conjunto comprova também rollback ao tentar criar Grupo individual com nome em conflito e retirada posterior para Grupo individual após resolver o nome.

Os conflitos locais foram resolvidos preservando os dois módulos em `Program`, no modelo EF e na página inicial; o cliente foi regenerado em vez de mesclar código gerado manualmente. O helper de JWT fica em `account.server.ts`. Datas locais de Evento são aceitas pelos schemas gerados (`dates.local`), sem conversão ao fuso do navegador.

Logs locais desta execução ficam em `/tmp/casarme-validation/`: `integration-tests.log`, `integration.trx`, `integration-api-build.log`, `integration-model-check.log`, `integration-generate.log`, `integration-typecheck.log`, `integration-web-build.log`, `integration-bun-format.log` e `integration-dotnet-format.log`. Este documento versionado mantém o resumo verificável mesmo após a limpeza de `/tmp`.

O roteiro visual depende da sessão Google real ainda adiada; compilação e testes HTTP não constituem evidência de uso visual autenticado. Não há hipótese tratada como teste aprovado. Importação, Convites, RSVP, PIN e Check-in não fazem parte destas entregas. Sem push, merge na principal, deploy, serviço pago ou migração em banco de uso.
