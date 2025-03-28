# Gestão Inscrições em Avaliações Extraordinárias

## Descrição do Sistema
Neste projecto pretende-se um sistema de informação que faça a gestão das inscrições
e pagamentos das inscrições dos alunos nas avaliações extraordinárias que
têm lugar a pagamento, mesmo quando efectuadas em época de recuperações
prevista em calendário.
O processo de pedido de avaliação extraordinária (AE) passa por três “actores” no
sistema: o aluno, que faz o pedido, o professor, que valida esse pedido, e a secretaria
que regista o pagamento pelo aluno da AE.
Cada pedido refere-se a uma única AE e tem taxa associadas de valores diferentes,
dependendo da altura é que é realizada. Actualmente só estão consideradas três
situações: isento, época de recuperações ou fora de época, mas o sistema deve
suportar a introdução de outras situações.
Cada pedido deve ser numerado automaticamente com o número de ordem do pedido
e do número do aluno. Por exemplo, 02/I180500 seria o segundo pedido do aluno de
número 180500.
O pedido deve indicar qual a data e hora do exame, duração, o curso, a disciplina e o
módulo (ou UFCD) que se pretende e ainda o professor que irá realizar a AE.
Uma vez inserido o pedido, o mesmo tem de ser validado pelo professor, passando ao
estado “para pagamento” (ou “não aprovado”, situação que termina o processo).
Quando o pagamento for efectuado, a secretaria altera o pedido do pedido para “pago”,
indicando qual a forma de pagamento.
Na altura de validação, o professor pode alterar a duração da AE, mas não a data e a
hora. Pode também indicar um novo professor para fazer a AE, mantendo-se nesse
caso o estado do pedido em “para aprovação”.
Por fim, o professor que realizou a AE termina o processo indicando qual o nota final do
aluno nesse módulo/UFCD (ou se faltou à AE), sendo alterado o estado do pedido para
“lançado”.
Em cada passo do processo (para aprovação, aprovado, pago e terminado) deverá ser
registada a data e hora em que esse passo foi efectuado, sendo necessário controlo
sobre o estado do pedido de AE. Por exemplo, a secretaria não pode marcar um
pedido como “pago” se não estiver no estado “aprovado”.

## Implementação
Na implementação do sistema, todo o processo de pedido de uma AE terá de ser feita
via web.
O backoffice (inserir/actualizar alunos, curso, disciplinas, etc.) pode ser feita utilizando
Windows Forms.
Note que quer num no backoffice, quer na plataforma web, é necessário um processo
que identifique o papel do utilizador no sistema (aluno, professor, secretaria).
Deve ser considerado um papel de utilizador especial de consulta de qualquer pedido,
e devem ser implementadas algumas consultas em bulk. Por exemplo, todos os
pedidos de um aluno ou todos os pedidos referentes a um módulo.
Note que pelo menos as entidades “aluno” e “pedido” têm de ter as funcionalidades
CRUD (Create, Read, Update, Delete) implementadas.
No entanto, uma entidade “pedido” não pode ser apagada depois de ter sido aprovada,
nem mesmo se não foi aprovada pelo professor.
