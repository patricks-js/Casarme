# A API autentica o convidado e a Recepção; o better-auth só autentica Contas

O serviço de autenticação (better-auth) cuida só das Contas, ou seja, dos Noivos. Os acessos sem Conta são credenciais do domínio, e quem os valida é a API: o convidado é autenticado pelo segredo do Convite, e a Cerimonialista troca o PIN de Recepção por um token de Recepção emitido pela própria API, de vida curta e válido só para o Check-in daquele Casamento. Descartamos criar pseudo-contas ou sessões anônimas no better-auth porque o serviço de autenticação teria de consultar Convites e PINs, que são dados da API, e passaria a depender do domínio, invertendo a dependência definida no [ADR 0005](./0005-frontend-api-e-auth-separados.md).

## Consequências

- A API aceita três tipos de credencial: o token de Conta (emitido pelo serviço de autenticação), o segredo do Convite e o token de Recepção (esses dois, a própria API emite ou valida).
- A API precisa de chave própria para assinar o token de Recepção, separada das chaves do serviço de autenticação.
- O limite de tentativas de PIN fica na API.
