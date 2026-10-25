---
version: 1.0 (mvp)
status:
  - in-progress
---

# Product Requirement Document (PRD)

## 1. Visão do Produto

**Casarme é uma plataforma de organização de casamentos** que ajuda noivos a planejarem e operarem seus eventos em um ambiente único, **substituindo planilhas, documentos dispersos, mensagens e controles manuais** por um fluxo centralizado de **convidados**, **convites**, **RSVP** e **recepção**. O produto nasce como um hub operacional do casamento, e não como uma suíte completa para todos os aspectos da cerimônia desde o primeiro dia.

## 2. Problema

Casais em processo de casamento frequentemente precisam **organizar múltiplos eventos**, como noivado, chá de panela e cerimônia, usando ferramentas fragmentadas. Isso gera **retrabalho**, **perda de contexto**, dificuldade para acompanhar **confirmações**, dependência excessiva do **cerimonialista** e baixa visibilidade do que já foi resolvido ou ainda está pendente.

## 3. Público-Alvo (ICP - Ideal Customer Profile)

O usuário principal é o **casal de noivos** que está organizando seu casamento e deseja uma **experiência simples, amigável e centralizada**. O usuário secundário, pode ser o **cerimonialista ou organizador** que colabora na operação dos eventos.

## 4. Proposta de Valor

A proposta de valor do produto é **permitir que o casal organize convidados e eventos do casamento em um único lugar**, com **convites digitais**, **confirmações centralizadas** e **recepção mais fluida** no dia do evento. Em vez de operar com planilhas e múltiplos apps, o casal passa a ter um **painel único** para controlar **quem foi convidado**, **quem confirmou** e **quem entrou em cada evento**.

## 5. Escopo do MVP

### 5.1 Dentro do Escopo

- [x] **Feature 1**: Autenticação simples (ex: Social sign-in com Google).
- [x] **Feature 2**: Cadastro do casamento.
- [ ] **Feature 3**: Cadastro de múltiplos eventos vinculados ao casamento.
- [ ] **Feature 4**: Cadastro manual e importação de convidados via planilha.
- [ ] **Feature 5**: Configurações básicas do casamento.
- [ ] **Feature 6**: Agrupamento social de convidados em convites.
- [ ] **Feature 7**: Associação seletiva de membros do convite a eventos específicos.
- [ ] **Feature 8**: Envio de convite por canal digital quando houver contato válido.
- [ ] **Feature 9**: Compartilhamento manual de link quando necessário.
- [ ] **Feature 10**: RSVP separado por evento.
- [ ] **Feature 11**: Edição do RSVP até a deadline do evento.
- [ ] **Feature 12**: Geração de QR code por evento.
- [ ] **Feature 13**: Check-in do evento via QR code com registro de presença.

### 5.2 Fora do Escopo

- ❌ Marketplace de fornecedores.
- ❌ Sistema avançado de permissões para cerimonialista/organizadores à gestão (RBAC).
- ❌ Aplicativo mobile nativo.
- ❌ Site público customizado.
- ❌ Lista de presentes.
- ❌ Seating plan.
- ❌ IA ou automações avançadas.
- ❌ Múltiplos fluxos sofisticados de comunicação.

## 6. Requisitos

### 6.1 Requisitos Funcionais (FR)

#### 6.1.1 Fundação do Casamento & Eventos

- **FR-01:** O sistema permite o cadastro do casal (nome, data principal do casamento e link único do espaço digital).
- **FR-02:** O sistema cria um espaço único para o casamento após salvar as informações básicas.
- **FR-03:** O sistema permite editar as informações básicas do casamento.
- **FR-12:** O sistema permite o cadastro de múltiplos eventos vinculados ao casamento (nome, data, horário e local).
- **FR-13:** O sistema permite definir deadline de RSVP para cada evento.
- **FR-14:** O sistema permite a visualização de todos os eventos criados.

#### 6.1.2 Convidados e Convites

- **FR-04:** O sistema permite o cadastro manual de convidados (com campo ‘Nome’ obrigatório).
- **FR-05:** O sistema permite a criação de convites e a associação/dissociação de convidados a esses convites.
- **FR-06:** O sistema permite a visualização de todos os convidados cadastrados, agrupados por convite.
- **FR-29:** O sistema permite atribuir um convidado como representante do convite.

#### 6.1.3 Importação em Lote

- **FR-07:** O sistema permite a importação de listas de convidados através de arquivos nos formatos `.csv` e `.xlsx`.
- **FR-08:** O sistema valida o cabeçalho e a estrutura mínima do arquivo antes de iniciar o processamento.
- **FR-09:** O sistema realiza a importação parcial, salvando os registros válidos e rejeitando os inválidos.
- **FR-10:** O sistema exibe um relatório resumido pós-importação informando a quantidade de registros importados e rejeitados.
- **FR-11:** O sistema detalha os erros de importação apontando a linha exata e o motivo da falha.

#### 6.1.4 Vínculo & Elegibilidade (Convite-Evento)

- **FR-15:** O sistema permite vincular um convite a um evento ativo.
- **FR-16:** O sistema permite remover o vínculo de convites em eventos antes do envio dos convites.
- **FR-17:** O sistema permite selecionar parte ou todos os membros do convite como elegíveis para um evento específico.
- **FR-18:** O sistema exibe claramente quais membros estão elegíveis naquele evento.

#### 6.1.5 Disparo & Compartilhamento do Convite

- **FR-19:** O sistema gera um token/link único e um QR code associado para cada relação convite-evento.
- **FR-21:** O sistema permite iniciar envio (manual ou em lote) quando houver e-mail ou telefone válido.
- **FR-22:** O sistema registra o status de envio iniciado.
- **FR-23:** O sistema informa quando o canal do convite está ausente ou inválido.
- **FR-24:** O sistema não bloqueia o envio do convite caso o envio nativo não seja possível.
- **FR-25:** O sistema permite copiar o link de RSVP com um clique.
- **FR-26:** O sistema permite marcar um convite manualmente como compartilhado.

#### 6.1.6 Interface & Resposta de RSVP (Visão Convidado)

- **FR-27:** O convidado deve ver a tela correta do convite ao abrir o link único.
- **FR-28:** O sistema mostra apenas os membros elegíveis daquele evento no convite. _(Redundância unificada com antigo FR-20)_
- **FR-30:** O representante do convite pode marcar a presença ou ausência (vai/não vai) de cada membro elegível.
- **FR-31:** O sistema permite ao representante editar uma resposta já enviada até a data limite (deadline).
- **FR-32:** O sistema informa claramente quando o prazo para o RSVP expirou.

#### 6.1.7 Gestão & Acompanhamento (Visão Casal)

- **FR-33:** O sistema mostra convites pendentes, respondidos e expirados por evento.
- **FR-34:** O sistema mostra quantidade total de pessoas confirmadas por evento.
- **FR-35:** O sistema permite filtrar convites, eventos e respostas (RSVP) por status.
- **FR-44:** O sistema exibe o painel consolidado por evento contendo: total de convites vinculados, respostas recebidas, pessoas confirmadas e check-ins realizados.
- **FR-45:** O sistema exibe o detalhamento por participante: membros elegíveis, status de envio, resposta do RSVP e status de check-in.

#### 6.1.8 Recepção e Check-in

- **FR-36:** O sistema permite localizar a relação convite-evento a partir da leitura do QR code.
- **FR-37:** O sistema exibe os membros elegíveis e a resposta de RSVP confirmada ao escaneá-lo.
- **FR-38:** O sistema registra a data e horário do check-in do convite.
- **FR-39:** O sistema permite registrar a presença parcial dos membros elegíveis do convite.
- **FR-40:** O sistema permite concluir o check-in do grupo mesmo sem a presença de todos os membros.
- **FR-41:** O sistema exibe o status de check-in como parcial ou completo.
- **FR-42:** O sistema permite atualizar o registro de presença caso os membros restantes do convite cheguem posteriormente.
- **FR-43:** O sistema permite ao casal gerar um PIN/código de acesso temporário para a recepção, permitindo que a cerimonialista opere o scanner de check-in sem necessidade de conta/login completo no sistema.

### 6.2 Requisitos Não-Funcionais (NFR)

- **NFR-01 (Feedback Visual):** O sistema deve fornecer feedback claro e imediato (mensagens de toast, alertas ou badges) indicando o sucesso ou falha de qualquer ação executada pelo usuário.
- **NFR-02 (Resiliência de Processamento):** A importação em lote não deve ser abortada por falhas ou erros em linhas individuais da planilha.
- **NFR-03 (Auditabilidade do Banco):** As tabelas principais da base de dados devem armazenar o registro de data e hora de criação (`created_at`) e atualização (`updated_at`).
- **NFR-04 (Arquitetura de Interface / Reatividade):** A aplicação deve operar como uma Aplicação de Página Única (SPA/PWA), garantindo a atualização e navegação fluida da interface sem necessidade de recarregamento completo (_full page reload_) do navegador.

### 6.3 Regras de Negócio (BR)

- **BR-01:** Todo casamento possui um link único para o seu espaço digital.
- **BR-02:** Todo convidado pertence obrigatoriamente a um único convite ativo.
- **BR-03:** Um convite contém um ou mais convidados (um indivíduo ou uma família/grupo social).
- **BR-04:** E-mail e telefone/WhatsApp são dados opcionais no cadastro do convidado; o registro do convidado é válido mesmo sem canais digitais informados.
- **BR-05:** É permitido criar um convite vazio (sem convidados associados no momento da criação).
- **BR-06:** Um convite não vinculado a um evento não pode responder ao RSVP daquele evento.
- **BR-07:** Um convite não pode responder ao RSVP de um evento inativo ou cancelado.
- **BR-08:** Pelo menos um membro elegível é exigido para manter ativo o vínculo entre um convite e um evento.
- **BR-09:** A definição de elegibilidade de membros para um evento não altera a composição social original do convite.
- **BR-10:** O link de convite deve redirecionar estritamente para o registro e dados do convite correspondente.
- **BR-11:** O link do convite permanece válido até a deadline do RSVP ou cancelamento do evento.
- **BR-12:** O envio de mensagens/links aos convidados pode ser realizado manualmente pelo casal mesmo que os canais nativos de disparo não estejam cadastrados.
- **BR-13:** A edição ou envio de respostas de RSVP deve ser bloqueada automaticamente após o encerramento da deadline do evento.
- **BR-14:** A resposta mais recente gravada para o RSVP substitui a resposta anterior (_last write wins_).
- **BR-15:** Cada relação convite-evento possui um QR code único.
- **BR-16:** QRs de eventos diferentes associados ao mesmo convite são obrigatoriamente distintos.
- **BR-17:** O QR code fica disponível para consulta do convidado assim que houver confirmação de resposta no RSVP.
- **BR-18:** O QR code direciona a leitura da recepção diretamente para o registro correto de recepção/check-in daquele evento.

## 7. Fluxos Principais

### FW-01: Configuração de Casamento

- **Ator**: Noivos/Organizador.
- **Pré-requisito**: Estar autenticado na plataforma
- **Fluxo Principal**:
  - O casal cria o casamento.
  - O casal cadastra os eventos.
  - O casal importa ou cadastra convidados.
  - O casal organiza os convidados em convites.

### FW-02: Elegibilidade de Convidado Por Evento

- **Ator**: Noivos/Organizador.
- **Pré-requisito**: Estar autenticado na plataforma
- **Fluxo Principal**:
  - O casal escolhe quais convites participam de cada evento.
  - Dentro de cada convite, o casal define quais membros estão elegíveis para aquele evento.
  - O sistema cria a relação convite-evento correspondente.

### FW-03: Envio de Convites

- **Ator**: Noivos/Organizador.
- **Pré-requisito**: Estar autenticado na plataforma
- **Fluxo Principal**:
  - O sistema gera o link de RSVP do convite para cada evento.
  - Se houver contato válido, o casal pode disparar o envio a partir da plataforma.
  - Caso contrário, o casal pode copiar e compartilhar manualmente o link.

### FW-04: RSVP

- **Ator**: Convidado
- **Pré-requisito**: Ter acesso a um link válido de convite
- **Fluxo Principal**:
  - O representante abre o link do evento.
  - O sistema mostra apenas aquele evento e apenas os membros elegíveis.
  - O representante informa quem vai ou não vai.
  - O sistema registra a resposta e permite edição até a deadline.

### FW-05: Recepção

- **Ator**: Cerimonialista
- **Pré-requisito**: Ter um PIN para acesso à área de recepção
- **Fluxo Principal**:
  - A equipe de recepção acessa a área do evento.
  - O QR code daquele evento é escaneado.
  - O sistema localiza o registro do convite-evento.
  - A equipe confirma a entrada e, quando necessário, ajusta a chegada parcial dos participantes.
  - O sistema grava horário e status do check-in.

## 8. Métricas de Sucesso

As métricas iniciais devem medir adoção do fluxo central:

- Número de casamentos criados.
- Número de eventos criados por casamento.
- Percentual de convidados cadastrados com sucesso.
- Taxa de RSVP respondido.
- Tempo médio entre criação do evento e envio dos convites.
- Percentual de check-ins realizados pelo sistema.
- Percentual de casais que substituíram planilha pelo produto como ferramenta principal.

Para o MVP, a métrica mais importante provavelmente é: “casais conseguem gerenciar a lista e o RSVP do evento principal dentro do sistema até o dia do evento?”.
