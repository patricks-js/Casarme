# Casarme

Plataforma em que os noivos organizam convidados, convites, RSVP e recepção de todos os eventos do seu casamento num só lugar.

## Casamento e Eventos

**Casamento**:
O espaço que concentra tudo da organização de um casal: eventos, convidados e convites.
_Evite_: espaço digital, conta, projeto

**Link de acesso**:
Link de uso único e com validade que um Noivo gera para que outra Conta se torne Noivo do mesmo Casamento.
_Evite_: convite de noivo, convite de acesso, compartilhamento

**Identificador do Casamento**:
Nome curto e único de um Casamento (ex.: `joao-e-maria`), usado como prefixo das URLs. Não tem página pública atrás.
_Evite_: link do espaço digital, link do casamento, slug (fora do código)

**Conta**:
A identidade de quem faz login na plataforma. Fica fora do domínio do Casamento; convidados e Cerimonialista não têm Conta.
_Evite_: usuário, login, perfil

**Noivo**:
O vínculo que dá a uma Conta acesso total a um Casamento. Um Casamento tem um ou mais Noivos, todos com o mesmo poder, e uma Conta é Noivo de no máximo um Casamento.
_Evite_: casal (quando falar das contas), organizador, dono, administrador

**Evento**:
Uma ocasião do Casamento com data, horário e local próprios (ex.: noivado, chá de panela, cerimônia). Pode estar **ativo** ou **cancelado**, e pode ser **com Recepção** ou **sem Recepção**.
_Evite_: festa, ocasião, evento inativo

**Evento principal**:
A cerimônia, criada junto com o Casamento. A data dela é a data do casamento. Não pode ser cancelada.
_Evite_: data principal, evento padrão

**Evento sem Recepção**:
Evento em que só se confirma presença: não há QR code, Check-in nem acesso por PIN. Os Convites e o RSVP funcionam normalmente.
_Evite_: evento público, evento privado, evento aberto

**Cerimonialista**:
A pessoa que opera a recepção dos eventos no lugar dos Noivos. Não tem conta: entra só com o PIN de Recepção.
_Evite_: organizador, recepcionista

## Convidados e Convites

**Convidado**:
Uma pessoa da lista do Casamento. Pertence a exatamente um Grupo.
_Evite_: participante, pessoa, guest

**Grupo**:
O núcleo social de Convidados que é convidado em conjunto: uma pessoa sozinha, um casal ou uma família.
_Evite_: família, núcleo, convite (no sentido de agrupamento)

**Convite**:
Um Grupo convidado para um Evento específico. É o que tem link próprio, RSVP, prazo e QR code. Um Grupo convidado para três Eventos tem três Convites.
_Evite_: relação convite-evento, vínculo, convocação

**Status de envio**:
Como o Convite chegou ao Grupo: **não enviado**, **enviado** (e-mail disparado ou WhatsApp aberto pelos Noivos) ou **compartilhado manualmente**.
_Evite_: entregue, disparado, sem canal (é um aviso, não um status)

**Membro elegível**:
Convidado do Grupo que foi selecionado para um Convite. Só ele aparece no RSVP e na recepção daquele Evento.
_Evite_: participante, convidado do evento

**Representante**:
O Membro elegível a quem o Convite é endereçado. Em princípio é o representante do Grupo; se ele não for elegível para o Evento, passa a ser outro Membro elegível.
_Evite_: responsável, titular, líder

## RSVP

**RSVP**:
A resposta de um Convite dizendo, para cada Membro elegível, se ele vai ou não vai. Só conta como enviada quando todos os Membros elegíveis foram marcados.
_Evite_: confirmação (quando o termo for ambíguo), resposta parcial

**Prazo de RSVP**:
A data-limite de um Evento depois da qual o Convidado não pode mais responder nem alterar o RSVP. O prazo não vale para os Noivos.
_Evite_: deadline (fora do código), data limite

**Status do Convite**:
Em que pé o RSVP do Convite está: **pendente** (ainda sem resposta), **respondido** (RSVP enviado) ou **expirado** (prazo vencido sem resposta). Se o Grupo for retirado do Evento, o Convite fica **cancelado**: o link deixa de funcionar, mas o histórico é mantido.
_Evite_: parcial, confirmado (como status do Convite), convite excluído

## Recepção

**Recepção**:
A operação de entrada num Evento no dia em que ele acontece, conduzida pela Cerimonialista.
_Evite_: portaria, credenciamento

**Check-in**:
O registro de que um Membro elegível entrou no Evento. Pode ser feito para qualquer Membro elegível, tenha ele confirmado ou não. O Check-in do Convite fica **completo** quando todos os membros que responderam "vai" já entraram, e **parcial** quando entrou ao menos um, mas ainda falta alguém que respondeu "vai".
_Evite_: presença, entrada, credenciamento

**PIN de Recepção**:
Código gerado pelos Noivos que dá à Cerimonialista acesso à Recepção de todos os Eventos do Casamento.
_Evite_: senha, código de acesso, token
