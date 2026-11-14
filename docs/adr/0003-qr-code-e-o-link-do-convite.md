# O QR code é o próprio link do Convite

Cada Convite tem um único segredo, que serve tanto para o link do RSVP quanto para o QR code de Check-in: o QR code contém o próprio link do Convite, e a Recepção extrai dele o Convite ao lê-lo. Os links levam só o segredo (`/c/{segredo}`), sem o Identificador do Casamento, para que o Identificador possa ser editado sem quebrar links e QR codes já enviados. Escolhemos um segredo só, e não um para o RSVP e outro para o Check-in, porque o risco que a separação evitaria é pequeno (o QR fica com o próprio Grupo, e depois do Prazo de RSVP o convidado não altera mais a resposta) e não compensa ter o dobro de identificadores e de regras de validade.

## Consequências

- Quem tem o QR code consegue abrir a página do RSVP do Convite.
- Depois que os QR codes forem enviados ou impressos, separar os segredos exigiria reenviar todos os Convites.
- Um Convite cancelado nunca volta a valer; convidar o Grupo de novo gera um Convite novo, com segredo novo.
