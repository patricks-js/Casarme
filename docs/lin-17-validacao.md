# LIN-17 — Eventos

## Objetivo e menor escopo útil

Permitir aos Noivos manter os Eventos do próprio Casamento, usando o JWT e o BFF da LIN-15. Implementa a issue #14: lista ordenada, cadastro, edição, cancelamento e exclusão, com interface na página inicial. Não inclui Convites nem compartilhamento de acesso entre Noivos.

## Aceite

- Nome, data e horário locais ao fuso do Casamento, local, Recepção e Prazo de RSVP opcional.
- Prazo efetivo calculado pelo início quando ausente; lista por início com identificação do principal.
- Recepção pode mudar em qualquer edição. Cancelamento é irreversível; editar um cancelado mantém seu status.
- Principal não pode ser cancelado nem apagado. Editar as configurações do Casamento preserva o horário definido para ele.
- A autorização verifica o vínculo de Noivo em toda operação, inclusive combinando Casamento e Evento. Outra Conta recebe 404.

## Implementação revisável

`/casamentos/{casamentoId}/eventos`: GET, POST; `/{id}`: PUT, DELETE; `/{id}/cancel`: POST. Todos autenticados. Datas sem offset são mantidas como `timestamp without time zone`, com validação de horário local e rejeição de datas inexistentes na transição de fuso. O resolver `EventTimeZone.Resolve` concentra a obtenção do fuso. Campos de texto têm limites; status e principal não podem ser definidos pelo cliente. A migração Eventos foi aplicada apenas em Postgres descartável dos testes.

A cerimônia criada pela LIN-15 mantém local a definir até ser editada. Recepção começa desabilitada; o Noivo pode habilitá-la. Não há regra adicional impondo prazo anterior ao início, pois a issue e o PRD não a exigem. O bloqueio de exclusão por Convite enviado pertence ao ticket posterior.

## Evidências

- `APPDATA=/tmp/casarme-tests-eventos NUGET_HTTP_CACHE_PATH=/tmp/casarme-nuget-http DOCKER_HOST=unix:///run/user/1000/podman/podman.sock TESTCONTAINERS_RYUK_DISABLED=true dotnet test apps/api/Casarme.slnx`: **4 testes HTTP aprovados**, incluindo os três da LIN-15 e o fluxo completo de Eventos. JWT assinado pelo `dotnet user-jwts`, Postgres 18 real, sem bypass de autenticação.
- `bun run typecheck`: monorepo aprovado.
- `bun run --filter @casarme/web build`: build cliente/servidor aprovado.
- `bun run generate`: OpenAPI e cliente regenerados. Os schemas gerados foram exercitados com datas locais de request e response; `dates.local` evita rejeitar data válida do domínio.

O ambiente emite NU1900 porque o cache NuGet original está em montagem somente leitura; o build compila. APPDATA gravável é necessário neste sandbox para o SDK criar os JWTs. Nenhum token ou chave é versionado.

## Roteiro manual

Com uma sessão autenticada, cadastrar Casamento, criar Evento com data anterior à cerimônia, editar local, horário, Recepção e prazo. Conferir ordem, fuso e prazo efetivo. Cancelar, editar o cancelado e confirmar que permanece cancelado. Apagar o Evento. Editar o principal e confirmar ausência de ações cancelar/apagar. Alterar a data do Casamento e conferir preservação do horário principal.

## Limitações reais

A interface foi compilada, mas o fluxo visual autenticado com Google não foi exercitado: LIN-14 está adiada por credenciais. QR code, PIN, RSVP e histórico de Convites ainda pertencem aos tickets posteriores. Sem push, deploy ou migração em banco em uso.
