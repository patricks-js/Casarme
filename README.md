# Casarme

Plataforma de organização de casamentos: os Noivos organizam Eventos, Convidados, Convites, RSVP e Recepção num só lugar.

## Documentação

- [PRD.md](./PRD.md): escopo do MVP, requisitos (FR/NFR) e regras de negócio (BR).
- [GLOSSARY.md](./GLOSSARY.md): a linguagem do domínio. Use estes termos no código, nas telas e nos tickets.
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
