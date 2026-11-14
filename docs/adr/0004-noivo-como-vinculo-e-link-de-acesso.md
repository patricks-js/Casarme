# Noivo é um vínculo entre Conta e Casamento, e o parceiro entra por Link de acesso

A Conta (identidade de login) fica fora do domínio. Ela se liga ao Casamento por um Noivo, e todos os Noivos de um Casamento têm o mesmo poder. Isso substitui o modelo de "um dono por Casamento", que não permitia que os dois noivos gerenciassem o Casamento. Para virar Noivo, a segunda pessoa usa um Link de acesso de uso único e com validade, gerado por um Noivo. Descartamos o caminho de vincular pelo e-mail do parceiro porque ele só é seguro se a plataforma confirmar que a pessoa é dona daquele e-mail. Sem essa confirmação, qualquer um poderia criar uma Conta com o e-mail do parceiro e ganhar acesso ao Casamento.

## Consequências

- Todo acesso aos dados do Casamento é verificado pelo vínculo de Noivo, nunca por um campo de dono no Casamento.
- No MVP, uma Conta é Noivo de no máximo um Casamento.
- O último Noivo não pode sair, para que um Casamento nunca fique sem Noivo.
- Se um dia a plataforma passar a confirmar e-mails, a entrada por e-mail pode ser acrescentada sem mudar o modelo.
