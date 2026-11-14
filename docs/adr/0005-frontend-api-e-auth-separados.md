# Frontend, API e autenticação como partes separadas num monorepo

O sistema é dividido em partes com deploys independentes, todas num mesmo monorepo:

- **API em .NET/C#**: é um *resource server*, ou seja, só serve a API do domínio e valida tokens que não emitiu.
- **Serviço de autenticação em TypeScript com better-auth**: cuida das Contas (cadastro, login, sessão) e emite os tokens que a API aceita.
- **Frontends**: um ou mais, podendo haver um por público (Noivos, convidado, Recepção), se isso se justificar.

Escolhemos essa separação, e não um monólito *full-stack*, para manter o domínio numa API própria em .NET, independente de qualquer frontend, e para reaproveitar o better-auth em vez de implementar autenticação na mão.

## Consequências

- A API não guarda senhas nem sessões de Conta. Ela só valida os tokens emitidos pelo serviço de autenticação, com as chaves públicas desse serviço.
- Existem pelo menos três deploys, e um contrato entre eles (formato do token, quem emite, quem valida) que precisa ser mantido.
- Os tipos da API não são compartilhados automaticamente com os frontends. É preciso um contrato explícito entre eles.
