---
version: 1.1 (mvp)
status:
  - draft
start-date: 2026-07-23
---

# Product Requirement Document (PRD)

> O modelo de dados está em [docs/modelo-de-dominio.md](./docs/modelo-de-dominio.md) e a arquitetura em [docs/arquitetura.md](./docs/arquitetura.md).
>
> Os termos em **negrito** com inicial maiúscula (Casamento, Noivos, Evento, Grupo, Convite, Membro elegível, Representante, Prazo de RSVP, Recepção, Check-in, PIN de Recepção) seguem as definições do [GLOSSARY.md](./GLOSSARY.md). As decisões de arquitetura estão em [docs/adr/](./docs/adr/).

## 1. Visão do Produto

**Casarme é uma plataforma de organização de casamentos** que ajuda noivos a planejarem e operarem seus eventos em um ambiente único, **substituindo planilhas, documentos dispersos, mensagens e controles manuais** por um fluxo centralizado de **convidados**, **convites**, **RSVP** e **recepção**. O produto nasce como um hub operacional do casamento, e não como uma suíte completa para todos os aspectos da cerimônia desde o primeiro dia.

## 2. Problema

Casais em processo de casamento frequentemente precisam **organizar múltiplos eventos**, como noivado, chá de panela e cerimônia, usando ferramentas fragmentadas. Isso gera **retrabalho**, **perda de contexto**, dificuldade para acompanhar **confirmações**, dependência excessiva do **cerimonialista** e baixa visibilidade do que já foi resolvido ou ainda está pendente.

## 3. Público-Alvo (ICP - Ideal Customer Profile)

O usuário principal é o **casal de noivos** que está organizando seu casamento e deseja uma **experiência simples, amigável e centralizada**. Os dois noivos podem ter conta e gerenciar o mesmo Casamento com o mesmo nível de acesso. O usuário secundário é a **cerimonialista**, que opera a Recepção dos eventos por meio de um PIN, sem precisar de conta.

## 4. Proposta de Valor

A proposta de valor do produto é **permitir que o casal organize convidados e eventos do casamento em um único lugar**, com **convites digitais**, **confirmações centralizadas** e **recepção mais fluida** no dia do evento. Em vez de operar com planilhas e múltiplos apps, o casal passa a ter um **painel único** para controlar **quem foi convidado**, **quem confirmou** e **quem entrou em cada evento**.

## 5. Escopo do MVP

### 5.1 Dentro do Escopo

- [ ] **Feature 1**: Autenticação simples com login pelo Google.
- [ ] **Feature 2**: Cadastro do casamento.
- [ ] **Feature 3**: Cadastro de múltiplos eventos vinculados ao casamento, com ou sem Recepção.
- [ ] **Feature 4**: Cadastro manual e importação de convidados via planilha.
- [ ] **Feature 5**: Configurações básicas do casamento.
- [ ] **Feature 6**: Agrupamento social de convidados em Grupos.
- [ ] **Feature 7**: Criação de Convites (Grupo × Evento) com seleção dos Membros elegíveis.
- [ ] **Feature 8**: Envio de Convite por canal digital (e-mail ou WhatsApp) quando houver contato válido.
- [ ] **Feature 9**: Compartilhamento manual de link quando necessário.
- [ ] **Feature 10**: RSVP separado por evento.
- [ ] **Feature 11**: Edição do RSVP até o Prazo de RSVP do evento.
- [ ] **Feature 12**: Geração de QR code por Convite em eventos com Recepção.
- [ ] **Feature 13**: Check-in do evento via QR code ou busca por nome, com registro de presença.

### 5.2 Fora do Escopo

- ❌ Marketplace de fornecedores.
- ❌ Sistema avançado de permissões para cerimonialista/organizadores à gestão (RBAC).
- ❌ Aplicativo mobile nativo.
- ❌ Site público customizado (inclusive página pública do Casamento).
- ❌ Eventos abertos, em que qualquer pessoa com o link se inscreve sem estar na lista de convidados.
- ❌ Integração com a API oficial do WhatsApp Business ([ADR 0002](./docs/adr/0002-whatsapp-via-wa-me.md)).
- ❌ Lista de presentes.
- ❌ Seating plan.
- ❌ IA ou automações avançadas.
- ❌ Múltiplos fluxos sofisticados de comunicação.

## 6. Requisitos

### 6.1 Requisitos Funcionais (FR)

#### 6.1.1 Fundação do Casamento & Eventos

- **FR-01:** O sistema permite o cadastro do Casamento (nome do casal, data do casamento, Identificador do Casamento e fuso horário, com padrão `America/Sao_Paulo`).
- **FR-02:** Ao salvar as informações básicas, o sistema cria o Casamento e, junto com ele, o Evento principal (cerimônia) com a data informada.
- **FR-03:** O sistema permite editar as informações básicas do Casamento. A data do casamento é a data do Evento principal.
- **FR-46:** O sistema permite que um Noivo gere um Link de acesso, de uso único e com validade, para que outra Conta se torne Noivo do mesmo Casamento, com acesso total ([ADR 0004](./docs/adr/0004-noivo-como-vinculo-e-link-de-acesso.md)).
- **FR-57:** O sistema permite que um Noivo saia do Casamento ou remova outro Noivo, exceto quando for o último Noivo.
- **FR-12:** O sistema permite o cadastro de múltiplos eventos vinculados ao Casamento (nome, data, horário, local e se o Evento é com ou sem Recepção).
- **FR-13:** O sistema permite definir um Prazo de RSVP opcional para cada Evento.
- **FR-14:** O sistema permite a visualização de todos os eventos criados.
- **FR-47:** O sistema permite cancelar um Evento, exceto o Evento principal.
- **FR-61:** O sistema permite apagar um Evento enquanto nenhum Convite dele tiver sido enviado, exceto o Evento principal.
- **FR-48:** O sistema permite alterar a qualquer momento se um Evento é com ou sem Recepção, inclusive depois de enviados os Convites.

#### 6.1.2 Convidados e Grupos

- **FR-04:** O sistema permite o cadastro manual de convidados (com campo ‘Nome’ obrigatório e e-mail e telefone opcionais).
- **FR-60:** Quando um convidado é cadastrado sem Grupo, o sistema cria automaticamente um Grupo só com ele, com o nome dele.
- **FR-05:** O sistema permite criar Grupos, adicionar e remover convidados de um Grupo e mover convidados entre Grupos.
- **FR-06:** O sistema permite a visualização de todos os convidados cadastrados, agrupados por Grupo.
- **FR-29:** O sistema permite definir um convidado como representante do Grupo.
- **FR-49:** O sistema permite nomear e renomear o Grupo. Se o Grupo não receber nome, ele assume o nome do seu representante.
- **FR-62:** O sistema permite apagar um convidado, avisando antes quando ele já tiver respondido a algum RSVP ou feito Check-in, e apagar um Grupo vazio.

#### 6.1.3 Importação em Lote

- **FR-07:** O sistema permite a importação de listas de convidados através de arquivos nos formatos `.csv` e `.xlsx`.
- **FR-08:** O sistema valida o cabeçalho e a estrutura mínima do arquivo antes de iniciar o processamento.
- **FR-09:** O sistema realiza a importação parcial, salvando os registros válidos e rejeitando os inválidos.
- **FR-10:** O sistema exibe um relatório resumido pós-importação informando a quantidade de registros importados e rejeitados.
- **FR-11:** O sistema detalha os erros de importação apontando a linha exata e o motivo da falha.
- **FR-50:** O arquivo de importação tem as colunas `Nome` (obrigatória), `Grupo`, `Email`, `Telefone` e `Representante` (S/N), sendo opcionais todas menos `Nome`. Linhas com o mesmo valor em `Grupo` formam um Grupo; linha sem `Grupo` vira um Grupo só daquele convidado; se o nome do Grupo já existir no Casamento, o convidado é adicionado a esse Grupo; e, quando nenhuma linha do Grupo estiver marcada como `Representante`, a primeira linha do Grupo assume.
- **FR-59:** A importação ignora a linha cujo nome já existe no mesmo Grupo, comparando sem diferenciar maiúsculas e minúsculas, acentos e espaços extras, e mostra essa linha no relatório como "já existia". O relatório de importação não é guardado.

#### 6.1.4 Convites & Elegibilidade (Grupo × Evento)

- **FR-15:** O sistema permite convidar um Grupo para um Evento ativo, o que cria um Convite.
- **FR-16:** O sistema permite retirar um Grupo de um Evento. Se o Convite ainda não foi enviado, ele é removido; se já foi enviado, ele é cancelado: o link deixa de funcionar e o histórico é mantido.
- **FR-17:** O sistema permite selecionar parte ou todos os membros do Grupo como Membros elegíveis do Convite.
- **FR-18:** O sistema exibe claramente quais membros estão elegíveis em cada Convite.
- **FR-51:** O sistema permite alterar os Membros elegíveis até o Prazo de RSVP, mesmo depois do envio, mantendo o mesmo link. Se um membro sai, a resposta dele é descartada; se um membro entra, o Convite volta para pendente.
- **FR-52:** O sistema define o Representante do Convite: é o representante do Grupo quando ele é elegível para o Evento; caso contrário, outro Membro elegível.

#### 6.1.5 Disparo & Compartilhamento do Convite

- **FR-19:** O sistema gera um link único para cada Convite ([ADR 0001](./docs/adr/0001-um-link-por-convite.md)) e, em eventos com Recepção, um QR code associado.
- **FR-21:** O sistema permite iniciar o envio (manual ou em lote) para o contato do Representante. Se ele não tiver contato válido, o envio vai para outro Membro elegível que tenha. O e-mail é enviado pela plataforma; o WhatsApp abre um link `wa.me` com a mensagem pronta para os Noivos enviarem ([ADR 0002](./docs/adr/0002-whatsapp-via-wa-me.md)).
- **FR-22:** O sistema registra o status de envio do Convite: não enviado, enviado ou compartilhado manualmente.
- **FR-23:** O sistema avisa quando nenhum Membro elegível do Convite tem canal válido.
- **FR-24:** O sistema não bloqueia o envio do convite caso o envio nativo não seja possível.
- **FR-25:** O sistema permite copiar o link de RSVP com um clique.
- **FR-26:** O sistema permite marcar um Convite manualmente como compartilhado.

#### 6.1.6 Interface & Resposta de RSVP (Visão Convidado)

- **FR-27:** O convidado deve ver a tela correta do Convite ao abrir o link único.
- **FR-28:** O sistema mostra no Convite apenas os Membros elegíveis daquele evento. _(Redundância unificada com antigo FR-20)_
- **FR-30:** Quem abre o link do Convite marca a presença ou ausência (vai/não vai) de cada Membro elegível. O RSVP só é enviado quando todos os Membros elegíveis estão marcados.
- **FR-31:** O sistema permite editar uma resposta já enviada até o Prazo de RSVP.
- **FR-32:** O sistema informa claramente quando o Prazo de RSVP terminou.
- **FR-53:** O sistema mostra o QR code na página do Convite quando o Evento é com Recepção e o RSVP tem ao menos um "vai".

#### 6.1.7 Gestão & Acompanhamento (Visão Noivos)

- **FR-33:** O sistema mostra os Convites pendentes, respondidos, expirados e cancelados de cada evento.
- **FR-34:** O sistema mostra quantidade total de pessoas confirmadas por evento.
- **FR-35:** O sistema permite filtrar convites, eventos e respostas (RSVP) por status.
- **FR-44:** O sistema exibe o painel consolidado por evento contendo: total de Convites, respostas recebidas, pessoas confirmadas e, em eventos com Recepção, check-ins realizados.
- **FR-45:** O sistema exibe o detalhamento por participante: membros elegíveis, status de envio, resposta do RSVP e status de check-in.
- **FR-54:** O sistema permite que os Noivos registrem ou alterem o RSVP de qualquer Convite a qualquer momento, inclusive depois do Prazo de RSVP, ficando registrado que a alteração foi feita pelos Noivos.

#### 6.1.8 Recepção e Check-in

- **FR-36:** O sistema permite localizar o Convite a partir da leitura do QR code.
- **FR-55:** O sistema permite localizar o Convite pelo nome de um convidado, para quem chega sem o QR code.
- **FR-37:** Ao localizar o Convite, o sistema exibe os Membros elegíveis e a resposta de cada um no RSVP, destacando quem não confirmou ou respondeu que não ia.
- **FR-38:** O sistema registra a data e horário do check-in de cada membro.
- **FR-39:** O sistema permite registrar o check-in de qualquer Membro elegível do Convite, independentemente da resposta no RSVP. Membros não elegíveis não aparecem.
- **FR-40:** O sistema permite concluir o check-in do grupo mesmo sem a presença de todos os membros.
- **FR-41:** O sistema exibe o status de check-in do Convite como parcial ou completo.
- **FR-42:** O sistema permite atualizar o registro de presença caso os membros restantes do convite cheguem posteriormente.
- **FR-43:** O sistema permite aos Noivos gerar um PIN de Recepção para o Casamento, que dá à cerimonialista acesso à Recepção de todos os eventos sem conta nem login. O PIN não expira sozinho: vale até os Noivos o revogarem ou gerarem outro, e dá acesso apenas à Recepção.
- **FR-58:** O sistema permite que os Noivos façam Check-in pela própria Conta, sem PIN.
- **FR-56:** Ao entrar com o PIN, a cerimonialista vê os Eventos ativos, com Recepção e marcados para o dia de hoje. Se houver só um, a Recepção dele abre direto.

### 6.2 Requisitos Não-Funcionais (NFR)

- **NFR-01 (Feedback Visual):** O sistema deve fornecer feedback claro e imediato (mensagens de toast, alertas ou badges) indicando o sucesso ou falha de qualquer ação executada pelo usuário.
- **NFR-02 (Resiliência de Processamento):** A importação em lote não deve ser abortada por falhas ou erros em linhas individuais da planilha.
- **NFR-03 (Auditabilidade do Banco):** As tabelas principais da base de dados devem armazenar o registro de data e hora de criação (`created_at`) e atualização (`updated_at`).
- **NFR-04 (Arquitetura de Interface / Reatividade):** A aplicação deve operar como uma Aplicação de Página Única (SPA/PWA), garantindo a atualização e navegação fluida da interface sem necessidade de recarregamento completo (_full page reload_) do navegador.
- **NFR-05 (Links não adivinháveis):** O link do Convite deve usar um segredo aleatório que não seja possível adivinhar e não deve incluir o Identificador do Casamento (`/c/{segredo}`).
- **NFR-06 (Sigilo do PIN):** O PIN de Recepção é guardado apenas como hash e mostrado aos Noivos uma única vez, no momento em que é gerado.

### 6.3 Regras de Negócio (BR)

#### Casamento & Eventos

- **BR-01:** Todo Casamento tem um Identificador único, que pode ser editado. Ele aparece só nas telas dos Noivos e no endereço da Recepção, nunca nos links dos Convites. Não pode coincidir com caminhos reservados do sistema (ex.: `api`, `c`). No MVP não há página pública atrás dele.
- **BR-27:** Todo Casamento tem pelo menos um Noivo, e uma Conta é Noivo de no máximo um Casamento.
- **BR-28:** Todos os Eventos usam o fuso horário do Casamento. O Prazo de RSVP, a expiração dos Convites e os "Eventos de hoje" da Recepção são calculados nesse fuso.
- **BR-19:** Todo Casamento tem um Evento principal, criado junto com ele, que não pode ser cancelado.
- **BR-07:** Um Evento está ativo ou cancelado, e um Evento cancelado não pode ser reativado. Não é possível responder ao RSVP de um Evento cancelado nem de um Convite cancelado.
- **BR-23:** Um Evento sem Recepção não tem QR code nem check-in e não aparece para quem entra com o PIN de Recepção; nele, os Convites e o RSVP funcionam normalmente.

#### Convidados & Grupos

- **BR-02:** Todo convidado pertence a exatamente um Grupo.
- **BR-03:** Um Grupo contém um ou mais convidados (um indivíduo, um casal ou uma família).
- **BR-04:** E-mail e telefone/WhatsApp são dados opcionais no cadastro do convidado; o registro do convidado é válido mesmo sem canais digitais informados.
- **BR-05:** É permitido criar um Grupo vazio (sem convidados associados no momento da criação).
- **BR-24:** O nome do Grupo não pode se repetir dentro de um mesmo Casamento.
- **BR-25:** Quando um convidado é movido para outro Grupo, ele sai dos Convites do Grupo de origem.
- **BR-29:** Todo Grupo com pelo menos um convidado tem exatamente um representante. O primeiro convidado do Grupo vira representante; se o representante sair, assume o convidado mais antigo que ficou.
- **BR-30:** O contato (e-mail e telefone) pertence ao convidado, não ao Grupo. O telefone é guardado em formato internacional, com DDI.

#### Convites & Elegibilidade

- **BR-06:** Um Grupo só pode responder ao RSVP de um Evento se tiver Convite para aquele Evento.
- **BR-08:** Para continuar ativo, um Convite precisa ter pelo menos um Membro elegível. Se o último Membro elegível for apagado ou mudar de Grupo, o Convite é apagado, caso nunca tenha sido enviado, ou cancelado, caso já tenha sido.
- **BR-31:** Um Grupo tem no máximo um Convite não cancelado por Evento. Um Convite cancelado nunca é reativado: convidar o Grupo de novo cria um Convite novo, com link novo.
- **BR-32:** Só pode ser apagado o que nunca saiu do sistema: um Convite enviado, ou um Evento com Convite enviado, só pode ser cancelado.
- **BR-09:** A definição de Membros elegíveis de um Convite não altera a composição do Grupo.
- **BR-10:** O link do Convite deve levar estritamente ao Convite correspondente.
- **BR-11:** O link do Convite continua acessível depois do Prazo de RSVP, para consultar a resposta e o QR code, e só deixa de funcionar se o Convite ou o Evento for cancelado.
- **BR-12:** O envio de mensagens/links aos convidados pode ser realizado manualmente pelo casal mesmo que os canais nativos de disparo não estejam cadastrados.

#### RSVP

- **BR-20:** Se o Evento não tiver Prazo de RSVP definido, o prazo é o início do Evento.
- **BR-21:** Não existe RSVP parcial: o RSVP só conta como enviado quando todos os Membros elegíveis estão marcados como "vai" ou "não vai".
- **BR-13:** Depois do Prazo de RSVP, o convidado não pode mais enviar nem editar o RSVP. O prazo não se aplica aos Noivos.
- **BR-14:** A resposta mais recente gravada para o RSVP substitui a resposta anterior (_last write wins_).
- **BR-26:** O Convite está pendente (sem RSVP enviado), respondido (RSVP enviado), expirado (prazo vencido sem resposta) ou cancelado.

#### QR Code & Check-in

- **BR-15:** Cada Convite de um Evento com Recepção possui um QR code único, que contém o próprio link do Convite ([ADR 0003](./docs/adr/0003-qr-code-e-o-link-do-convite.md)).
- **BR-16:** Os QR codes dos Convites de um mesmo Grupo em eventos diferentes são obrigatoriamente distintos.
- **BR-17:** O QR code só fica disponível para o convidado quando o Evento é com Recepção e o RSVP tem ao menos um "vai".
- **BR-18:** Ao ser lido pela Recepção, o QR code leva direto ao Check-in daquele Convite naquele Evento.
- **BR-22:** O Check-in do Convite fica completo quando todos os membros que responderam "vai" já entraram, e parcial quando entrou ao menos um, mas ainda falta alguém que respondeu "vai". Membros que entram sem ter confirmado contam como presentes, mas não entram nessa conta.

## 7. Fluxos Principais

### FW-01: Configuração de Casamento

- **Ator**: Noivos.
- **Pré-requisito**: Estar autenticado na plataforma
- **Fluxo Principal**:
	- Os Noivos criam o Casamento, e o sistema cria o Evento principal junto.
	- Os Noivos cadastram os demais eventos, indicando se cada um é com ou sem Recepção.
	- Os Noivos importam ou cadastram convidados.
	- Os Noivos organizam os convidados em Grupos.

### FW-02: Convites e Elegibilidade por Evento

- **Ator**: Noivos.
- **Pré-requisito**: Estar autenticado na plataforma
- **Fluxo Principal**:
	- Os Noivos escolhem quais Grupos serão convidados para cada evento.
	- O sistema cria um Convite para cada Grupo convidado.
	- Em cada Convite, os Noivos definem quais membros do Grupo estão elegíveis.

### FW-03: Envio de Convites

- **Ator**: Noivos.
- **Pré-requisito**: Estar autenticado na plataforma
- **Fluxo Principal**:
	- O sistema gera o link de cada Convite.
	- Se houver contato válido, os Noivos disparam o envio pela plataforma: o e-mail é enviado pelo sistema e o WhatsApp é aberto com a mensagem pronta.
	- Se não houver, os Noivos copiam o link, compartilham por conta própria e marcam o Convite como compartilhado.

### FW-04: RSVP

- **Ator**: Convidado (normalmente o Representante)
- **Pré-requisito**: Ter acesso a um link válido de Convite
- **Fluxo Principal**:
	- O convidado abre o link do Convite.
	- O sistema mostra apenas aquele evento e apenas os Membros elegíveis.
	- O convidado marca, para cada Membro elegível, se vai ou não vai.
	- O sistema registra a resposta e permite editá-la até o Prazo de RSVP.
	- Em eventos com Recepção, se ao menos um membro vai, o QR code aparece na página do Convite.

### FW-05: Recepção

- **Ator**: Cerimonialista
- **Pré-requisito**: Ter o PIN de Recepção do Casamento (os Noivos podem fazer a Recepção pela própria Conta)
- **Fluxo Principal**:
	- A cerimonialista entra com o PIN e acessa a Recepção do evento do dia.
	- Ela lê o QR code do Convite ou busca o convidado pelo nome.
	- O sistema mostra o Convite, com os Membros elegíveis e a resposta de cada um.
	- A cerimonialista confirma a entrada dos membros presentes e, se necessário, registra a chegada dos demais mais tarde.
	- O sistema grava o horário de cada entrada e o status do check-in (parcial ou completo).

## 8. Métricas de Sucesso

As métricas iniciais devem medir adoção do fluxo central:

- Número de casamentos criados.
- Número de eventos criados por casamento.
- Percentual de convidados cadastrados com sucesso.
- Taxa de RSVP respondido.
- Tempo médio entre criação do evento e envio dos convites.
- Percentual de check-ins realizados pelo sistema (em eventos com Recepção).
- Percentual de casais que substituíram planilha pelo produto como ferramenta principal.

Para o MVP, a métrica mais importante provavelmente é: “casais conseguem gerenciar a lista e o RSVP do evento principal dentro do sistema até o dia do evento?”.
