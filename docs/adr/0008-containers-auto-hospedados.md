# Containers Docker desde o início, hospedados primeiro em casa

Todos os serviços (`web`, `api`, `auth` e Postgres) rodam em containers Docker desde o primeiro dia, descritos num Docker Compose e gerenciados pelo **Dokploy**, que faz o deploy automático a partir das imagens publicadas pelo CI e cuida do proxy reverso (Traefik), dos domínios e do HTTPS. A primeira hospedagem é um servidor caseiro do autor; para a produção real, o plano é migrar para uma VPS. Escolhemos isso, e não um PaaS ou banco gerenciado, para manter o custo perto de zero enquanto o produto é validado. Como tudo já está em containers, migrar é instalar o Dokploy em outra máquina, levar o mesmo Compose e restaurar o banco.

## Consequências

- Backup do Postgres, atualizações e disponibilidade são responsabilidade nossa, não de um provedor.
- A disponibilidade de um servidor caseiro depende da energia e da internet da casa, o que pesa principalmente no dia de um evento com Recepção. A migração para a VPS precisa acontecer antes do primeiro casamento real.
- Os containers não podem depender de nada específico da máquina de casa (caminhos locais, rede da casa), para que a migração continue sendo só levar o Compose.
