# CONTEXT.md

Fonte de verdade do projeto **Fita Sem Etiqueta**. Qualquer sessão nova começa lendo este arquivo.
Atualizar SEMPRE antes de cada commit.

Última atualização: 2026-11-11 (fim da Entrega 2 da Etapa 3).

## 1. Estado atual

- **Etapa 1 (Pesquisa sobre Fears to Fathom):** concluída e aprovada em 2026-11-11.
- **Etapa 2 (Planejamento):** concluída e aprovada em 2026-11-11.
- **Etapa 3 (Desenvolvimento):** em andamento. Entrega 1 ("Instalação e projeto vazio") e **Entrega 2 ("Filtro VHS e menu") concluídas e aprovadas em 2026-11-11.**
- **Próxima entrega: 3, "Física e interação"** (ver seção 9).
- **Código de jogo:** projeto Godot em `game/`:
  - `levels/test_room/test_room.tscn`: sala cinza de teste. Instancia o jogador, o filtro VHS e o menu de pausa.
  - `player/`: controlador de primeira pessoa. WASD anda, mouse olha.
  - `core/Settings.cs`: autoload com intensidade do filtro, sensibilidade do mouse e volume. Grava em `user://settings.cfg`.
  - `vfx/`: filtro VHS (`vhs_filter.tscn`, `VhsFilter.cs`, `vhs_filter.gdshader`). O 3D é renderizado a 480 linhas.
  - `ui/`: menu de pausa (`pause_menu.tscn`, `PauseMenu.cs`). Esc abre e fecha, pausa o jogo e solta o mouse.
  - `localization/ui.csv`: textos de interface em `pt`, `en` e `es`. O jogo usa o idioma do Windows; ainda não há troca no menu.
- **Ferramentas:** Godot, .NET SDK e extensão C# instalados (ver seção 6).
- **Repositório:** pasta local `C:\Projetos\fita-sem-etiqueta`. GitHub público: `https://github.com/SZonta/fita-sem-etiqueta`. Branch `main`.

## 2. Visão do jogo

- **Nome:** Fita Sem Etiqueta.
- Terror 3D em primeira pessoa, estilo "Fears to Fathom", com filtro VHS. Outras referências: "Murder House" e "The Convenience Store".
- **Premissa:** em junho de 1996, o atendente de uma locadora de VHS do interior paulista descobre, fita a fita, que um freguês que ninguém na cidade conhece está filmando a vida dele cada vez mais de perto.
- Ameaça HUMANA, nada sobrenatural. Sensação de "poderia acontecer comigo a qualquer momento".
- Personagens sem voz: cada um tem um "ruído" de fala, gesticula, e a legenda aparece na parte inferior da tela.
- Brasilidade explícita só pelos cenários e detalhes: cachorro caramelo, cadeiras de plástico na calçada, lanchonete que vira bar à noite com mesa de sinuca, bandeirinhas de festa junina, orelhão, porta de aço.
- **Episódio único.** História completa e independente, não uma antologia. Sem ganchos, sem estrutura de temporada.
- Protagonista: **Epaminondas ("Nondas")**, 19 anos.
- Destino: Steam (Windows). Código em repositório público no GitHub (conta `SZonta`).

## 3. Requisitos

- Mais de um final (definidos: 3 + 1 secreto).
- Duração mínima de 40 minutos (estimada: 41 a 53).
- Três idiomas com troca dentro do jogo: português, inglês e espanhol. Localização planejada desde o início.
- Jogo curto, mas caprichado: pequenos detalhes importam.
- Somente ferramentas gratuitas.

### Mecânicas de imersão exigidas

- Objetos reagem fisicamente quando o personagem esbarra (ex.: lata).
- Corpo em primeira pessoa: ao olhar para baixo, o jogador vê as próprias pernas.
- Poça d'água ondula ao ser pisada.
- Animação da mão se esticando para pegar itens.

## 4. Decisões da Etapa 2

Os três documentos abaixo são a referência completa. Esta seção é só o resumo.

| Documento | Conteúdo |
|---|---|
| `docs/design/01-story.md` | História, personagens, roteiro das 3 noites, clímax, finais |
| `docs/design/02-stack-architecture.md` | Stack, arquitetura, pipeline de arte, riscos |
| `docs/design/03-mvp.md` | 19 entregas em 3 marcos, estimativas |

### História

| Assunto | Decisão |
|---|---|
| Local | Dois Córregos (SP), cidade real. Rua, locadora e bar fictícios |
| Datas | Quinta 13, sexta 14 e sábado 15 de junho de 1996, época de festa junina |
| Ambientes | Astro Vídeo Locadora, um quarteirão de rua com o Bar do Tião, quitinete nos fundos |
| Antagonista | "O Freguês": cliente obcecado que ninguém conhece. Sem nome, profissão ou endereço sabidos |
| Estrutura | 3 noites curtas. A noite avança por missão concluída, não por relógio |
| Refúgio | No bar, sentar na cadeira de plástico e beber cerveja, com o tempo parado |
| Clímax | 4 saídas, sem combate: esconder, fugir, pedir ajuda (orelhão), armadilha (depósito) |
| Finais | "Rebobinado", "Palavra contra palavra", "Fita sem etiqueta" e o secreto "Sexta-feira" |
| Elenco | Nondas, o Freguês, Seu Agenor, Cabo Moreira, Tião, Dona Neide e o cachorro Paçoca |
| Paçoca | Alarme do jogador. Nunca se machuca |

### Stack

| Assunto | Decisão |
|---|---|
| Motor e linguagem | Godot 4.7.2 (edição .NET) com C#. Projeto em `net10.0`, e não no `net8.0` padrão do Godot, porque o suporte ao .NET 8 terminou em 2026-11-10 |
| Renderizador | Forward+ com o driver padrão (Vulkan). Cerca de 495 quadros por segundo a 1080p na sala de teste |
| Física | Jolt Physics (embutido no Godot) |
| Estilo visual | Semi-realista sob filtro VHS, com resolução interna baixa (cerca de 480 linhas) |
| Resolução interna | Só o 3D é reduzido (`Scaling3DScale` = 480 / altura da janela). Menus e textos ficam na resolução da janela, com esticamento `canvas_items` (base 1280x720) |
| Filtro VHS | Um shader de tela cheia. Aparência de câmera antiga, **sem nada que pisque ou corra pela tela** (escolha do dono). Cerca de 595 quadros por segundo a 1080p na sala de teste |
| Interpolação de física | Ligada. A cabeça do jogador é destacada do corpo e posicionada a cada quadro |
| Cenário e objetos | 100% próprios, gerados por script Python no Blender. Texturas procedurais |
| Personagens humanos | Malha base de gerador gratuito (MPFB/MakeHuman) + rosto, roupa, textura e animação próprios |
| Áudio | Efeitos genéricos de acervos gratuitos, **somente licença CC0**, registrados em `docs/credits.md`. "Ruídos de fala" próprios |
| Microfone | Opcional, desligado por padrão, última entrega de mecânica |

### Licença (decidida em 2026-11-11)

| Conteúdo | Licença | Arquivo |
|---|---|---|
| Código-fonte: C#, shaders, configuração de projeto e de build, testes | MIT | `LICENSE` |
| Todo o resto: modelos, texturas, áudio, cenas, textos, história, `docs/`, nome do jogo e os scripts de `tools/blender/` | Todos os direitos reservados | `LICENSE-ASSETS.md` |

- Titular: `SZonta`.
- Motivo: o jogo será vendido na Steam. O código fica útil para quem estuda; a arte e a história não podem ser redistribuídas.
- Os scripts do Blender ficam do lado reservado porque são a fonte da arte: em MIT, qualquer pessoa regeneraria os modelos.
- O conteúdo reservado pode ser visto para estudo. A licença **não** autoriza compilar o jogo com essa arte para uso pessoal (decisão do dono: o jogo será vendido).
- Material de terceiros mantém a licença original e é listado em `docs/credits.md`.

### Arquitetura (resumo)

- Cena do Godot = componente React. Sinal = evento. `[Export]` = props. Autoload = serviço singleton.
- Composição em vez de herança. Chamadas descem, sinais sobem.
- `GameState` é a única fonte de verdade da história (como uma store do Redux).
- Lógica de história em C# puro, sem tipos do Godot, testada com xUnit.
- Texto nunca fica no código: sempre chave de tradução.
- Sistema de ruído único: lata, poça, corrida e microfone geram o mesmo evento, que o Freguês ouve no clímax.
- 4 autoloads: `GameState`, `Settings`, `SaveService`, `LevelLoader`. Só o `Settings` existe até agora.
- Projeto Godot em `game/`, testes em `tests/`, scripts do Blender em `tools/blender/`.

### Agentes (`./.claude/agents/`)

| Agente | Papel |
|---|---|
| `researcher` | Fatos de época, documentação oficial, licenças. Só fontes confiáveis, com grau de confiança |
| `reviewer` | Revisão sem editar, antes do OK do dono. Softlock em primeiro lugar |
| `artist` | Scripts do Blender, materiais, shaders |
| `language` | Falas curtas em português e tradução para inglês e espanhol |

A sessão principal orquestra, desenvolve e faz o commit. Não existem agentes Orchestrator, Developer nem Commiter: um subagente não consegue chamar outros, um Developer separado perderia o contexto da conversa com o dono, e o commit é um comando cujas regras estão no `CLAUDE.md`. Todos os agentes herdam o modelo e o esforço da sessão.

### MVP

| Marco | Entregas | Resultado |
|---|---|---|
| 1. Noite 1 jogável | 1 a 9 | 12 a 15 minutos de jogo de verdade |
| 2. MVP: jogo completo | 10 a 14 | 3 noites, 3 saídas no clímax, 3 finais. Publicável |
| 3. Versão 1.0 | 15 a 19 | Armadilha, final secreto, microfone, polimento, release |

Estimativa de esforço: 19 entregas planejadas, 25 a 35 sessões. Rever ao fim do marco 1.

## 5. Pesquisa: Fears to Fathom (Etapa 1)

Relatório completo, com fontes e grau de confiança: `docs/research/01-fears-to-fathom.md`.

Fórmula comum da série: rotina banal → sinais de estranheza → pistas falsas → invasão do espaço seguro → esconder-se em silêncio → fuga roteirizada → epílogo em texto.

### Lições que valem como regra de design

1. **Rotina com propósito.** Cada tarefa carrega história, tensão ou brasilidade. Nada de enchimento. Primeiro sinal de estranheza nos primeiros 5 minutos.
2. **Clímax justo.** Mais de uma solução válida, sinais claros, checkpoint imediatamente antes. Falhar pode levar a outro final em vez de reiniciar a cena.
3. **Checkpoints generosos e menu de pausa/opções desde a primeira versão jogável** (volume, sensibilidade, tamanho da legenda, idioma, intensidade do VHS).
4. **Toda preparação tem recompensa.** Objetos, informações e escolhas precisam pesar no desfecho.
5. **Escopo pequeno e polido.** Bugs e softlocks são a reclamação número 1 da série. Poucas mecânicas, todas bem acabadas. Sem direção de veículo. Um jogador só.
6. **Sustos raros**, nunca o mesmo duas vezes. Tensão prolongada vale mais.
7. **Microfone opcional e desligável.**
8. **Falas curtas**, para compensar a falta de voz.
9. **Epílogo coerente** com o que o jogador fez.
10. **Duração-alvo de 40 a 60 minutos**, com preço compatível.
11. **Originalidade:** mesmo gênero e técnicas, mas nome, personagens, história e arte próprios.

## 6. Ambiente do dono (verificado em 2026-11-11)

Windows 11. GPU NVIDIA RTX 3050 Laptop (4 GB), 16 GB de RAM, 353 GB livres em C:.

| Ferramenta | Situação |
|---|---|
| Git 2.56 | Instalado |
| GitHub CLI 2.103 | Instalado, autenticado como `SZonta` |
| winget | Instalado |
| VS Code 1.146 | Instalado |
| Godot 4.7.2, edição .NET | Instalado em 2026-11-11 em `C:\Tools\Godot\4.7.2\Godot_v4.7.2-stable_mono_win64\` (zip portátil, SHA-512 conferido) |
| .NET SDK 10.0.404 (LTS) | Instalado em 2026-11-11 pelo winget. Runtimes 8 e 10 também presentes |
| Extensão C# do VS Code 2.160.4 | Instalada em 2026-11-11, com a dependência ".NET Install Tool" |
| Pacotes NuGet do Godot 4.7.2 | Em `~/.nuget/packages`, baixados do nuget.org pelo `dotnet restore`. O dono aceitou manter |
| Blender, Python, Audacity | Não instalados |

- A GPU usada pelo Godot é a NVIDIA, não a Intel integrada (conferido no log).
- Telemetria do .NET desligada na conta do dono (`DOTNET_CLI_TELEMETRY_OPTOUT=1`), a pedido dele.

### Como abrir, rodar e compilar

- Abrir o editor: `C:/Tools/Godot/4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe --editor --path C:/Projetos/fita-sem-etiqueta/game`. Depois, F5.
- Compilar o C# sem abrir o editor: `dotnet build game/FitaSemEtiqueta.sln`.
- **Não usar `--headless --build-solutions --quit`:** nesta versão o processo compila, mas não encerra sozinho.
- Para rodar o jogo pela linha de comando, usar o executável `_console.exe` com `--path game`. Ele abre uma janela e captura o mouse do dono.
- **Só abrir a janela do jogo depois que o dono escrever "pode rodar".** Terminar antes tudo o que não precisa da janela, dizer quantas execuções e quanto tempo, encerrar o turno e esperar. Um aviso seguido da execução não basta: o dono recusou esse formato. Cada liberação vale só para as execuções anunciadas. Sempre usar `timeout`.
- Comandos sem janela não precisam de liberação: `dotnet build` e a importação abaixo.
- Depois de criar ou alterar um arquivo que o Godot importa (como o CSV de tradução): `..._console.exe --headless --import --path game`. Encerra sozinho.
- Para conferir a imagem sem o dono: `--write-movie <pasta>/frame.png --fixed-fps 30 --quit-after 45` grava os quadros em PNG.
- Medição de quadros por segundo: `--fullscreen --disable-vsync --print-fps`, com `timeout 20`. A tela do dono é 1920x1080, 120 Hz ou mais.
- Shader só é compilado com a janela aberta. Erro de shader não aparece no `dotnet build` nem no modo `--headless`.

### A instalar nas próximas entregas

**Toda instalação exige autorização explícita do dono**, uma por vez, informando o que é, a origem, o tamanho confirmado na fonte e o motivo. Dependências baixadas automaticamente por um comando (como pacotes NuGet) também precisam ser avisadas antes.

| Ferramenta | Origem oficial | Primeira entrega que precisa |
|---|---|---|
| Blender (LTS atual) | blender.org | 3 ou 4 |
| MPFB (complemento do Blender) | makehumancommunity.org | 4, depois de confirmar a licença |
| Audacity | audacityteam.org | 7 |

## 7. Convenções

- Respostas ao dono em português do Brasil.
- Código, commits, nomes de arquivos, branches e comentários em inglês.
- Documentos de contexto (`CONTEXT.md`, `docs/`, `.claude/agents/`) em português.
- Identidade git configurada SOMENTE neste repositório: `SZonta` com e-mail noreply do GitHub. Não existe `.gitconfig` global na máquina e não deve ser criado por este projeto.
- Branch principal: `main`.
- 1 entrega = 1 sessão: desenvolver → testar → OK do dono → atualizar este arquivo → commit → push → prompt para a próxima sessão.
- **Decisão de aparência (shader, cor, estilo de interface):** quando o dono não gostar ou estiver em dúvida, mostrar de 3 a 5 opções lado a lado, mais a atual, numa página de teste descartável, sem mexer no jogo. Ele escolhe ou pede uma mistura; só a aprovada é levada para o Godot. Funcionou na entrega 2 com uma página HTML/WebGL aberta no painel do navegador do app. A cena de teste precisa ter cor: a sala cinza esconde quase todo o efeito do VHS.
- Nada de contexto fora do repositório: anotações de sessão ficam neste arquivo, não na memória global do Claude.

## 8. Pendências e decisões em aberto

- **Licença do MPFB/MakeHuman não confirmada.** A página consultada não informa. Confirmar na fonte oficial antes da entrega 4. Plano B: personagens próprios mais simples.
- **Textura procedural** pode parecer artificial em material brasileiro (chapisco, caquinho). Validar na entrega 5; se não convencer, reabrir a decisão com o dono.
- **Lata da entrega 3:** ligar `continuous_cd` no corpo rígido. No teste da entrega 1, objetos pequenos e rápidos atravessaram um piso fino nos dois motores de física.
- **Corpo do Nondas (entrega 4) e interpolação:** a cabeça gira a cada quadro, direto do mouse; o corpo gira a 60 ticks por segundo, interpolado. Conferir se as pernas parecem atrasar em relação à câmera ao girar rápido.
- **Teleporte do jogador (troca de fase, checkpoint):** com a interpolação ligada, chamar `ResetPhysicsInterpolation()` depois de mover o jogador, senão ele desliza de um ponto ao outro.
- **Filtro VHS validado só na sala cinza e na página de teste.** A parte que o dono mais gostou (cor vazando) quase não aparece sem cor. Conferir de novo com ele na entrega 5, na locadora.
- **Defeitos fortes de fita** (faixa de tracking, riscos brancos, tremor): fora do filtro geral por escolha do dono. Candidatos para as fitas assistidas na TV (entrega 6). A receita está em `docs/design/02-stack-architecture.md` só como descrição; o código das opções recusadas não foi guardado.
- **Menu de pausa acima do filtro:** decisão minha, para o menu continuar legível com o filtro no máximo. O dono não opinou. Inverter é trocar o número da camada.
- **Filtro e menu instanciados em `test_room.tscn`:** quando o `LevelLoader` existir, eles precisam ficar num lugar que sobreviva à troca de fase.
- **Volume:** o controle já atua no barramento principal, mas não há som para ouvir. Conferir na primeira entrega com áudio.
- **Menu de pausa mínimo:** só os três controles e "Continuar". Faltam, por lição 3 da pesquisa: tamanho da legenda e idioma (entrega 7), sair do jogo, navegação por teclado.
- **Janela 4K:** o código limita a escala 3D a 0,25, então em 2160 linhas o 3D fica com 540, não 480. O limite real do motor pode ser 0,1 (não conferido). Para conferir, na próxima execução liberada: atribuir 0,2 e imprimir `GetWindow().Scaling3DScale`.
- **Alt+Tab com o menu fechado (não testado):** o clique que recapturava o mouse saiu nesta entrega. Pedir ao dono um teste de 30 segundos: jogar, Alt+Tab, voltar, mexer o mouse. Se o mouse não voltar capturado, recapturar ao recuperar o foco.
- **Pausa, pontos a vigiar (achados do reviewer na entrega 2):**
  - Fechar o menu sempre captura o mouse. Quando existir tela com mouse solto (menu principal, epílogo), guardar o modo anterior e restaurar.
  - Trocar de fase com o menu aberto deixaria a árvore pausada sem menu. Tratar junto com o `LevelLoader`.
  - O jogo não pausa ao perder o foco da janela. Passa a importar quando houver ameaça (entrega 9).
- **`settings.cfg` sem validação:** valores editados à mão fora da faixa entram como estão (por exemplo, sensibilidade 0). Mexer no controle corrige. Não tratado de propósito: só acontece editando o arquivo.
- **Padrão do filtro: 0,7, decidido pelo dono em 2026-11-11.** No teste ele deixou o próprio controle em 0,3 e a sensibilidade em 0,8, mas escolheu manter o padrão do jogo. Não perguntar de novo.
- **Cenas sem `uid`:** `player.tscn`, `test_room.tscn`, `vhs_filter.tscn` e `pause_menu.tscn` foram escritas à mão. Na primeira vez que o editor salvar, ele reescreve o cabeçalho; esse diff é esperado.
- **`docs/credits.md`** ainda não existe. Criar junto com o primeiro item de terceiros.
- **Driver de vídeo:** o Direct3D 12 mediu 19% a mais que o Vulkan na sala vazia. Reavaliar na entrega 5, com uma cena de verdade.
- **Exportação para Windows com `net10.0`:** não validada. Validar na entrega 19 (ou antes, se alguma entrega precisar exportar).
- **Tamanhos de download** das ferramentas: confirmar na fonte no momento de pedir cada autorização.
- **Fatos de época a verificar pelo `researcher`** (lista completa no fim de `docs/design/01-story.md`): Dois Córregos em 1996, orelhão (ficha ou cartão, ligação de emergência), preço de locação, filmadora doméstica e carimbo de data, viatura e farda da PM.
- **Taxa de publicação da Steam** (100 dólares por jogo, citada de memória): confirmar na página oficial antes da entrega 19.
- **Nome do jogo:** a busca por jogos homônimos na Steam foi rápida. Não é verificação de marca registrada.
- Os enredos dos episódios 2 e 3 de Fears to Fathom têm confiança média. Confirmar por gameplay antes de usar algum detalhe como referência direta.
- Escopo no teto (6 personagens, armadilha, final secreto, microfone, visual semi-realista). A ordem do MVP deixa os itens cortáveis por último.

## 9. Próxima entrega

**Entrega 3: Física e interação.**

- O dono joga: chuta uma lata, pisa numa poça que ondula, pega um objeto. São três das quatro mecânicas de imersão (a mão que se estica é da entrega 4).
- Passos:
  1. Lata: `RigidBody3D` com `continuous_cd` ligado (pendência da seção 8), empurrada quando o jogador esbarra. O `CharacterBody3D` não empurra corpos rígidos sozinho: aplicar impulso nas colisões do `MoveAndSlide`.
  2. Poça: superfície com shader de ondulação em `game/vfx/`, disparada na posição em que o jogador pisa.
  3. Sistema Interaction em `game/interaction/`: raio que sai da câmera, interface `IInteractable`, e um objeto que pode ser pegado.
  4. Tudo na sala de teste. Dar alguma cor aos objetos novos, para o filtro VHS aparecer.
- Fora desta entrega: sistema de ruído (só no clímax, entrega 12), animação da mão e corpo (entrega 4), som (não há áudio ainda), inventário além de "segurar um objeto".
- A câmera fica em `Player/Head/Camera3D`. A cabeça é `top_level`: a posição dela no mundo vem de `_Process`, não da hierarquia.
- Texto na tela (por exemplo, a dica de "pegar") entra por chave em `game/localization/ui.csv`, seguido da importação sem janela (seção 6).
- Instalação: nenhuma, se lata, poça e objeto forem feitos com formas primitivas do Godot. O Blender só entra se o dono quiser modelos de verdade já nesta entrega; perguntar antes.
- Pronto quando: o dono chuta a lata e ela rola sem atravessar o chão, vê a poça ondular ao pisar, e pega e solta um objeto.

## 10. Release final (tarefas obrigatórias)

- Remover do repositório todos os arquivos relacionados ao Claude (`CLAUDE.md`, `.claude/`, `CONTEXT.md` e semelhantes), mantendo somente o que pertence ao jogo.
