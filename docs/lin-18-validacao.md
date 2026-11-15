# LIN-18 — Convidados e Grupos

## Objetivo e menor escopo útil

Permitir aos Noivos organizar manualmente a lista completa do Casamento em Grupos, pela página inicial, com API HTTP autenticada e isolamento por Conta. Referência: issue GitHub #15, PRD e modelo de domínio existentes.

Inclui cadastro/edição/exclusão de Convidado, criação/renomeação/exclusão de Grupo vazio, movimentação, retirada para Grupo individual e escolha/sucessão de representante. E-mail e telefone pertencem ao Convidado. Importação, Convites e efeitos sobre Convites enviados ficam nos respectivos tickets. Compartilhamento de acesso aos Noivos permanece adiado para Google/LIN-14.

## Critérios de aceite e decisões verificáveis

- Nome do Convidado obrigatório (até 200 caracteres); contatos opcionais, e-mail validado. Telefone exige DDI explícito com `+`, remove espaços, parênteses e hífens e guarda formato internacional com 8–15 dígitos. Não se adivinha país de números locais.
- Sem Grupo escolhido, o cadastro cria um Grupo individual com o nome do Convidado. Na edição, a mesma opção retira o Convidado para um novo Grupo individual; o Grupo de origem pode ficar vazio.
- Grupo pode ser vazio e sem nome. Sem nome explícito, exibe o representante; sem membros, a interface mostra “Grupo sem nome”. Nomes efetivos não se repetem no Casamento (comparação sem diferenciar caixa e ignorando espaços nas pontas). Conflitos retornam 409 e não deixam registros parciais. Para homônimos, selecionar Grupo existente ou renomear o Grupo antes do cadastro individual.
- Todo Grupo não vazio tem um representante: primeiro a entrar, sucessão pelo mais antigo ainda no Grupo e escolha explícita pelos Noivos. A ordem de entrada no Grupo é própria, independente de empates no relógio e da data original de cadastro do Convidado.
- Só Grupo vazio pode ser apagado. Retirada e exclusão de Convidado mantêm o Grupo restante válido.
- Todas as rotas são autenticadas e consultam o vínculo Conta–Casamento. IDs de Grupo e Convidado são resolvidos dentro do Casamento autorizado. Mutações serializam na linha do Casamento para evitar corridas entre movimentações, sucessão e nomes. Índice único de nomes no Postgres complementa essa proteção.
- BFF usa sessão/JWT somente no servidor, reutilizando helper `account.server.ts`. A interface lista Grupos vazios e todos os Convidados, permite editar contatos/mover, escolher representante e confirma exclusões.

## Evidências e reprodução

Em 15/11/2026, passaram:

- HTTP com JWTs de teste .NET e Postgres 18 real descartável: 5/5 no filtro `ConvidadoTests` (2 testes deste ticket e 3 fatos herdados da LIN-15). Cobre lista agrupada, contatos, Grupo individual e vazio, unicidade, renomeação, exclusão, movimentação, sucessão por antiguidade, escolha de representante, 401/404 por Conta, IDs estrangeiros, rollback e concorrência de nomes/entrada de membros.
- `bun run typecheck`: todos os 6 trabalhos passaram, incluindo build .NET/OpenAPI e regeneração do cliente.
- `bun run --cwd apps/web build`: build cliente, SSR e Nitro concluídos.
- Formatação C# e TypeScript aplicada com os comandos locais.

Comando HTTP neste sandbox (diretórios temporários evitam escrita em perfil somente leitura):

```sh
APPDATA=/tmp/casarme-tests-convidados NUGET_HTTP_CACHE_PATH=/tmp/casarme-nuget-http DOCKER_HOST=unix:///run/user/1000/podman/podman.sock TESTCONTAINERS_RYUK_DISABLED=true dotnet test apps/api/Casarme.slnx --filter FullyQualifiedName~ConvidadoTests
```

A fixture verifica token não vazio e remove chaves temporárias também quando APPDATA está definido. Migração aplicada somente nos containers descartáveis. Nenhuma migração em banco de uso, push ou deploy.

## Roteiro manual e limites reais

Após integrar Google/LIN-14: entrar, cadastrar Casamento, criar Grupo vazio, cadastrar primeiro e segundo Convidados nele, trocar representante, mover o representante para Grupo individual e verificar sucessão; editar contatos; tentar repetir nome do Grupo; excluir membros e só então apagar Grupo vazio. Usar segunda Conta para confirmar que a lista não aparece.

O fluxo visual com sessão Google real não foi exercitado; credenciais continuam adiadas. Os testes HTTP validam a API e a autorização com JWT assinado, não o login Google. Nenhum efeito de movimentação/exclusão sobre Convites foi implementado, conforme exclusão explícita da issue #15. O restore emitiu NU1900 porque o cache de auditoria NuGet original é somente leitura; compilação e testes concluíram.
