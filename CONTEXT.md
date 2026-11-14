# CONTEXT.md

Fonte de verdade do projeto **Fita Sem Etiqueta**. Qualquer sessão nova começa lendo este arquivo.
Atualizar SEMPRE antes de cada commit.

Última atualização: 2026-11-13 (fim da Entrega 4, parte 1: pernas).

## 1. Estado atual

- **Etapa 1 (Pesquisa sobre Fears to Fathom):** concluída e aprovada em 2026-11-11.
- **Etapa 2 (Planejamento):** concluída e aprovada em 2026-11-11.
- **Etapa 3 (Desenvolvimento):** em andamento. Entregas 1 ("Instalação e projeto vazio") e 2 ("Filtro VHS e menu") concluídas e aprovadas em 2026-11-11. Entrega 3 ("Física e interação") concluída e aprovada em 2026-11-12. **Entrega 4 ("Corpo do Nondas") dividida em duas sessões: parte 1 (pernas) concluída e aprovada em 2026-11-13.**
- **Próxima entrega: 4, parte 2, "Mão do Nondas"** (ver seção 9).
- **Código de jogo:** projeto Godot em `game/`:
  - `levels/test_room/test_room.tscn`: sala cinza de teste. Instancia o jogador, o filtro VHS e o menu de pausa. Tem uma lata vermelha, uma poça e uma caixa amarela do tamanho de uma fita VHS em cima do caixote.
  - `player/`: controlador de primeira pessoa (`PlayerController`). WASD anda a 2 m/s, mouse olha. A cena do jogador tem o corpo visível (`Body`, com o script `PlayerBody`), o raio de interação, o ponto da mão e a dica na tela.
  - `player/PlayerBody.cs`: corpo do Nondas visto de dentro. Pernas e braços animados pela direção e pela velocidade; cabeça desenhada só como sombra; uma área do tamanho do sapato em cada pé, que chuta objetos soltos e faz a poça ondular.
  - `assets/models/nondas.glb`: modelo do Nondas (malha do MPFB, CC0), gerado por script do Blender que fica num repositório privado (ver seção 4, Licença).
  - `interaction/`: `Interactor` (raio da câmera, tecla E), interface `IInteractable` e `Pickable` (objeto que se pega e solta).
  - `core/Settings.cs`: autoload com intensidade do filtro, sensibilidade do mouse e volume. Grava em `user://settings.cfg`.
  - `vfx/`: filtro VHS (`vhs_filter.tscn`, `VhsFilter.cs`, `vhs_filter.gdshader`). O 3D é renderizado a 480 linhas. Poça que ondula (`puddle.tscn`, `Puddle.cs`, `puddle.gdshader`).
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
| Camadas de física | 1 `world`, 2 `player`, 3 `props` (objetos soltos), 4 `feet` (pés do jogador). O jogador não colide com objetos soltos: ele os chuta com os pés. Tabela completa em `docs/design/02-stack-architecture.md` |
| Corpo do jogador | Modelo próprio sobre a malha do MPFB, visível em primeira pessoa. Animações geradas por script (parado e andando nas 4 direções). Detalhes em `docs/design/02-stack-architecture.md`, decisão da entrega 4 |
| Interação | Tecla E. Raio de 2 m que sai da câmera; o alvo implementa `IInteractable`. Um item na mão por vez, sem inventário. A dica na tela fica acima do filtro VHS |
| Cenário e objetos | 100% próprios, gerados por script Python no Blender. Texturas procedurais |
| Personagens humanos | Malha base de gerador gratuito (MPFB/MakeHuman) + rosto, roupa, textura e animação próprios. Licença confirmada em 2026-11-12: os assets do MPFB são CC0 (só os pacotes CC0 são usados); o código do complemento é GPLv3 e não vai para o jogo. Registro em `docs/credits.md` |
| Áudio | Efeitos genéricos de acervos gratuitos, **somente licença CC0**, registrados em `docs/credits.md`. "Ruídos de fala" próprios |
| Microfone | Opcional, desligado por padrão, última entrega de mecânica |

### Licença (decidida em 2026-11-11)

| Conteúdo | Licença | Arquivo |
|---|---|---|
| Código-fonte: C#, shaders, configuração de projeto e de build, testes | MIT | `LICENSE` |
| Todo o resto: modelos, texturas, áudio, cenas, textos, história, `docs/`, nome do jogo | Todos os direitos reservados | `LICENSE-ASSETS.md` |
| Scripts do Blender que geram a arte (`tools/blender/`) | Não publicados | Repositório **privado** `SZonta/fita-sem-etiqueta-art` |

- Titular: `SZonta`.
- Motivo: o jogo será vendido na Steam. O código fica útil para quem estuda; a arte e a história não podem ser redistribuídas.
- **Scripts do Blender fora do repositório público (decisão do dono, 2026-11-12).** O FAQ do blender.org diz que scripts publicados que usam a API do Blender devem ser GPL (confiança média; a questão jurídica é aberta). GPL não serve: o script é a fonte da arte, e qualquer pessoa poderia gerar e usar os modelos. Solução: `tools/blender/` é um repositório git próprio, aninhado na pasta do projeto e ignorado pelo `.gitignore` do público. Os `.glb` gerados continuam no público. Commit e push dos scripts são feitos dentro de `tools/blender/`, com a mesma identidade git local.
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
- Projeto Godot em `game/`, testes em `tests/`, scripts do Blender em `tools/blender/` (repositório privado aninhado).

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
| Blender 5.2.4 LTS | Instalado em 2026-11-12 em `C:\Tools\Blender\5.2.4\blender-5.2.4-windows-x64\` (zip portátil de download.blender.org, 405.779.142 bytes, SHA-256 conferido). **Modo portátil:** a pasta `portable\` ao lado do `blender.exe` guarda configurações e extensões; nada vai para o `%APPDATA%`. Traz o próprio Python 3.13 |
| MPFB 2.0.18 (complemento do Blender) | Instalado em 2026-11-12 dentro da pasta portátil do Blender (extensions.blender.org, 45.956.992 bytes, SHA-256 conferido). Acesso à rede desligado. O pacote extra de assets (267 MB) **não** foi necessário |
| Python avulso, Audacity | Não instalados |

- Os zips baixados do Blender e do MPFB (cerca de 430 MB) ficaram em `C:\Tools\Blender\download\`. Podem ser apagados; servem só para reinstalar sem baixar de novo.

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
- O dono testa pelo editor, com F5.
- **Teste sem janela (não precisa de liberação):** `..._console.exe --headless --path game --script <arquivo.gd>`, com `timeout`. O arquivo é um GDScript descartável, fora do repositório, que estende `SceneTree`, carrega a sala, simula teclas com `Input.action_press` e imprime posições. A física, os autoloads e o C# rodam normalmente. Shader e imagem não. Foi assim que a entrega 3 achou três defeitos antes de o dono jogar.
- Para conferir a imagem sem o dono: `--write-movie <pasta>/frame.png --fixed-fps 30 --quit-after 45` grava os quadros em PNG. Aceita `--script` junto, para gravar um roteiro. **Cada quadro custa cerca de 0,18 s para gravar:** 280 quadros levaram 55 s. Estimar o tempo por esse número ao pedir a liberação.
- Medição de quadros por segundo: `--fullscreen --disable-vsync --print-fps`, com `timeout 20`. A tela do dono é 1920x1080, 120 Hz ou mais.
- Shader só é compilado com a janela aberta. Erro de shader não aparece no `dotnet build` nem no modo `--headless`.
- **`--fixed-fps 60` no teste sem janela** deixa cada quadro com exatamente um tick de física: os números saem iguais a cada execução.
- Um roteiro `--script` que estende `SceneTree` precisa devolver `false` no fim do `_process`. O nó só fica utilizável a partir do primeiro quadro, e não no `_initialize`.
- Propriedade C# de tipo que não é Variant (como `IReadOnlyList<Area3D>`) não aparece para o GDScript dos roteiros.

### Blender (gerar modelos)

- Rodar um script: `C:/Tools/Blender/5.2.4/blender-5.2.4-windows-x64/blender.exe --background --python tools/blender/<script>.py`, a partir da raiz do projeto. Não abre janela e não precisa de liberação. O corpo do Nondas leva cerca de 4 s.
- Depois de exportar um `.glb`: `..._console.exe --headless --import --path game`.
- **Ver a aparência sem abrir o jogo:** renderizar no Blender (EEVEE em `--background` funciona) com uma câmera na altura e no campo de visão do jogo (75° na vertical, 854x480). Foi assim que o dono escolheu a roupa.
- **Página de comparação no painel do navegador:** arquivo fora da pasta do projeto vira foto estática (sem JavaScript). Arquivo dentro do projeto abre como endereço `data:`, onde imagens com caminho relativo não carregam, e páginas grandes são recusadas. O que funcionou: servir uma pasta temporária com `python.exe -m http.server` (o Python do Blender) por uma entrada em `.claude/launch.json`, com as imagens em JPEG. Apagar a pasta e a entrada depois.
- API do MPFB usada por script: `HumanService.create_human`, `TargetService.bake_targets`, `HumanService.add_builtin_rig(body, "game_engine")`. O módulo é `bl_ext.user_default.mpfb`; ligar com `addon_utils.enable` **depois** de `read_factory_settings`.
- **Ação nova estraga a primeira chave** se já existir uma faixa de NLA tocando: criar as faixas de NLA só depois de gravar todas as ações.

### A instalar nas próximas entregas

**Toda instalação exige autorização explícita do dono**, uma por vez, informando o que é, a origem, o tamanho confirmado na fonte e o motivo. Dependências baixadas automaticamente por um comando (como pacotes NuGet) também precisam ser avisadas antes.

| Ferramenta | Origem oficial | Primeira entrega que precisa |
|---|---|---|
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

- **Mão (entrega 4, parte 2):** o item na mão ainda salta direto para o `HoldPoint`, filho da câmera. Olhando 89° para baixo, ele fica dentro da barriga ou do braço (achado do `reviewer`). Os braços do modelo pendem ao lado do corpo e ainda não alcançam nada.
- **Corpo 32 cm atrás da câmera:** sem isso, olhando para baixo, aparece o buraco do pescoço. Efeito colateral: andando de costas até a parede, os calcanhares entram uns 8 cm nela. Só se vê virando a câmera para trás. O chute por trás da parede foi bloqueado por um raio (achado do `reviewer`).
- **Chute pelos pés:** a lata só voa quando o sapato encosta nela. Uma lata centrada entre os pés (30 cm de distância entre eles) não é chutada; as pernas passam por cima. O pé sobe 10 cm na passada e às vezes passa por cima da lata (12 cm), e quem chuta é o calcanhar, um pouco tarde. Se o dono reclamar: baixar a passada para 5 cm (mais realista) e ajustar o `TouchHeight` da poça junto.
- **Passo para o lado:** o pé desliza cerca de 60%, porque o passo lateral é curto (para os pés não se cruzarem) e o ritmo é o da caminhada para a frente. O dono aprovou assim.
- **`TouchHeight` da poça (10 cm):** objeto solto com centro acima de 10 cm (uma garrafa em pé, por exemplo) nunca faz a poça ondular. A altura é medida em metros no mundo, então a poça pode ser escalada.
- **Quadros por segundo com o personagem:** não medidos. Medir na entrega 5, com a locadora.
- **Cores e tom de pele do Nondas:** camiseta clara, jeans, Kichute preto, pele parda (escolhas do dono). As cores são provisórias e mudam numa linha do script. Ainda não há textura; o rosto não aparece.
- **Textura procedural** pode parecer artificial em material brasileiro (chapisco, caquinho). Validar na entrega 5; se não convencer, reabrir a decisão com o dono.
- **Todo objeto solto pequeno precisa de `continuous_cd`** (lata e fita já têm). No teste da entrega 1, objetos pequenos e rápidos atravessaram um piso fino nos dois motores de física.
- **Todo objeto solto novo:** `collision_layer = 4` (camada 3, `props`) e `collision_mask = 5` (mundo e outros objetos). Na camada errada, o jogador volta a subir em cima dele.
- **O jogador atravessa objetos soltos** (decisão da entrega 3). Um objeto que precise barrar o caminho deve ficar na camada `world`.
- **Chute determinístico:** a lata tomba sempre para a frente e segue deitada. Se parecer repetitivo na rua (entrega 8), variar o giro.
- **Lata e fita estão escritas dentro de `test_room.tscn`.** Viram cenas próprias quando forem usadas em outro lugar (fita na entrega 6, lata na 8).
- **Item na mão e troca de fase:** o `Pickable` guarda o pai original para voltar a ele ao ser solto. Se a fase for trocada com o item na mão, esse pai não existe mais: soltar lança erro e o `Interactor` fica preso em "Soltar" (achado do `reviewer`). No caso inverso, se o jogador for recarregado com o item na mão, o item é apagado junto, porque é filho do ponto da mão. Tratar os dois junto com o `LevelLoader`.
- **Item de missão pode ser chutado para longe** (achado do `reviewer`): a fita é chutável e não há reposição se ela parar num vão estreito demais para o jogador ou cair fora do cenário. Na sala de teste todo ponto é alcançável. Decidir na entrega 6, quando a fita virar item de missão.
- **`Interactor` busca o ponto da mão como irmão** (`"../HoldPoint"`), um desvio leve de "chamadas descem". Na entrega 4, parte 2, que mexe nesse ponto por causa da mão, tornar o `HoldPoint` filho do `Interactor`.
- **Poça no editor:** sem o jogo rodando, o shader mostra anéis parados no centro da poça (o script que os zera não roda no editor). No jogo não aparece.
- **Texto da dica com a tecla fixa** ("[E] Pegar"). Se um dia houver troca de teclas, a tecla sai do texto.
- **Dica acima do filtro VHS:** decisão minha, igual à do menu de pausa. O dono viu e não opinou.
- **Objeto pequeno afunda alguns milímetros no piso** em repouso (tolerância de penetração padrão do Jolt, 2 cm). Não se vê a 480 linhas. Baixar `physics/jolt_physics_3d/simulation/penetration_slop` resolve, mas não foi adotado por ser um ajuste global sem necessidade visível.
- **Teleporte do jogador (troca de fase, checkpoint):** com a interpolação ligada, chamar `ResetPhysicsInterpolation()` depois de mover o jogador, senão ele desliza de um ponto ao outro.
- **Filtro VHS validado só na sala cinza e na página de teste.** A parte que o dono mais gostou (cor vazando) quase não aparece sem cor. Conferir de novo com ele na entrega 5, na locadora.
- **Defeitos fortes de fita** (faixa de tracking, riscos brancos, tremor): fora do filtro geral por escolha do dono. Candidatos para as fitas assistidas na TV (entrega 6). A receita está em `docs/design/02-stack-architecture.md` só como descrição; o código das opções recusadas não foi guardado.
- **Menu de pausa acima do filtro:** decisão minha, para o menu continuar legível com o filtro no máximo. O dono não opinou. Inverter é trocar o número da camada.
- **Filtro e menu instanciados em `test_room.tscn`:** quando o `LevelLoader` existir, eles precisam ficar num lugar que sobreviva à troca de fase.
- **Volume:** o controle já atua no barramento principal, mas não há som para ouvir. Conferir na primeira entrega com áudio.
- **Menu de pausa mínimo:** só os três controles e "Continuar". Faltam, por lição 3 da pesquisa: tamanho da legenda e idioma (entrega 7), sair do jogo, navegação por teclado.
- **Janela 4K (resolvido na entrega 3):** o limite real da escala 3D é 0,1. O piso no código passou de 0,25 para 0,1, então em 2160 linhas o 3D fica com 480. Não visto numa tela 4K de verdade: a do dono é 1080p.
- **Pausa, pontos a vigiar (achados do reviewer na entrega 2):**
  - Fechar o menu sempre captura o mouse. Quando existir tela com mouse solto (menu principal, epílogo), guardar o modo anterior e restaurar.
  - Trocar de fase com o menu aberto deixaria a árvore pausada sem menu. Tratar junto com o `LevelLoader`.
  - O jogo não pausa ao perder o foco da janela. Passa a importar quando houver ameaça (entrega 9).
- **`settings.cfg` sem validação:** valores editados à mão fora da faixa entram como estão (por exemplo, sensibilidade 0). Mexer no controle corrige. Não tratado de propósito: só acontece editando o arquivo.
- **Padrão do filtro: 0,7, decidido pelo dono em 2026-11-11.** No teste ele deixou o próprio controle em 0,3 e a sensibilidade em 0,8, mas escolheu manter o padrão do jogo. Não perguntar de novo.
- **Cenas sem `uid`:** `player.tscn`, `test_room.tscn`, `vhs_filter.tscn`, `pause_menu.tscn` e `puddle.tscn` foram escritas à mão. Na primeira vez que o editor salvar, ele reescreve o cabeçalho; esse diff é esperado.
- **Driver de vídeo:** o Direct3D 12 mediu 19% a mais que o Vulkan na sala vazia. Reavaliar na entrega 5, com uma cena de verdade.
- **Exportação para Windows com `net10.0`:** não validada. Validar na entrega 19 (ou antes, se alguma entrega precisar exportar).
- **Tamanhos de download** das ferramentas: confirmar na fonte no momento de pedir cada autorização.
- **Fatos de época a verificar pelo `researcher`** (lista completa no fim de `docs/design/01-story.md`): Dois Córregos em 1996, orelhão (ficha ou cartão, ligação de emergência), preço de locação, filmadora doméstica e carimbo de data, viatura e farda da PM.
- **Taxa de publicação da Steam** (100 dólares por jogo, citada de memória): confirmar na página oficial antes da entrega 19.
- **Nome do jogo:** a busca por jogos homônimos na Steam foi rápida. Não é verificação de marca registrada.
- Os enredos dos episódios 2 e 3 de Fears to Fathom têm confiança média. Confirmar por gameplay antes de usar algum detalhe como referência direta.
- Escopo no teto (6 personagens, armadilha, final secreto, microfone, visual semi-realista). A ordem do MVP deixa os itens cortáveis por último.

## 9. Próxima entrega

**Entrega 4, parte 2: Mão do Nondas.**

- O dono joga: ao pegar a fita, vê o braço direito do Nondas se esticar até ela e trazê-la para a mão; ao soltar, a mão abre e o braço volta. É a quarta mecânica de imersão.
- Já existe (parte 1): o modelo `nondas.glb` com esqueleto `game_engine` (ossos `upperarm_r`, `lowerarm_r`, `hand_r` e dedos), braços pendendo ao lado do corpo, o script gerador em `tools/blender/nondas_body.py` (repositório privado) e o `PlayerBody`, que monta a `AnimationTree` em código.
- Passos:
  1. Decidir com o dono como o braço alcança o item. Opções: cinemática inversa em tempo real no Godot (o Godot 4.7 tem modificadores de esqueleto) ou animação feita no Blender. A primeira alcança qualquer ponto; a segunda é mais simples, mas é uma pose só. Recomendação a confirmar na documentação oficial.
  2. Ponto de encaixe: `Interactor.Hold` e `Pickable.AttachTo`. Tornar o `HoldPoint` filho do `Interactor` (pendência da seção 8).
  3. A varredura que afasta o item da parede (`Interactor._Process`) precisa continuar valendo, e o item não pode entrar na barriga ao olhar para baixo (achado do `reviewer`, seção 8).
  4. Dedos fechando no item e mão abrindo ao soltar, se couber.
- Fora desta entrega: outros personagens, a locadora (entrega 5), som, sistema de ruído.
- Pronto quando: o dono pega a fita e vê a mão se esticar até ela; solta e vê a mão voltar.

## 10. Release final (tarefas obrigatórias)

- Remover do repositório todos os arquivos relacionados ao Claude (`CLAUDE.md`, `.claude/`, `CONTEXT.md` e semelhantes), mantendo somente o que pertence ao jogo.
- O repositório privado dos scripts do Blender (`SZonta/fita-sem-etiqueta-art`) continua privado. Conferir que nenhum script foi parar no público.
