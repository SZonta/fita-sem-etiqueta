# STATUS.md — Estado Atual e Próximas Entregas

Última atualização: 2026-11-26 (Entrega 8 aprovada pelo dono).
Atualizar este arquivo **sempre antes de cada commit de entrega**.

---

## 1. Progresso Geral

- **Etapa 1 — Pesquisa (*Fears to Fathom*):** concluída e aprovada (2026-11-11). Ver `docs/research/01-fears-to-fathom.md`.
- **Etapa 2 — Planejamento (História, Stack, Arquitetura e MVP):** concluída e aprovada (2026-11-11). Ver `docs/design/01-story.md`, `02-stack-architecture.md` e `03-mvp.md`.
- **Etapa 3 — Desenvolvimento:** **8 de 19 entregas concluídas** (Marco 1: *Noite 1 jogável*, entregas 1 a 9):
  - [x] **Entrega 1:** Instalação e projeto vazio (sala cinza, 1ª pessoa, Forward+ Vulkan, Jolt, `net10.0`) — 2026-11-11
  - [x] **Entrega 2:** Filtro VHS e menu de pausa/opções (`Settings` autoload, 480 linhas 3D) — 2026-11-11
  - [x] **Entrega 3:** Física e interação (lata chutável, poça com ondulação, `Interactor` + `Pickable`) — 2026-11-12
  - [x] **Entrega 4:** Corpo do Nondas — Parte 1 (pernas, caminhada, chute/poça pelos pés) e Parte 2 (mão direita com IK, agachar para pegar, recuo contra paredes) — 2026-11-13 / 2026-11-14
  - [x] **Entrega 5:** A locadora (*Astro Vídeo Locadora*, `store.gltf`, 7 luzes, placas traduzidas, caminho varrido da pegada) — 2026-11-14
  - [x] **Entrega 6:** As fitas (bandeja de devolução, videocassete/TV com gravação ao vivo da fachada noturna, rebobinadeira, estante por gênero) — 2026-11-19
  - [x] **Entrega 7:** Diálogo e idiomas (Dona Neide, legendas inferiores + ruído de fala por formantes, troca de idioma e tamanho de legenda, carimbo `AUTO DATE`) — 2026-11-26
  - [x] **Entrega 8:** A rua e o bar (cenário contínuo loja + rua + Bar do Tião, lata reutilizável, Tião, Paçoca, cadeira do refúgio com o enquadramento da 1ª fita) — 2026-11-26
  - [ ] **Entrega 9 (PRÓXIMA):** Noite 1 completa

Histórico técnico detalhado das entregas 1–8: `docs/design/04-decisions-log.md`.

---

## 2. O Que Já Existe em `game/`

- **Cena principal (F5):** `levels/store/store.tscn` (*Astro Vídeo Locadora* + cenário contínuo da rua e *Bar do Tião*, com `store.gltf`, `floor.tres`, `facade.gltf`, `street.gltf`, soleira aberta com `SteelDoorHalf` oculta e `SteelDoorBarrier` desabilitada, 7 luzes internas + `StreetLamp` + `OppositeStreetLamp` + 3 luzes do bar, `ReflectionProbe`, placas `Label3D` traduzidas incluindo `SIGN_BAR_NAME`, jogador, filtro VHS, menu de pausa, 4 fitas devolvidas em `ReturnedTapes`, 3 fitas pegáveis em `RomanceTapes`, 5 vãos em `ShelfGaps`, `TapeDeck`, `Rewinder`, `RentalLedger`, `CounterSpot`, `DonaNeide`, `Tiao`, `Pacoca`, `BarRadio`, `RefugeChair`, 3 latas `StreetCans`, `DialoguePlayer` e `SubtitleBox`).
- **Cena de teste:** `levels/test_room/test_room.tscn` (sala cinza com `props/can.tscn`, poça e caixa de teste).
- **Sistemas prontos:**
  - `core/Settings.cs` (autoload: intensidade VHS padrão `0.7`, sensibilidade, volume, idioma `pt`/`en`/`es` e tamanho de legenda em `user://settings.cfg` — único dos 4 autoloads criado até agora).
  - `player/` (`PlayerController.cs` com giro lateral no chute e modo `Watch` com ação opcional, `PlayerBody.cs`, `HandAim.cs`, `FingerCurl.cs`, `player.tscn`).
  - `interaction/` (`Interactor.cs`, `Pickable.cs`, `RefugeChair.cs`, interfaces `IInteractable` com `WorksWhileHolding`, `IReceiver`, `IReacher`, `IViewer` com `WatchPromptKey`, `IDescribed`).
  - `tapes/` (`TapeInfo.cs` com `Stamp`, `TapeDeck.cs` com carimbo `AUTO DATE` de 5,5 s, `Rewinder.cs`, `ShelfGap.cs`, `tape_screen.gdshader`, `tape_shot.tscn`).
  - `dialogue/` (`DialogueLine.cs`, `DialoguePlayer.cs`, `SubtitleBox.cs`, `subtitle_box.tscn`, `RentalLedger.cs`, `CounterSpot.cs`).
  - `npc/` (`CustomerNeide.cs`, `dona_neide.tscn`, `BarOwnerTiao.cs`, `tiao.tscn`, `DogPacoca.cs`, `pacoca.tscn`).
  - `props/tape.tscn`, `props/can.tscn`, `ui/PauseMenu.cs`, `vfx/VhsFilter.cs`, `vfx/Puddle.cs`, `localization/ui.csv` (`pt`, `en`, `es`).
- **Assets gerados (`tools/blender/` → `game/assets/`):** `nondas.glb`, `dona_neide.glb` (+ 4 PNGs), `tiao.glb` (+ 5 PNGs), `pacoca.glb` (+ 1 PNG), `can.glb` (+ 1 PNG), `tape.glb`, `rewinder.glb`, `store/` (glTF separado + 12 PNGs), `facade/` (glTF separado + 11 PNGs), `street/` (glTF separado + 8 PNGs) e `audio/` (`speech/neide_01..06.wav`, `speech/nondas_01..06.wav`, `speech/tiao_01..06.wav`, `sfx/pen_write.wav`, `sfx/pacoca_happy.wav`, `sfx/beer_sip.wav`, `sfx/bar_radio.wav`).

---

## 3. Próxima Entrega — Entrega 9: Noite 1 completa

Referências: `docs/design/03-mvp.md` (Marco 1) e `docs/design/01-story.md` (Noite 1, itens 1–6).

- **Escopo jogável:**
  - Arquitetura de história e persistência: `GameState` (lógica pura em `game/story/` coberta por xUnit em `tests/`), `SaveService` e `LevelLoader` (Autoloads oficiais).
  - **Noite 1 completa do início ao fim:** bilhete do Seu Agenor (regras da loja e aviso da porta do depósito), atendimento à Dona Neide e ao **Freguês** (devolvendo a fita adulterada), escolha de guardar a fita na gaveta do balcão ou devolver à prateleira, baixar a porta de aço móvel no fim do expediente, passar pelo bar com Tião e **Cabo Moreira** na sinuca, atravessar o portão/corredor lateral até a **quitinete**, jantar, dormir e salvar checkpoint.
- **Decisões para alinhar com o dono ANTES de codar:**
  1. Organização da transição para a **quitinete** (cena separada via `LevelLoader` ao atravessar o portão do corredor lateral, aproveitando para resolver os itens da seção 4 sobre `Pickable` na mão e `VhsFilter`/`PauseMenu` persistentes).
  2. Porta de aço móvel da locadora e gaveta do balcão (animação/colisão da porta descendo ao fechar e interação simples de guardar fita na gaveta).
  3. Ajustes menores anotados pelo dono na Entrega 8 (confirmar se algum entra já na Entrega 9 ou em polimento posterior).
- **Fora do escopo desta entrega:** Seu Agenor em pessoa, investigação do caderno/durex (Entrega 10), chuva da Noite 3 (Entrega 11) e perseguição do clímax (Entregas 12–13).
- **Critério de pronto:** jogar a quinta-feira completa do começo ao dormir na quitinete, com troca de cena limpa pelo `LevelLoader`, salvamento em disco (`SaveService`) e suíte `dotnet test` verde em `tests/`.

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
