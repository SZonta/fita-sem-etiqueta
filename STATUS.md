# STATUS.md — Estado Atual e Próximas Entregas

Última atualização: 2026-11-26 (Entrega 7 aprovada pelo dono).
Atualizar este arquivo **sempre antes de cada commit de entrega**.

---

## 1. Progresso Geral

- **Etapa 1 — Pesquisa (*Fears to Fathom*):** concluída e aprovada (2026-11-11). Ver `docs/research/01-fears-to-fathom.md`.
- **Etapa 2 — Planejamento (História, Stack, Arquitetura e MVP):** concluída e aprovada (2026-11-11). Ver `docs/design/01-story.md`, `02-stack-architecture.md` e `03-mvp.md`.
- **Etapa 3 — Desenvolvimento:** **7 de 19 entregas concluídas** (Marco 1: *Noite 1 jogável*, entregas 1 a 9):
  - [x] **Entrega 1:** Instalação e projeto vazio (sala cinza, 1ª pessoa, Forward+ Vulkan, Jolt, `net10.0`) — 2026-11-11
  - [x] **Entrega 2:** Filtro VHS e menu de pausa/opções (`Settings` autoload, 480 linhas 3D) — 2026-11-11
  - [x] **Entrega 3:** Física e interação (lata chutável, poça com ondulação, `Interactor` + `Pickable`) — 2026-11-12
  - [x] **Entrega 4:** Corpo do Nondas — Parte 1 (pernas, caminhada, chute/poça pelos pés) e Parte 2 (mão direita com IK, agachar para pegar, recuo contra paredes) — 2026-11-13 / 2026-11-14
  - [x] **Entrega 5:** A locadora (*Astro Vídeo Locadora*, `store.gltf`, 7 luzes, placas traduzidas, caminho varrido da pegada) — 2026-11-14
  - [x] **Entrega 6:** As fitas (bandeja de devolução, videocassete/TV com gravação ao vivo da fachada noturna, rebobinadeira, estante por gênero) — 2026-11-19
  - [x] **Entrega 7:** Diálogo e idiomas (Dona Neide, legendas inferiores + ruído de fala por formantes, troca de idioma e tamanho de legenda, carimbo `AUTO DATE`) — 2026-11-26
  - [ ] **Entrega 8 (PRÓXIMA):** A rua e o bar

Histórico técnico detalhado das entregas 1–7: `docs/design/04-decisions-log.md`.

---

## 2. O Que Já Existe em `game/`

- **Cena principal (F5):** `levels/store/store.tscn` (*Astro Vídeo Locadora* com `store.gltf`, `floor.tres`, meia-porta `SteelDoorHalf` + `facade.gltf` noturna visível fora, 7 luzes internas + `StreetLamp`, `ReflectionProbe`, placas `Label3D` traduzidas, jogador, filtro VHS, menu de pausa, 4 fitas devolvidas em `ReturnedTapes`, 3 fitas pegáveis em `RomanceTapes`, 5 vãos em `ShelfGaps`, `TapeDeck`, `Rewinder`, `RentalLedger`, `CounterSpot`, `DonaNeide`, `DialoguePlayer` e `SubtitleBox`).
- **Cena de teste:** `levels/test_room/test_room.tscn` (sala cinza com lata vermelha, poça e caixa de teste).
- **Sistemas prontos:**
  - `core/Settings.cs` (autoload: intensidade VHS padrão `0.7`, sensibilidade, volume, idioma `pt`/`en`/`es` e tamanho de legenda em `user://settings.cfg` — único dos 4 autoloads criado até agora).
  - `player/` (`PlayerController.cs`, `PlayerBody.cs`, `HandAim.cs`, `FingerCurl.cs`, `player.tscn`).
  - `interaction/` (`Interactor.cs`, `Pickable.cs`, interfaces `IInteractable` com `WorksWhileHolding`, `IReceiver`, `IReacher`, `IViewer`, `IDescribed`).
  - `tapes/` (`TapeInfo.cs` com `Stamp`, `TapeDeck.cs` com carimbo `AUTO DATE` de 5,5 s, `Rewinder.cs`, `ShelfGap.cs`, `tape_screen.gdshader`, `tape_shot.tscn`).
  - `dialogue/` (`DialogueLine.cs`, `DialoguePlayer.cs`, `SubtitleBox.cs`, `subtitle_box.tscn`, `RentalLedger.cs`, `CounterSpot.cs`).
  - `npc/` (`CustomerNeide.cs`, `dona_neide.tscn`).
  - `props/tape.tscn`, `ui/PauseMenu.cs`, `vfx/VhsFilter.cs`, `vfx/Puddle.cs`, `localization/ui.csv` (`pt`, `en`, `es`).
- **Assets gerados (`tools/blender/` → `game/assets/`):** `nondas.glb`, `dona_neide.glb` (+ 4 PNGs de textura), `tape.glb`, `rewinder.glb`, `store/` (glTF separado + 12 PNGs), `facade/` (glTF separado + 11 PNGs) e `audio/` (`speech/neide_01..06.wav`, `speech/nondas_01..06.wav`, `sfx/pen_write.wav`).

---

## 3. Próxima Entrega — Entrega 8: A rua e o bar

Referências: `docs/design/03-mvp.md` (Marco 1) e `docs/design/01-story.md` (Noite 1, item 5).

- **Escopo jogável:**
  - Sair da locadora e atravessar o quarteirão noturno (junho de 1996, quermesse ao longe, bandeirinhas, poste, orelhão na esquina e latas chutáveis) até o **Bar e Lanchonete do Tião** (mesa de sinuca, rádio, cadeiras de plástico na calçada).
  - Conversar com **Tião** (diálogo + gesto + ruído de fala próprio).
  - Interagir/fazer carinho no **Paçoca** (cachorro caramelo modelado e animado do zero por script no Blender; nunca sofre dano).
  - Mecânica de **refúgio**: sentar na cadeira de plástico da calçada e beber cerveja (marca fictícia), vendo da cadeira exatamente o enquadramento da fachada filmado na primeira fita.
- **Decisões para alinhar com o dono ANTES de codar:**
  1. Como o jogador passa da loja para a rua nesta entrega antes da missão completa de baixar a porta de aço da Entrega 9 (ex.: porta de aço já aberta/recolhida sem barreira na soleira, ou interação de abrir/fechar a porta).
  2. Organização de cenas (`street.tscn` separada com transição na soleira ou incorporada a um cenário contínuo loja + rua) e conversão da lata de `test_room.tscn` em prop reutilizável (avaliando variar o giro do chute).
  3. Validação visual das texturas procedurais de **chapisco** e **caquinho** no bar/muros sob o filtro VHS e inspeção anti-softlock de frestas `>= 3 cm` na calçada/rua; avaliar se a passada do chute (10 cm) e `TouchHeight` da poça precisam descer para 5 cm.
- **Fora do escopo desta entrega:** Freguês, Cabo Moreira, quitinete/dormir e máquina de missões/`GameState`/`SaveService`/`LevelLoader` (Entrega 9).
- **Critério de pronto:** com F5, o dono sai à rua, chuta lata até o bar, conversa com Tião, faz carinho no Paçoca, senta na cadeira com cerveja e reconhece o ângulo da primeira fita.

---

## 4. Pendências e Pontos Vigiados para Entregas Futuras

### Para a Entrega 9 (`GameState`, `LevelLoader`, `SaveService` e Noite 1 completa)
- **Item na mão ao trocar de fase:** `Pickable` guarda o pai original; se a fase anterior for destruída com item na mão, soltar lança erro e trava em "Soltar". Se o jogador for recarregado, o item filho de `HoldPoint` some junto. Tratar ambos no `LevelLoader`.
- **Acoplamento `TapeDeck` → jogador:** substituir `Interactor.Viewer.Watch` (via `Action`) quando o estado das fitas ("testada", "rebobinada", "guardada") migrar para `GameState` e definir as fitas exatas de cada noite.
- **Persistência de UI/VFX entre cenas:** mover `VhsFilter` e `PauseMenu` para fora das cenas individuais de fase para sobreviverem ao `LevelLoader`, e vigiar recriações repetidas de cena (`VhsFilter._EnterTree`).
- **Menu de pausa (`PauseMenu`):** salvar e restaurar o `MouseMode` anterior ao fechar (necessário quando houver menu principal/epílogo com mouse solto), pausar automaticamente ao perder foco da janela, adicionar botão "Sair" e navegação por teclado.
- **Porta de aço da loja:** tornar móvel (abrir/baixar como missão).

### Para Entregas 10 a 19 (Marcos 2 e 3)
- **Cor e roupa do Nondas:** cores provisórias lisas hoje; revisar se ganhará textura ou ajustes.
- **Pegada de outros formatos:** a pegada atual pela ponta foi calibrada para fita VHS; garrafa/outros itens pegáveis podem exigir pose dedicada.
- **Esconderijo e armadilha na loja:** abrir a colisão do vão sob o balcão na Entrega 12 (esconderijo) e definir sentido/movimento da porta do depósito na Entrega 15 (armadilha).
- **Pesquisas de época pendentes (`fse-pesquisa-epoca`):** comércio/DDD de Dois Córregos em 1996, orelhão (ficha vs cartão e gratuidade do 190), preço de locação em Real, farda e viatura da PM-SP.
- **Desempenho e Release (Entregas 18–19):** reavaliar FPS Vulkan vs D3D12 com jogo completo (18); confirmar valor atual da taxa Steam Direct na fonte oficial; validar exportação Windows com `net10.0`; remover arquivos internos de IA/governança (`GEMINI.md`, `STATUS.md`, `.agents/`) no pacote/repositório de release final, mantendo `tools/blender/` privado.
