# STATUS.md — Estado Atual e Próximas Entregas

Última atualização: 2026-11-25 (reestruturação para o ecossistema Gemini/Antigravity; jogo na Entrega 6 aprovada).
Atualizar este arquivo **sempre antes de cada commit de entrega**.

---

## 1. Progresso Geral

- **Etapa 1 — Pesquisa (*Fears to Fathom*):** concluída e aprovada (2026-11-11). Ver `docs/research/01-fears-to-fathom.md`.
- **Etapa 2 — Planejamento (História, Stack, Arquitetura e MVP):** concluída e aprovada (2026-11-11). Ver `docs/design/01-story.md`, `02-stack-architecture.md` e `03-mvp.md`.
- **Etapa 3 — Desenvolvimento:** **6 de 19 entregas concluídas** (Marco 1: *Noite 1 jogável*, entregas 1 a 9):
  - [x] **Entrega 1:** Instalação e projeto vazio (sala cinza, 1ª pessoa, Forward+ Vulkan, Jolt, `net10.0`) — 2026-11-11
  - [x] **Entrega 2:** Filtro VHS e menu de pausa/opções (`Settings` autoload, 480 linhas 3D) — 2026-11-11
  - [x] **Entrega 3:** Física e interação (lata chutável, poça com ondulação, `Interactor` + `Pickable`) — 2026-11-12
  - [x] **Entrega 4:** Corpo do Nondas — Parte 1 (pernas, caminhada, chute/poça pelos pés) e Parte 2 (mão direita com IK, agachar para pegar, recuo contra paredes) — 2026-11-13 / 2026-11-14
  - [x] **Entrega 5:** A locadora (*Astro Vídeo Locadora*, `store.gltf`, 7 luzes, placas traduzidas, caminho varrido da pegada) — 2026-11-14
  - [x] **Entrega 6:** As fitas (bandeja de devolução, videocassete/TV com gravação ao vivo da fachada noturna, rebobinadeira, estante por gênero) — 2026-11-19
  - [ ] **Entrega 7 (PRÓXIMA):** Diálogo e idiomas

Histórico técnico detalhado das entregas 1–6: `docs/design/04-decisions-log.md`.

---

## 2. O Que Já Existe em `game/`

- **Cena principal (F5):** `levels/store/store.tscn` (*Astro Vídeo Locadora* com `store.gltf`, `floor.tres`, 7 luzes com sombra, `ReflectionProbe`, placas `Label3D` traduzidas, jogador, filtro VHS, menu de pausa, 4 fitas devolvidas em `ReturnedTapes`, 4 vãos em `ShelfGaps`, `TapeDeck` e `Rewinder`).
- **Cena de teste:** `levels/test_room/test_room.tscn` (sala cinza com lata vermelha, poça e caixa de teste).
- **Sistemas prontos:**
  - `core/Settings.cs` (autoload: intensidade VHS padrão `0.7`, sensibilidade, volume em `user://settings.cfg` — único dos 4 autoloads criado até agora).
  - `player/` (`PlayerController.cs`, `PlayerBody.cs`, `HandAim.cs`, `FingerCurl.cs`, `player.tscn`).
  - `interaction/` (`Interactor.cs`, `Pickable.cs`, interfaces `IInteractable`, `IReceiver`, `IReacher`, `IViewer`, `IDescribed`).
  - `tapes/` (`TapeInfo.cs`, `TapeDeck.cs`, `Rewinder.cs`, `ShelfGap.cs`, `tape_screen.gdshader`, `tape_shot.tscn`).
  - `props/tape.tscn`, `ui/PauseMenu.cs`, `vfx/VhsFilter.cs`, `vfx/Puddle.cs`, `localization/ui.csv` (`pt`, `en`, `es`).
- **Assets 3D gerados (`tools/blender/` → `game/assets/models/`):** `nondas.glb`, `tape.glb`, `rewinder.glb`, `store/` (glTF separado + 12 PNGs), `facade/` (glTF separado + 11 PNGs, base da rua da Entrega 8).

---

## 3. Próxima Entrega — Entrega 7: Diálogo e Idiomas

Referências: `docs/design/03-mvp.md` (Marco 1) e `docs/design/01-story.md` (Noite 1, item 2).

- **Escopo jogável:**
  - Atender **Dona Neide** no balcão: ela chega e pede uma fita; Nondas localiza a fita e registra a locação no caderno.
  - Sistema de diálogo sem voz: legenda na parte inferior da tela + "ruído de fala" + gesto corporal. Falas curtas por chave de tradução (`fse-localizacao`).
  - Troca de idioma (`pt`, `en`, `es`) e tamanho da legenda no menu de pausa.
  - **Primeiro NPC além do Nondas (Dona Neide):** aparência no Blender via MPFB (rosto, roupa, materiais) + esqueleto, pesos e animações (`idle`, falando com gesto, andando).
  - **Áudio inicial:** "ruído de fala" gravado pelo dono e tratado no **Audacity** (pedir autorização explícita de instalação antes, informando origem `audacityteam.org`, tamanho confirmado na fonte e motivo). Efeitos de terceiros somente **CC0** registrados em `docs/credits.md`.
- **Decisões para alinhar com o dono ANTES de codar:**
  1. Por onde Dona Neide entra/aparece, já que a porta de aço está fechada e a rua só entra na Entrega 8.
  2. Como funciona na prática achar a fita pedida (estante atrás do balcão já tem vãos por gênero) e interagir com o caderno.
  3. Carimbo de data na gravação da fita adulterada (pendência da Entrega 6: mostrar a data da véspera nos primeiros segundos da fachada para ensinar o jogador a ler o carimbo que entrega o clímax na Noite 3).
- **Fora do escopo desta entrega:** rua e bar (8), Freguês, Cabo Moreira, máquina de missões/`GameState` (9), sistema de ruído.
- **Critério de pronto:** com F5, o dono atende Dona Neide do início ao fim, lê as legendas nos 3 idiomas e alterna idioma e tamanho de legenda no menu.

---

## 4. Pendências e Pontos Vigiados para Entregas Futuras

### Para a Entrega 8 (A rua e o bar)
- Converter a lata de `test_room.tscn` em cena própria reutilizável; avaliar variar o giro do chute caso o tombamento determinístico pareça repetitivo na rua.
- Validar texturas procedurais de **chapisco** e **caquinho** sob o filtro VHS e auditar frestas >= 3 cm na calçada/rua (risco de perder item chutável).
- Se o dono reclamar do chute passando por cima da lata (passada sobe 10 cm): reduzir passada para 5 cm e ajustar `TouchHeight` da poça (hoje 10 cm).

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
