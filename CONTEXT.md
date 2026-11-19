# CONTEXT.md

Fonte de verdade do projeto **Fita Sem Etiqueta**. Qualquer sessão nova começa lendo este arquivo.
Atualizar SEMPRE antes de cada commit.

Última atualização: 2026-11-19 (entrega 6, "As fitas", aprovada; primeira entrega com a arte pelo Gemini).

## 1. Estado atual

- **Etapa 1 (Pesquisa sobre Fears to Fathom):** concluída e aprovada em 2026-11-11.
- **Etapa 2 (Planejamento):** concluída e aprovada em 2026-11-11.
- **Etapa 3 (Desenvolvimento):** em andamento. Entregas 1 ("Instalação e projeto vazio") e 2 ("Filtro VHS e menu") concluídas e aprovadas em 2026-11-11. Entrega 3 ("Física e interação") concluída e aprovada em 2026-11-12. Entrega 4 ("Corpo do Nondas") concluída em duas sessões: parte 1 (pernas) aprovada em 2026-11-13, parte 2 (mão) aprovada em 2026-11-14. Entrega 5 ("A locadora") aprovada em 2026-11-14. **Entrega 6 ("As fitas") aprovada em 2026-11-19.**
- **Próxima entrega: 7, "Diálogo e idiomas"** (ver seção 9).
- **Arte:** desde 2026-11-19, a parte visual é gerada pelo Gemini e revisada pelo Claude (seção 4, "Arte com o Gemini"). A entrega 6 foi a primeira nesse fluxo (fachada e rebobinadeira).
- **Código de jogo:** projeto Godot em `game/`:
  - `levels/store/store.tscn`: **a Astro Vídeo Locadora, cena principal (F5)**. Instancia o cenário (`assets/models/store/store.gltf`, filhos editáveis), 7 luzes com sombra, uma sonda de reflexo, as placas de gênero (`Label3D` com chave de tradução), o jogador, o filtro VHS, o menu de pausa, 4 fitas devolvidas na bandeja (`ReturnedTapes`), os 4 vãos da estante por gênero (`ShelfGaps`), o videocassete com a TV (`TapeDeck`) e a rebobinadeira (`Rewinder`). `floor.tres` repete o material do piso ligando a cor de vértice (desgaste), que o importador de glTF deixa desligada.
  - `tapes/`: as fitas (entrega 6). `TapeInfo` (filme, gênero, créditos, adulterada, rebobinada; filho da fita), `ShelfGap` (vão da estante de um gênero), `TapeDeck` (videocassete, TV, programa da fita, modo assistir), `Rewinder` (tampa, luz, 4 s), `tape_screen.gdshader` (defeitos de fita na TV), `tape_shot.tscn` (a cena noturna gravada: fachada e loja, câmera na mão). Decisões em `docs/design/02-stack-architecture.md`, decisão da entrega 6.
  - `assets/models/facade/`: fachada da loja, vizinhos, calçada, metade da rua, poste, fios e bandeirinhas (glTF separado, 11 texturas, cerca de 5,6 mil triângulos, 50 colisores). Gerada por `tools/blender/facade.py` (Gemini). Por enquanto só aparece na fita; é a base da rua da entrega 8.
  - `assets/models/rewinder.glb`: rebobinadeira com tampa (`Lid`), luz (`Led`) e o ponto da fita (`TapeSpot`). Gerada por `tools/blender/rewinder.py` (Gemini).
  - `levels/test_room/test_room.tscn`: sala cinza de teste. Instancia o jogador, o filtro VHS e o menu de pausa. Tem uma lata vermelha, uma poça e uma caixa amarela do tamanho de uma fita VHS em cima do caixote, na borda (o braço alcança até uns 70 cm à frente).
  - `props/tape.tscn`: fita VHS pegável (estojo preto, `assets/models/tape.glb`, adesivo dourado; as variantes verde e vermelha estão no modelo, escondidas).
  - `assets/models/store/`: cenário da loja em glTF separado (`store.gltf`, `store.bin`, 12 texturas PNG). Gerado por `tools/blender/store.py`. 285 corpos de colisão convexos (camada `world`), cerca de 21 mil triângulos, 12 materiais.
  - `player/`: controlador de primeira pessoa (`PlayerController`). WASD anda a 2 m/s, mouse olha. A cena do jogador tem o corpo visível (`Body`, com o script `PlayerBody`), o raio de interação, o ponto da mão e a dica na tela.
  - `player/PlayerBody.cs`: corpo do Nondas visto de dentro. Pernas e braços animados pela direção e pela velocidade; cabeça desenhada só como sombra; uma área do tamanho do sapato em cada pé, que chuta objetos soltos e faz a poça ondular. Fica 12 a 32 cm atrás da câmera conforme o olhar e é empurrado para baixo dela pelas paredes. Agacha para pegar coisas e estica o braço direito até elas; agachado, a câmera fica nos olhos da pose, e o corpo recua onde a cabeça não cabe.
  - `player/HandAim.cs` e `player/FingerCurl.cs`: modificadores de esqueleto que giram a mão e fecham os dedos, depois da cinemática inversa do braço.
  - `assets/models/nondas.glb`: modelo do Nondas (malha do MPFB, CC0), gerado por script do Blender que fica num repositório privado (ver seção 4, Licença).
  - `interaction/`: `Interactor` (raio da câmera com mira tolerante, tecla E; conduz a pegada: agachar, alcançar, fechar a mão, levantar, segurar pela ponta; e o "pôr", a pegada ao contrário), interface `IInteractable`, `Pickable` (objeto que se pega, solta ou põe num lugar), interfaces `IReacher` (o corpo que alcança) e `IViewer` (quem para para assistir), ambas injetadas pelo `PlayerController`, `IReceiver` (lugar onde se põe o item na mão) e `IDescribed` (o que o item é, na dica).
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
- Somente ferramentas gratuitas. Assistentes de IA (Claude e Gemini) não contam; ver `CLAUDE.md`.

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
| Renderizador | Forward+ com o driver padrão (Vulkan). Na locadora, com o corpo, 7 luzes com sombra e o filtro VHS: cerca de 410 quadros por segundo a 1080p (Direct3D 12: cerca de 530). Driver reavaliado na entrega 5: fica o Vulkan; medir de novo na entrega 18 |
| Física | Jolt Physics (embutido no Godot) |
| Estilo visual | Semi-realista sob filtro VHS, com resolução interna baixa (cerca de 480 linhas) |
| Resolução interna | Só o 3D é reduzido (`Scaling3DScale` = 480 / altura da janela). Menus e textos ficam na resolução da janela, com esticamento `canvas_items` (base 1280x720) |
| Filtro VHS | Um shader de tela cheia. Aparência de câmera antiga, **sem nada que pisque ou corra pela tela** (escolha do dono). Cerca de 595 quadros por segundo a 1080p na sala de teste |
| Interpolação de física | Ligada. A cabeça do jogador é destacada do corpo e posicionada a cada quadro |
| Camadas de física | 1 `world`, 2 `player`, 3 `props` (objetos soltos), 4 `feet` (pés do jogador). O jogador não colide com objetos soltos: ele os chuta com os pés. Tabela completa em `docs/design/02-stack-architecture.md` |
| Corpo do jogador | Modelo próprio sobre a malha do MPFB, visível em primeira pessoa. Animações geradas por script (parado, andando nas 4 direções, agachar para pegar). Braço direito por cinemática inversa (`TwoBoneIK3D`). Detalhes em `docs/design/02-stack-architecture.md`, decisões da entrega 4 (partes 1 e 2) |
| Interação | Tecla E. Raio de 2 m que sai da câmera, com uma esfera de 8 cm quando o raio não acerta; o alvo implementa `IInteractable`. Item solto só se pega se o braço alcança (agachando se preciso; o item desliza no máximo 10 cm até a mão) e se a linha do ombro ao pulso não atravessa nada. O caminho do item até a mão é planejado e varrido: sob uma tábua, ele é puxado para fora antes de subir. Um item na mão por vez, sem inventário. A dica na tela fica acima do filtro VHS. Detalhes em `docs/design/02-stack-architecture.md`, decisão da entrega 5 |
| Cenário | Gerado por script do Blender (escrito pelo Gemini desde 2026-11-19; ver "Arte com o Gemini"), exportado em **glTF separado** (`.gltf` + `.bin` + PNG), para textura que não muda não pesar de novo no histórico. Colisão feita de caixas nomeadas `-convcolonly` no próprio script. Texturas importadas com mipmaps e compressão de vídeo (ajuste feito à mão nos `.import`; sem isso o granilite cintila a 480 linhas) |
| Cenário e objetos | 100% próprios, gerados por script Python no Blender. Texturas procedurais. Imagens do Gemini só como referência, nunca no jogo |
| Personagens humanos | Malha base de gerador gratuito (MPFB/MakeHuman) + rosto, roupa, textura e animação próprios (aparência pelo Gemini; esqueleto, pesos e animação pelo Claude, desde 2026-11-19). Licença confirmada em 2026-11-12: os assets do MPFB são CC0 (só os pacotes CC0 são usados); o código do complemento é GPLv3 e não vai para o jogo. Registro em `docs/credits.md` |
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
| `artist` | Diretor de arte (desde 2026-11-19): escreve os prompts para o Gemini e revisa o que volta (render na câmera do jogo, medidas, triângulos, colisões, nomes). Não escreve scripts nem shaders |
| `language` | Falas curtas em português e tradução para inglês e espanhol |

A sessão principal orquestra, desenvolve e faz o commit. Não existem agentes Orchestrator, Developer nem Commiter: um subagente não consegue chamar outros, um Developer separado perderia o contexto da conversa com o dono, e o commit é um comando cujas regras estão no `CLAUDE.md`. Todos os agentes herdam o modelo e o esforço da sessão.

### Arte com o Gemini (decidido em 2026-11-19)

O dono assina o Google AI Pro. Desde 2026-11-19, a parte visual é gerada pelo Gemini. O fluxo de 1 entrega = 1 sessão não muda: muda só quem escreve os scripts de arte. Base da decisão (testes de 2026-11-18): um orelhão e a locadora inteira gerados pelo Gemini rodaram de primeira no Blender; a locadora saiu fiel à planta num prompt só e chegou perto da aprovada depois de uma rodada de correção.

| Gemini | Claude |
|---|---|
| Scripts do Blender de cenário, objetos, texturas procedurais, capas, cartazes e rótulos | Prompts, pesquisa de época (`researcher`), revisão e integração no Godot |
| Aparência dos personagens: corpo pelo MPFB, rosto, roupas, acessórios, materiais | Esqueleto, pesos (o que prende a roupa ao esqueleto), animações e exportação dos personagens |
| Imagens e vídeos de referência (textura, roupa, ambiente) | Shaders (VHS, chuva, poça), luzes, materiais de correção (`.tres`), ajustes de importação (`.import`) |
| | Código C#, testes, commit e push (repositório público e `tools/blender/`) |

**Fluxo de um asset:**

1. O Claude, com o `artist`, escreve o prompt e manda ao dono no chat.
2. O dono cola no Gemini e salva o resultado na pasta `Downloads`.
3. O Claude pega em `Downloads` o arquivo mais recente do Gemini (`gemini-code-*.py`, `Gemini_Generated_Image_*`, `gemini_generated_video_*`). Não mexe nem apaga nenhum outro arquivo dessa pasta.
4. O Claude lê o script inteiro antes de rodar (numa correção, o que mudou em relação à versão anterior), roda, renderiza na câmera do jogo e confere medidas, triângulos, colisões e nomes. Só então copia para `tools/blender/`.
5. Aparência a corrigir vira um prompt de correção, na mesma conversa do Gemini, e volta ao passo 1. Erro mecânico que não muda a aparência (nome de objeto, colisão, caminho de exportação, semente) o Claude corrige direto e avisa no relatório da entrega.

**Regras:**

- **Script que já existe:** para o Gemini alterá-lo, o dono anexa no Gemini a versão atual junto com o prompt. O `store.py` vai sempre com o `tape.py`, de onde importa as medidas da fita. Só o script em questão sai do repositório privado.
- **Referência nunca entra no jogo** (decisão de 2026-11-18): imagens e vídeos do Gemini servem para o script imitar. Ficam em `tools/blender/reference/`; imagens são versionadas no repositório privado, vídeos não.
- **Prompt só com o que o asset precisa.** Nada de finais nem reviravoltas da história: o que vai no prompt fica com o Google.
- **O Claude nunca aciona o Gemini.** A FAQ do Antigravity (conferida em 2026-11-19) diz que o `agy -p` "is intended for direct scripting and CI workflows", que envolvê-lo em agentes ou aplicativos de terceiros não é suportado nos planos pessoais e que usar o Claude Code com o login do Antigravity viola os termos e pode suspender a conta. Controlar o app do Gemini pela tela também não é usado.
- **Modelos:** scripts no Neon, raciocínio Médio (Alto se a resposta falhar); imagens no Hydrogen, raciocínio Baixo; vídeo só quando valer o custo.
- **Custo medido em 2026-11-18** (janela de 5 h do Google AI Pro): vídeo de 10 s, 20%; imagem, 1%; script do Blender, menos de 1%. A página "Limites de uso" do app fica em cache: recarregar o app (Ctrl+R) antes de ler.
- **Primeira rodada (entrega 6):** dois scripts num prompt cada, sem correção de aparência. A rebobinadeira rodou de primeira. A fachada tinha 2 erros mecânicos (multiplicação no lugar de matrizes do numpy com formatos diferentes, `*=`), corrigidos pelo Claude; o resto saiu certo e determinístico. Os prompts ficam em `docs/prompts/` (o `artist` escreve, o dono cola o bloco `text`).
- **Todo prompt de script diz, e o `artist` confere na volta:** eixos e origem; medidas; nomes dos objetos que o código usa; colisão `-convcolonly` por peça (cada tábua, lateral, fundo e tampo), uma caixa por trecho contínuo de fitas ou caixas e 2 cm livres acima de item em prateleira (seção 8); só Principled BSDF, com texturas geradas por código e semente fixa (o mesmo script gera o mesmo arquivo); nada de marca, logo ou texto legível; leve para 480 linhas; formato e caminho de exportação.

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
- **Pose final do esqueleto** (depois da cinemática inversa e dos outros modificadores): só no sinal `skeleton_updated`. `GetBoneGlobalPose` lido em `_process` devolve a pose sem os modificadores.
- **`--fixed-fps 144`** simula quadros mais rápidos que a física (quadros sem tick). Defeitos de ordem entre física e imagem (como o tremor da mão ao andar) só aparecem assim; com 60, cada quadro tem um tick e eles somem. Usar 30 ou 40 para quadros mais lentos que a física.
- O `_process` de um roteiro que estende `SceneTree` roda antes do `_process` dos nós no mesmo quadro.
- Nas gravações, o quadro logo depois de um salto de ângulo feito pelo roteiro (por exemplo de 0° a -60° num quadro) mostra uma transição; não julgar a aparência por ele.
- **Na locadora, cada quadro gravado custa cerca de 0,4 s** (330 quadros levaram 2 min 12 s), contra 0,18 s na sala cinza. Estimar por 0,4 s ao pedir a liberação.
- **Sem janela, o mouse não gira a câmera** (o modo capturado não existe). Para olhar para cima ou para baixo num roteiro: um nó auxiliar com `process_priority = 1` que escreve `Head.rotation.x` no `_process`, e o `Interactor` com `process_priority = 2`, mais a mesma escrita no sinal `physics_frame` do `SceneTree`. Só no tick de física não basta: no quadro de imagem o `PlayerController` volta a câmera ao ângulo dele, e o caminho do item é planejado com um olhar e executado com outro. O giro para os lados vem do `transform` do jogador antes de entrar na árvore.
- Testes que encontraram defeitos na entrega 5 (roteiros descartáveis, fora do repositório): fita, câmera, ossos do braço (`upperarm_r`, `lowerarm_r`, `hand_r`, lidos no sinal `skeleton_updated`) e peito/pescoço (`spine_03`, `neck_01` dentro do cone da câmera) contra a camada `world`, quadro a quadro, em 16 pegadas e 16 solturas. Para ler propriedades C# do `Interactor` no roteiro: `get("Bend")`.
- **Recriar a mesma cena muitas vezes num roteiro** (cerca de 12) gerou uma vez uma exceção da ponte C#/Godot no `_EnterTree` do `VhsFilter`. No jogo a cena carrega uma vez. Vigiar quando o `LevelLoader` recarregar fases (entrega 9).

### Blender (gerar modelos)

- Rodar um script: `C:/Tools/Blender/5.2.4/blender-5.2.4-windows-x64/blender.exe --background --python tools/blender/<script>.py`, a partir da raiz do projeto. Não abre janela e não precisa de liberação. O corpo do Nondas leva cerca de 4 s.
- **Script do Gemini** (seção 4, "Arte com o Gemini"): rodar primeiro com `--out` numa pasta fora do repositório e renderizar a câmera do jogo; só depois da revisão copiar para `tools/blender/` e gerar em `game/assets/`, para o dono testar no jogo. Rodar duas vezes e conferir que os arquivos saem idênticos.
- **Conferir um vídeo do Gemini:** servir o MP4 por um servidor local e abrir no painel do navegador do app, com o painel **visível**, capturando quadros com o vídeo tocando (`requestVideoFrameCallback`). Com o painel escondido o vídeo não decodifica, e a busca por `currentTime` devolve sempre o primeiro quadro. O áudio não dá para ouvir; só medir o volume.
- Depois de exportar um `.glb`: `..._console.exe --headless --import --path game`.
- **Ver a aparência sem abrir o jogo:** renderizar no Blender (EEVEE em `--background` funciona) com uma câmera na altura e no campo de visão do jogo (75° na vertical, 854x480). Foi assim que o dono escolheu a roupa.
- **Página de comparação no painel do navegador:** arquivo fora da pasta do projeto vira foto estática (sem JavaScript). Arquivo dentro do projeto abre como endereço `data:`, onde imagens com caminho relativo não carregam, e páginas grandes são recusadas. O que funcionou: servir uma pasta temporária com `python.exe -m http.server` (o Python do Blender) por uma entrada em `.claude/launch.json`, com as imagens em JPEG. Apagar a pasta e a entrada depois.
- API do MPFB usada por script: `HumanService.create_human`, `TargetService.bake_targets`, `HumanService.add_builtin_rig(body, "game_engine")`. O módulo é `bl_ext.user_default.mpfb`; ligar com `addon_utils.enable` **depois** de `read_factory_settings`.
- **Ação nova estraga a primeira chave** se já existir uma faixa de NLA tocando: criar as faixas de NLA só depois de gravar todas as ações.
- Ações que mexem nas costas (`stoop`) gravam também `spine_01` a `spine_03`; as outras ações gravam as costas retas (`stand_up_straight`), para não herdarem a pose da ação gravada antes.
- **Conferir o pescoço:** render no Blender da câmera do jogo olhando para baixo, com o corpo a 11, 18, 24 e 32 cm, e a malha `Head` escondida. Foi assim que a tampa do pescoço foi validada sem abrir o jogo.
- **Loja:** `tools/blender/store.py` (cerca de 5 s; `-- --preview <pasta>` renderiza 5 vistas da câmera do jogo em EEVEE). Importa as medidas da fita de `tape.py`. Dois rodadas seguidas geram arquivos idênticos.
- **Ao regerar a loja, os `.import` das texturas não mudam** (mipmaps e compressão de vídeo foram ligados à mão). Textura NOVA nasce com o padrão do Godot (sem mipmaps): repetir o ajuste (`compress/mode=2`, `mipmaps/generate=true`, `detect_3d/compress_to=0`).
- **Fachada:** `tools/blender/facade.py` (Gemini, cerca de 6 s; `-- --preview <pasta>` renderiza 4 vistas: do outro lado da rua a 75°, a mesma a 36° em 4:3, a porta e o portão). Mesma origem do `store.py`. Texturas com mipmaps e compressão ligadas à mão nos `.import` (como as da loja).
- **Rebobinadeira:** `tools/blender/rewinder.py` (Gemini; `-- --preview <pasta>` com a tampa fechada e aberta).
- **Conferir uma gravação sem abrir 400 imagens:** um script do Blender que monta uma folha de contato (9 quadros reduzidos num JPEG) ou recorta a mesma região de vários quadros lado a lado (a tela da TV). Fica na pasta de rascunho da sessão; refazer quando precisar.
- **SubViewport:** a textura da imagem chega em sRGB. Num shader, ruído e mistura de cor feitos depois da conversão para linear (`source_color`) pesam muito nos escuros; ler sem `source_color`, trabalhar em sRGB e converter com `pow(c, 2.2)` no fim.
- Para medir o custo de um viewport numa gravação: `RenderingServer.viewport_set_measure_render_time(rid, true)` e `viewport_get_measured_render_time_gpu(rid)`.
- O importador de glTF do Godot 4.7.2 **não liga `vertex_color_use_as_albedo`** mesmo com a cor de vértice na malha. Por isso o piso tem o material `levels/store/floor.tres` por cima. Mudou o material do piso no Blender: atualizar o `.tres`.

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

- **Corpo atrás da câmera (12 a 32 cm, conforme o olhar):** uma varredura empurra o corpo para baixo da câmera junto a paredes; nada entra mais de 6 mm (64 casos). Encostado de costas num canto, olhando para baixo, o corpo chega a 11 cm da câmera e o peito ocupa metade da tela (pescoço tampado; o dono aprovou). O chute por trás da parede continua bloqueado por um raio (achado do `reviewer` na parte 1).
- **Chute pelos pés:** a lata só voa quando o sapato encosta nela. Uma lata centrada entre os pés (30 cm de distância entre eles) não é chutada; as pernas passam por cima. O pé sobe 10 cm na passada e às vezes passa por cima da lata (12 cm), e quem chuta é o calcanhar, um pouco tarde. Se o dono reclamar: baixar a passada para 5 cm (mais realista) e ajustar o `TouchHeight` da poça junto.
- **Passo para o lado:** o pé desliza cerca de 60%, porque o passo lateral é curto (para os pés não se cruzarem) e o ritmo é o da caminhada para a frente. O dono aprovou assim.
- **`TouchHeight` da poça (10 cm):** objeto solto com centro acima de 10 cm (uma garrafa em pé, por exemplo) nunca faz a poça ondular. A altura é medida em metros no mundo, então a poça pode ser escalada.
- **Quadros por segundo com o personagem (medido na entrega 5):** cerca de 410 na locadora, tela cheia 1080p, Vulkan. Direct3D 12 cerca de 530 (+30%; na sala vazia eram +19%). Fica o Vulkan (padrão mais testado do motor; desempenho sobra). Medir de novo na entrega 18, com o jogo inteiro.
- **Cores e tom de pele do Nondas:** camiseta clara, jeans, Kichute preto, pele parda (escolhas do dono). As cores são provisórias e mudam numa linha do script. Ainda não há textura; o rosto não aparece.
- **Textura procedural:** aprovada pelo dono na entrega 5 (granilite, barrado a óleo, látex, madeira, galvanizado), sob o filtro VHS. Chapisco e caquinho (rua, quintal) ainda não foram feitos; conferir na entrega 8.
- **Todo objeto solto pequeno precisa de `continuous_cd`** (lata e fita já têm). No teste da entrega 1, objetos pequenos e rápidos atravessaram um piso fino nos dois motores de física.
- **Todo objeto solto novo:** `collision_layer = 4` (camada 3, `props`) e `collision_mask = 5` (mundo e outros objetos). Na camada errada, o jogador volta a subir em cima dele.
- **O jogador atravessa objetos soltos** (decisão da entrega 3). Um objeto que precise barrar o caminho deve ficar na camada `world`.
- **Chute determinístico:** a lata tomba sempre para a frente e segue deitada. Se parecer repetitivo na rua (entrega 8), variar o giro.
- **A fita virou cena própria** (`props/tape.tscn`) na entrega 5; a sala de teste ainda usa a dela, escrita dentro de `test_room.tscn`. A lata continua escrita lá e vira cena na entrega 8.
- **Fitas da bandeja (entrega 6):** 4 fitas devolvidas (`ReturnedTapes` em `store.tscn`), paradas no fim. As 5 fitas de teste saíram. A entrega 9 decide quais fitas cada noite traz.
- **O videocassete chama o jogador** (`TapeDeck` → `Interactor.Viewer.Watch`, com um `Action` no lugar de sinal; achado do `reviewer`): aceitável enquanto não há quem coordene a fase. Rever com o `LevelLoader` e o `GameState` (entrega 9), quando "fita testada", "rebobinada" e "guardada" saírem do nó da fita para o estado da história.
- **Guardar na estante (feito na entrega 6):** 4 vãos vazios, um por gênero das fitas da bandeja, nas tábuas de 0,91 e 1,23 m (`RACK_EMPTY` no `store.py`; o relatório do script dá os extremos de cada vão). Uma fita nova de outro gênero precisa de vão novo. A rebobinadeira só aceita fita testada até o fim (achado do `reviewer`: rebobinada antes, a gravação da fita adulterada sumiria). Guardar sem testar ou sem rebobinar ainda é possível: missões da entrega 9.
- **Item na mão e troca de fase:** o `Pickable` guarda o pai original para voltar a ele ao ser solto. Se a fase for trocada com o item na mão, esse pai não existe mais: soltar lança erro e o `Interactor` fica preso em "Soltar" (achado do `reviewer`). No caso inverso, se o jogador for recarregado com o item na mão, o item é apagado junto, porque é filho do ponto da mão. Tratar os dois junto com o `LevelLoader`. Desde a parte 2, um item apagado enquanto está na mão é esquecido (`ForgetFreedItem`); o caso do pai original que não existe mais continua aberto.
- **Regra de montagem de fase (alcance da mão, medido na entrega 4, parte 2, e conferido na loja na entrega 5):** item em cima de móvel a até uns 65 cm do eixo do jogador encostado (uns 30 cm da borda de um balcão de 1 m); no chão, até uns 50 cm à frente; em prateleira, até 1,7 m de altura. Fora disso o "[E] Pegar" não aparece. Na loja: a tábua de cima da estante de fitas (1,55 m, fita em pé) só se alcança encostado na estante; a bandeja de devolução, a uns 30 cm dela.
- **Regra de montagem: pelo menos 2 cm livres acima de item em prateleira** (achado do `reviewer` na entrega 5). As varreduras do Jolt ignoram o que já está sobreposto no ponto de partida; com uma tábua a menos de 2 cm, a mão não percebe a tábua e desce através dela. A fita em pé na tábua de cima da estante tem 4 cm.
- **Guarda na subida:** se o caminho real se afasta do planejado (o jogador gira a vista durante a pegada), a fita para onde encostaria e só vai para a mão no fim da pegada (1,05 s), num salto. Não atravessa, mas o salto pode aparecer.
- **Frestas que engolem fita:** a revisão achou e a entrega 5 fechou duas (3 cm de cada lado da porta dos fundos, que davam para fora do cenário; 10 cm entre a estante de fitas e o móvel da TV, que agora começa em z -3,0). Toda fresta entre móvel e parede ou entre móveis com 3 cm ou mais é um risco de softlock para a fita de missão (entrega 6): conferir ao montar fase.
- **Estante: espaço entre tábuas.** Fita deitada no fundo da tábua de baixo (33 cm até a tábua de cima) quase não se vê de pé. As fitas ficam em pé, lombada para fora, na borda: assim se veem e se alcançam. Manter isso na entrega 6.
- **Caminho do item (resolvido na entrega 5):** o alcance recusa item atrás de balcão ou grade (linha ombro→pulso com a espessura do braço); o caminho até a mão é planejado e varrido. Detalhes em `docs/design/02-stack-architecture.md`, decisão da entrega 5. Resta: na estante de cima, o pulso roça as fitas vizinhas do vão por 2 quadros (vão de 30 cm).
- **Item que desliza até a mão** (decisão do dono): até 10 cm quando o braço fica aquém. Girando muito durante a pegada (1 s), o item segue a mão no máximo 20 cm e pode descolar dela por um instante.
- **Pegada feita para a fita** (pela ponta, polegar na frente). Item de outro formato (lata, garrafa) pode precisar de outra pegada; a lata nem é `Pickable` hoje.
- **Pulso ainda gira uns 40°** em relação ao antebraço ao segurar (o esqueleto não tem ossos de torção; o antebraço já assume 60%). O dono aprovou a aparência.
- **Chute abaixo de 60 quadros por segundo:** o corpo é posicionado só no quadro de imagem (no tick de física a mão tremia ao andar). Os pés chutam da posição desenhada; abaixo de 60 quadros por segundo, um chute pode sair um quadro atrasado. A lata foi chutada a 144, 60 e 30.
- **Fita atravessou o chão uma vez** no teste do dono, sem que ele lembrasse o que fez antes. Não se reproduziu em 8 tentativas (soltar do caixote e do chão, 5 ângulos). Se acontecer de novo, pedir ao dono o que fez nos 5 segundos anteriores.
- **Item de missão chutado (decidido na entrega 6):** a fita continua chutável. O `reviewer` não achou na loja ponto onde uma fita chutada fique fora de alcance (rodapés fechados, estante da TV emendada na de fitas, corpo do balcão fechado). Vale a regra de montagem: nenhuma fresta de 3 cm ou mais. Rever na rua (entrega 8).
- **Poça no editor:** sem o jogo rodando, o shader mostra anéis parados no centro da poça (o script que os zera não roda no editor). No jogo não aparece.
- **Texto da dica com a tecla fixa** ("[E] Pegar"). Se um dia houver troca de teclas, a tecla sai do texto.
- **Dica acima do filtro VHS:** decisão minha, igual à do menu de pausa. O dono viu e não opinou.
- **Objeto pequeno afunda alguns milímetros no piso** em repouso (tolerância de penetração padrão do Jolt, 2 cm). Não se vê a 480 linhas. Baixar `physics/jolt_physics_3d/simulation/penetration_slop` resolve, mas não foi adotado por ser um ajuste global sem necessidade visível.
- **Teleporte do jogador (troca de fase, checkpoint):** com a interpolação ligada, chamar `ResetPhysicsInterpolation()` depois de mover o jogador, senão ele desliza de um ponto ao outro.
- **Filtro VHS conferido na locadora (entrega 5):** o dono elogiou a loja com o filtro. Sem mudança.
- **Defeitos de fita (feito na entrega 6):** opção "C" escolhida pelo dono, só na TV de teste (o filtro geral não muda). Receita em `docs/design/02-stack-architecture.md`, decisão da entrega 6.
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
- **Cenas sem `uid`:** `player.tscn`, `test_room.tscn`, `vhs_filter.tscn`, `pause_menu.tscn`, `puddle.tscn`, `store.tscn` e `tape.tscn` foram escritas à mão. Na primeira vez que o editor salvar, ele reescreve o cabeçalho; esse diff é esperado.
- **Locadora, peças estáticas para entregas futuras:**
  - Porta de aço fechada (decisão do dono: a rua só existe na entrega 8). Abrir e baixar é missão da entrega 9.
  - Porta do depósito aberta para dentro, com um engradado de calço e a maçaneta de dentro quebrada; porta dos fundos fechada, no corredor (não no depósito: na armadilha o Freguês, que tem a chave, sairia por ela). Ainda sem portas que se movem. O sentido de abertura da porta do depósito se decide na entrega 15 (armadilha).
  - Vão livre sob o balcão (lado do atendente, 1,22 m × 0,9 m × 0,56 m) para o esconderijo da entrega 12, mas a colisão do corpo do balcão ainda é fechada.
  - Sem colisão (decoração): caderno, fichário, caneta, cartazes, cortina, calhas, vassoura, mãos-francesas da bandeja. A rebobinadeira (entrega 6) só tem colisão no fundo da cavidade: uma fita solta em cima dela cai na cavidade, de onde se pega de novo.
  - Etiquetas pequenas da estante de fitas ficam ilegíveis a 480 linhas; as placas grandes ficam legíveis.
  - TV de teste: a tela é um objeto à parte (`TvScreen`, UV de 0 a 1 vista de frente); enquanto a fita toca, o `TapeDeck` troca o material dela pelo da imagem.
- **Fatos de época da loja (pesquisa da entrega 5, confiança média):** caixa vazia com a capa na prateleira; a fita ficava guardada atrás do balcão; estantes por gênero; ECA (1990) exigia respeitar a classificação etária. Sem fonte: cortina da seção adulta, fenda de devolução, piso, paredes (tratados como escolha de design).
- **Exportação para Windows com `net10.0`:** não validada. Validar na entrega 19 (ou antes, se alguma entrega precisar exportar).
- **Tamanhos de download** das ferramentas: confirmar na fonte no momento de pedir cada autorização.
- **Fatos de época a verificar pelo `researcher`** (lista completa no fim de `docs/design/01-story.md`): Dois Córregos em 1996 (já achado: 18.838 habitantes no Censo de 1991, 22.522 no de 2000; comércio do centro sem fonte), orelhão (ficha ou cartão, ligação de emergência), preço de locação, filmadora doméstica e carimbo de data, viatura e farda da PM.
- **Taxa de publicação da Steam** (100 dólares por jogo, citada de memória): confirmar na página oficial antes da entrega 19.
- **Nome do jogo:** a busca por jogos homônimos na Steam foi rápida. Não é verificação de marca registrada.
- Os enredos dos episódios 2 e 3 de Fears to Fathom têm confiança média. Confirmar por gameplay antes de usar algum detalhe como referência direta.
- Escopo no teto (6 personagens, armadilha, final secreto, microfone, visual semi-realista). A ordem do MVP deixa os itens cortáveis por último.

## 9. Próxima entrega

**Entrega 7: Diálogo e idiomas** (`docs/design/03-mvp.md`, marco 1).

- O dono atende Dona Neide (`docs/design/01-story.md`, noite 1, item 2): ela chega ao balcão e pede uma fita; Nondas acha a fita e anota a locação no caderno. Legenda na parte de baixo da tela, "ruído de fala" e gesto (personagens sem voz). Falas curtas, por chave de tradução, pelo agente `language`.
- Troca de idioma (português, inglês, espanhol) e tamanho da legenda no menu de pausa (lição 3 da pesquisa).
- **Primeiro personagem além do Nondas:** aparência pelo Gemini (corpo do MPFB, rosto, roupa, materiais), por prompt; esqueleto, pesos, animações (parada, falando com gesto, andando) e exportação pelo Claude. Seção 4, "Arte com o Gemini".
- **Ruído de fala:** gravado pelo dono e tratado no Audacity (a instalar, com autorização; seção 6). Efeitos de terceiros só CC0.
- **Decisões a levar ao dono antes de programar:** por onde Dona Neide entra, se a porta de aço está fechada e a rua só existe na entrega 8; como achar a fita pedida (a estante atrás do balcão já tem os vãos por gênero) e como é o caderno; e a data na imagem da fita adulterada (pendente da entrega 6: a fachada mostraria a data do dia anterior nos primeiros segundos, para o jogador aprender a ler o carimbo que revela tudo na noite 3).
- Fora desta entrega: rua e bar (8), Freguês, Cabo Moreira, missões e estado da história (9), sistema de ruído.
- Pronto quando: o dono, com F5, atende Dona Neide do começo ao fim, lê a legenda nos três idiomas e troca o idioma no menu.

## 10. Release final (tarefas obrigatórias)

- Remover do repositório todos os arquivos relacionados ao Claude (`CLAUDE.md`, `.claude/`, `CONTEXT.md` e semelhantes), mantendo somente o que pertence ao jogo.
- O repositório privado dos scripts do Blender (`SZonta/fita-sem-etiqueta-art`) continua privado. Conferir que nenhum script foi parar no público.
