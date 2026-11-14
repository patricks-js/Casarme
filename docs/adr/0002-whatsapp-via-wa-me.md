# WhatsApp via link wa.me, sem API oficial

No MVP, o envio de Convites por WhatsApp não usa a API do WhatsApp Business. Ele abre um link `wa.me` com a mensagem e o link do Convite já preenchidos, e quem aperta enviar são os Noivos, do próprio celular. O e-mail, ao contrário, é enviado de fato pela plataforma. Escolhemos assim porque a API oficial exige conta Business verificada, templates aprovados pela Meta e cobrança por conversa, um custo e uma burocracia que não se justificam antes de validar o produto. Além disso, uma mensagem que chega do número dos próprios noivos é mais pessoal que uma que chega de um número da plataforma.

## Consequências

- O status **enviado**, no WhatsApp, quer dizer que o `wa.me` foi aberto, não que a mensagem foi entregue. A plataforma não tem como saber se ela foi de fato enviada.
- O envio em lote (FR-21) pelo WhatsApp é sequencial: os Noivos abrem e enviam uma conversa de cada vez.
