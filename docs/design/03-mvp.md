# Design 03: MVP, entregas e estimativas

Etapa 2 do projeto. Status: **aprovado pelo dono em 2026-11-11.**

Regra do projeto: 1 entrega = 1 sessão, e toda entrega tem algo que o dono consegue ver ou jogar.

## Três marcos

| Marco | O que existe ao final | Entregas |
|---|---|---|
| **1. Noite 1 jogável** | 12 a 15 minutos de jogo de verdade: loja, rua, bar, quitinete, primeira fita | 1 a 9 |
| **2. MVP: jogo completo** | As 3 noites, clímax com 3 saídas, 3 finais. Já seria publicável | 10 a 14 |
| **3. Versão 1.0** | Armadilha, final secreto, microfone, polimento, release | 15 a 19 |

A armadilha, o final secreto e o microfone ficam no marco 3 de propósito. Se o projeto apertar, o jogo do marco 2 já está completo.

## Marco 1: Noite 1 jogável

| # | Entrega | O que você vê ou joga | Valida |
|---|---|---|---|
| 1 | Instalação e projeto vazio | Anda em primeira pessoa por uma sala cinza | Ferramentas, renderizador, física, quadros por segundo na sua máquina |
| 2 | Filtro VHS e menu | A sala parece uma fita VHS. Menu de pausa com intensidade do filtro, sensibilidade e volume | Resolução interna baixa; opções desde o início |
| 3 | Física e interação | Chuta uma lata, pisa numa poça que ondula, pega um objeto | Três das quatro mecânicas de imersão |
| 4 | Corpo do Nondas | Olha para baixo e vê as pernas. A mão se estica para pegar | Pipeline de personagem e licença da malha base |
| 5 | A locadora | Anda pela loja pronta: balcão, prateleiras, TV, porta de aço | Textura procedural (parede e piso). Se não convencer, a decisão é reaberta aqui |
| 6 | As fitas | Esvazia a caixa de devoluções, rebobina, assiste na TV. A primeira fita adulterada aparece | Coração do jogo |
| 7 | Diálogo e idiomas | Atende Dona Neide. Legenda, ruído de fala, gesto. Troca entre português, inglês e espanhol no menu | Primeiro NPC; localização |
| 8 | A rua e o bar | Atravessa a rua, conversa com Tião, faz carinho no Paçoca, senta e bebe uma cerveja | O refúgio; o enquadramento da primeira fita visto da cadeira |
| 9 | Noite 1 completa | Joga a quinta-feira do início ao fim: missões, Freguês, Cabo Moreira, quitinete, dormir, checkpoint | Ciclo de noite e salvamento |

## Marco 2: MVP, jogo completo

| # | Entrega | O que você vê ou joga |
|---|---|---|
| 10 | Noite 2 | Segunda fita, caderno de locações, durex no lacre, Seu Agenor, escolhas que ficam gravadas |
| 11 | Noite 3 até a fita | Chuva, poças, rua vazia, a conversa com o Freguês, a fita sem etiqueta |
| 12 | Clímax A: esconder | O Freguês entra na loja, ouve barulho e procura. Você se esconde |
| 13 | Clímax B: fugir e orelhão | Corre pela rua molhada até o bar, ou liga do orelhão e aguenta até a viatura |
| 14 | Os 3 finais | Finais 1, 2 e 3 com epílogo. Dá para jogar do começo ao fim |

## Marco 3: Versão 1.0

| # | Entrega | O que você vê ou joga |
|---|---|---|
| 15 | Armadilha | Atrai o Freguês para o depósito e bate a porta |
| 16 | Final secreto | Resolve tudo na sexta e assiste à prisão da cadeira do bar |
| 17 | Microfone opcional | Liga o microfone no menu e o seu barulho real denuncia o esconderijo |
| 18 | Polimento | Jogo inteiro com som, luz e textos revisados. Partida completa sem softlock |
| 19 | Release | Versão para Windows, página da Steam, repositório sem arquivos do Claude |

## Onde cada personagem entra

| Personagem | Entrega |
|---|---|
| Nondas (corpo e mãos) | 4 |
| Dona Neide | 7 |
| Tião e Paçoca | 8 |
| Freguês e Cabo Moreira | 9 |
| Seu Agenor | 10 |

## Estimativas

### Duração do jogo

41 a 53 minutos em uma partida, sem contar o tempo livre no bar. Cumpre o mínimo de 40 minutos. Detalhe por noite em `01-story.md`.

### Esforço de desenvolvimento

- **19 entregas planejadas.**
- **Conte com 25 a 35 sessões.** Entregas com personagem novo (4, 7, 8, 9, 10) e as do clímax (12, 13) tendem a se dividir em duas. É uma estimativa, não uma medição: nenhuma entrega foi feita ainda.
- Em calendário: com 3 sessões por semana, de 2 a 3 meses. Com 1 por semana, de 6 a 8 meses.
- A estimativa é revista ao final do marco 1, com o ritmo real medido.

### Custo em dinheiro

- Ferramentas: zero. Todas gratuitas.
- **Publicação na Steam: taxa única de 100 dólares por jogo (Steam Direct).** Não é ferramenta, mas é o único custo previsto. Valor a confirmar na página oficial da Steamworks antes da entrega 19.

## O que fica fora do jogo

- Dirigir veículo.
- Combate.
- Modo cooperativo.
- Jogar sinuca (a mesa é cenário).
- Qualquer ambiente além de loja, rua e quitinete.
- Gancho para continuação.
