# LIN-15 — Cadastro do Casamento

Implementação local em `feature/lin-15-cadastro-do-casamento`.

Patrick autorizou em 15/11/2026 desenvolver LIN-15 antes da LIN-14, adiada até haver credenciais Google. A dependência técnica necessária aqui é a validação JWT e o padrão de testes HTTP, incluídos nesta entrega. O login Google não foi implementado nem validado.

## Escopo e decisões

- Cadastro e edição na página inicial para uma Conta autenticada. Sem Casamento, o formulário de cadastro aparece; com Casamento, aparece a edição.
- O BFF obtém o JWT pelo cookie da sessão no auth; o token permanece no servidor.
- API: GET /casamentos/current, GET /casamentos/{id}, POST /casamentos, PUT /casamentos/{id}.
- Criação de Casamento, Noivo e cerimônia em uma transação EF; índices únicos protegem Identificador e Conta, inclusive sob concorrência. Não há leitura do schema auth.
- Data vem exclusivamente do Evento principal, guardada como início local à meia-noite enquanto não há horário informado. EventTimeZone.Resolve concentra o fuso.
- Identificador usa letras minúsculas ASCII, números e hífens, até 80 caracteres. A interface sugere a partir dos nomes e permite edição. Rotas reservadas ficam no código.
- Desenvolvimento usa chaves locais do dotnet user-jwts. Fora de Development/Testing, configurar Authentication__Issuer, Authentication__Audience e Authentication__JwksUrl (HTTPS). JWKS tem cache e atualização pelo ConfigurationManager. A integração real com Google/auth será validada na LIN-14.

## Validação

A suíte em apps/api/tests/Casarme.Api.Tests cria projeto temporário, gera tokens assinados com dotnet user-jwts, configura a API com as respectivas chaves locais e usa Postgres 18 real via Testcontainers. Não há bypass de autenticação. Relógio do domínio fixado no teste.

Cobre cadastro e defaults, validação de identificadores/fuso, edição da data da cerimônia, cópia e atualização de nome/e-mail, 401 sem token ou com token inválido, 404 entre Contas, duplicidade e concorrência tanto por Identificador quanto por Conta, com rollback sem registros órfãos.

Executar: `dotnet test apps/api/Casarme.slnx`.
No CachyOS com Podman: `systemctl --user start podman.socket`; depois `DOCKER_HOST=unix:///run/user/$(id -u)/podman/podman.sock TESTCONTAINERS_RYUK_DISABLED=true dotnet test apps/api/Casarme.slnx`.
A prontidão usa TCP externo e conexão Npgsql, pois podman exec neste ambiente falha com oom_score_adj. Cada teste descarta seu container e suas chaves temporárias.

Verificados: suíte HTTP (3 testes), build .NET, typecheck do monorepo e build do web. O fluxo visual com sessão Google real não foi exercitado; depende das credenciais adiadas. A migração só foi aplicada em bancos descartáveis de teste. Sem push, merge ou deploy.
