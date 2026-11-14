# O frontend é full-stack e faz o papel de BFF

O frontend usa um framework *full-stack* com SSR. O navegador fala só com o servidor do próprio frontend. As *server functions* desse servidor leem o cookie de sessão, obtêm um JWT de vida curta no serviço de autenticação e chamam a API com ele. Escolhemos isso, e não uma SPA chamando a API direto, porque assim não há CORS e nenhum token chega ao JavaScript do navegador. Descartamos um BFF como serviço separado porque o próprio servidor do frontend já cumpre esse papel, sem um deploy a mais.

## Consequências

- A API não precisa de CORS e só é chamada por servidores: os frontends e, se for o caso, o serviço de autenticação.
- O token de Recepção emitido pela API ([ADR 0006](./0006-api-autentica-convidado-e-recepcao.md)) fica num cookie `httpOnly` do frontend, nunca no JavaScript do navegador.
- O frontend precisa de um servidor rodando: não pode ser hospedado como arquivos estáticos.
- Cada chamada tem um salto a mais (navegador → servidor do frontend → API).
