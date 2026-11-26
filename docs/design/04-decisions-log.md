# Design 04: Histórico de Decisões e Medições por Entrega

Registro histórico das decisões técnicas, experimentos e medições das entregas já concluídas.
Para a arquitetura vigente e regras permanentes, consulte `docs/design/02-stack-architecture.md`. Para o estado atual, consulte `STATUS.md`.

---

## Entrega 1: Renderizador, física e versão do .NET (2026-11-11)

Medido na RTX 3050 Laptop (4 GB) do dono, com Godot 4.7.2, sala cinza de teste, 1920x1080, V-Sync desligado.

| Renderizador | Driver | Quadros por segundo |
|---|---|---|
| **Forward+ (escolhido)** | Vulkan | cerca de 495 |
| Forward+ | Direct3D 12 | cerca de 590 |
| Mobile | Vulkan | cerca de 850 |
| Compatibility | OpenGL 3.3 | cerca de 2440 |

- **Renderizador: Forward+.** Todos passam de 8 vezes a meta de 60 FPS, então a escolha foi por recurso, não por velocidade. Só o Forward+ tem neblina volumétrica e não limita a quantidade de luzes por objeto, necessário na noite 3 (chuva, postes, rua molhada).
- **Driver: Vulkan (padrão do motor).** Direct3D 12 mediu 19% a mais na sala vazia; reavaliado na entrega 5.
- **Física: Jolt Physics** (embutido no Godot, `physics/3d/physics_engine`). Teste descartável com 400 cilindros do tamanho de uma lata caindo num piso fino:

| Motor | Tempo médio de física por quadro | Latas que atravessaram o piso | Latas ainda tremendo após 8 s |
|---|---|---|---|
| Godot Physics | 2,4 ms | 217 de 400 | 179 |
| **Jolt (escolhido)** | 1,9 ms | 101 de 400 | 42 |

  Nenhum dos dois segura objeto pequeno e rápido sem detecção contínua de colisão (`continuous_cd`).
- **Versão do .NET: `net10.0`**, e não o `net8.0` padrão do Godot: o suporte ao .NET 8 terminou em 2026-11-10 e o SDK 10 (LTS até novembro de 2028) já estava instalado. A GPU utilizada pelo motor foi confirmada no log como a NVIDIA dedicada.

---

## Entrega 2: Resolução interna, filtro VHS e interpolação (2026-11-11)

Medido na mesma máquina, sala cinza de teste, tela cheia 1920x1080, V-Sync desligado.

| Configuração | Quadros por segundo |
|---|---|
| Entrega 1: 3D a 1080 linhas, sem filtro | cerca de 495 |
| Entrega 2: 3D a 480 linhas, 1ª versão do filtro (recusada; 10 leituras/pixel) | cerca de 780 (763 a 818) |
| **Entrega 2: 3D a 480 linhas, filtro VHS aprovado (21 leituras/pixel)** | cerca de 595 (575 a 641) |

- **Resolução interna:** só o 3D é reduzido via `Scaling3DScale = 480 / altura` (ampliação bilinear). Menus e textos seguem na resolução da janela (`canvas_items`, base 1280x720). Alternativas descartadas: `viewport` stretch (deixaria texto serrilhado em 480p) e `SubViewport` global (complexidade extra de input).
- **Escolha visual do filtro (`game/vfx/vhs_filter.gdshader`):** a primeira versão com tremor aleatório e faixa de tracking permanente foi recusada. Cinco opções foram comparadas lado a lado numa página HTML/WebGL descartável sobre cena colorida; o dono aprovou a mistura de "fita limpa" com "fita gasta" (ondulação lenta, desvio fixo nas últimas linhas, contorno duro, cor borrada para a direita, granulação e pretos lavados, intensidade padrão `0.7`). Custo medido: ~0,4 ms/quadro a 1080p.
- **Menu de pausa acima do filtro:** mantido acima para legibilidade com filtro no máximo.
- **Interpolação de física ligada:** corpo a 60 ticks/s suavizado pelo motor; cabeça (`top_level`, sem interpolação) posicionada a cada quadro com rotação direta do mouse (essencial em telas >= 120 Hz).

---

## Entrega 3: Objetos soltos, poça e interação (2026-11-12)

Feito na sala de teste com formas primitivas do Godot.

- **Jogador não colide com objetos soltos:** colidir fazia a cápsula subir 0,58 m sobre latas prensadas na parede. Passou a atravessar a camada `props` e chutá-los.
- **Zona inicial de chute:** inicialmente um cilindro nas pernas (`KickZone`, substituído pelos sapatos reais na entrega 4). Com cilindro de corpo inteiro, soltar a fita andando a arremessava 2,25 m porque o tronco a atingia no ar.
- **Poça (`Puddle.cs`, `puddle.gdshader`):** plano com até 8 ondulações simultâneas (ao entrar e a cada 0,5 m andado), material local por instância.
- **Interação e pegada inicial (`Interactor.cs`, `Pickable.cs`):** `RayCast3D` de 2 m, varredura contra `world` com 2 cm de folga para não atravessar paredes ao segurar, dica acima do filtro VHS (camada 5).
- **Escala 3D em 4K:** confirmado que o limite real do motor é `0.1` (não `0.25`); piso ajustado no código para manter 480 linhas mesmo em 2160p.

---

## Entrega 4 (Parte 1): Corpo do Nondas (2026-11-12 e 2026-11-13)

- **Modelo (`nondas.glb` via `tools/blender/nondas_body.py`):** malha base e esqueleto `game_engine` do MPFB (CC0), homem de 19 anos (`age` 0,366), olhos a 1,6 m, pele parda, camiseta clara, jeans e Kichute preto (escolhido pelo dono entre 3 opções renderizadas na câmera do jogo). Sapatos feitos pelo casco convexo subdividido dos pés; cabeça/pescoço separados na malha `Head` apenas como sombra (`ShadowOnly`).
- **Animações por código:** 5 ações de 1 s (`idle`, `walk_forward`, `walk_backward`, `walk_left`, `walk_right`), ciclo frontal de 1,05 m (`StrideLength`) sem deslize; passo lateral curto com ~60% de deslize aprovado pelo dono. Velocidade reduzida de 3,0 para **2,0 m/s** para ritmo natural de passadas.
- **Chute e poça pelos pés:** `Area3D` do tamanho do sapato (11×28×9 cm) em cada pé via `BoneAttachment3D` (camada 4 `feet`). Raio do jogador ao objeto contra `world` bloqueia chutes através de paredes finas. Poça filtra `TouchHeight` (10 cm) para ignorar pés no ar ou objetos voando alto.

---

## Entrega 4 (Parte 2): Mão do Nondas e pegada com IK (2026-11-13 e 2026-11-14)

- **Cinemática inversa em tempo real:** 3 modificadores filhos do `Skeleton3D` após `AnimationTree`:
  1. `TwoBoneIK3D` (ombro, cotovelo, pulso até o nó `Grip`);
  2. `HandAim.cs` (alinha a mão aos eixos do `Grip`, dividindo 60% da torção no antebraço pela ausência de twist bones no MPFB);
  3. `FingerCurl.cs` (fecha dedos em torno do eixo X local para pegada pela ponta).
- **Distância dinâmica e paredes (`PlayerBody.Place`):** corpo a 12 cm atrás da câmera olhando à frente e 32 cm olhando para baixo (transição entre 10° e 40°). Prisma varrido contra paredes empurra o corpo para baixo da câmera (máximo 6 mm de penetração em 64 casos testados); pescoço tampado com cor da camiseta (`close_neck`).
- **Coreografia de pegar (`Interactor`, 1,05 s):** inclinar/agachar (`stoop`, 0,45 s) + fechar mão (0,15 s, item desliza até 10 cm se necessário) + levantar segurando pela ponta (0,45 s). Mira tolerante com esfera de 8 cm quando o raio fino erra.
- **Sincronia quadro/física:** corpo posicionado apenas no quadro de renderização (posicioná-lo também no tick de física gerava tremor de 3,6 cm na mão a 144 FPS).

---

## Entrega 5: A locadora (2026-11-14)

- **Planta e modelo (`store.py` → `store.gltf`):** interior 6,0 × 10,0 m, pé-direito 2,9 m; salão com prateleiras laterais, gôndola central dupla (permite contornar no clímax), balcão à direita, estante de fitas por gênero e TV atrás do balcão; fundos com seção adulta atrás de cortina, corredor com porta dos fundos e depósito separado (para o Freguês não escapar pela porta dos fundos na armadilha).
- **Exportação e texturas:** glTF separado (`.gltf` + `.bin` + 12 PNGs procedurais com semente fixa) para evitar regravar ~8,5 MB binários a cada ajuste. Mipmaps e compressão VRAM ligados nos `.import`. Material `floor.tres` aplicado sobre o piso para ativar `vertex_color_use_as_albedo`.
- **Refinamento de alcance e caminho varrido:** linha ombro→pulso (esfera 3 cm) barra pegar através de balcão/grade; caminho da fita planejado em 16 trechos (arco direto ou puxando até 40 cm para fora sob prateleiras); câmera acompanha os olhos da pose agachada com recuo `HeadRoom`. Fechadas frestas de 3 cm na porta dos fundos e 10 cm entre estante e móvel da TV.
- **Desempenho na loja completa:** ~410 FPS em Vulkan (1080p, 7 luzes com sombra + filtro VHS) e ~530 FPS em D3D12; mantido Vulkan como padrão.

---

## Entrega 6: As fitas e pipeline visual com Gemini (2026-11-19)

- **Pipeline de arte:** transição da geração de scripts do Blender (`facade.py` e `rewinder.py`) para o Gemini. A rebobinadeira rodou de primeira; na fachada houve apenas 2 ajustes mecânicos pontuais de broadcast `numpy` na revisão, com geometria determinística.
- **Rotina das fitas:** 4 fitas devolvidas na bandeja (Aventura, Suspense, Terror e Comédia adulterada), paradas no fim. O videocassete (`TapeDeck`) exibe créditos finais ("FIM"), tela preta e chuvisco — ou, na fita adulterada, ~10 s da fachada noturna filmada da calçada oposta — seguido de tela azul (*Blue Back*) e ejeção. A rebobinadeira (`Rewinder`, 4 s) exige que a fita tenha sido testada até o fim antes de rebobinar (evitando apagar a pista antes de vê-la). A estante (`ShelfGap`) só aceita o gênero correto.
- **Colocação reversa (`IReceiver`, camada 5 `slots`):** colocar item reproduz a coreografia de pegada ao contrário sobre o mesmo caminho planejado e varrido, finalizando em `Pickable.SetDown`.
- **Modo assistir (`IViewer`) e `SubViewport`:** jogador desliza e inclina até 40 cm da TV com olhar semi-preso (±17°). `SubViewport` 320×240 (`tapes/tape_shot.tscn`) só renderiza enquanto há imagem na TV (0,17 ms de GPU/quadro). Shader `tape_screen.gdshader` opera em sRGB antes de converter para linear para preservar detalhes noturnos da fachada sob granulado de cópia VHS.

---

## Entrega 7: Diálogo, Dona Neide, idiomas e carimbo (2026-11-26)

- **Dona Neide (`dona_neide.py` → `dona_neide.glb`):** gerada via MPFB 2.0.18 (CC0) com idade ~60 anos, olhos preservados com UV frontal, cabelo grisalho em coque, cardigã/calça/sapatos com texturas procedurais empacotadas e animações `idle`, `walk` (ciclo `0,75 m`) e `talk`.
- **Entrada e atendimento em `store.tscn`:** porta de aço a meia-altura (`SteelDoorHalf`, 1,80 m livres) exibindo `facade.gltf` noturna fora e mantendo o colisor `SteelDoor-convcolonly` na soleira contra saída precoce do jogador e de fitas chutadas. Vão `Romance` aberto em `TapeRack` (`store.py`) com 3 fitas pegáveis; `IInteractable.WorksWhileHolding` permite anotar no caderno (`RentalLedger`, `Label3D` com `AlphaCut = Discard`) mesmo segurando a fita; `CounterSpot` aceita apenas a fita pedida e funciona em qualquer ordem com o caderno.
- **Diálogo, áudio e localização:** `SubtitleBox` + `DialoguePlayer` (sinal `GestureChanged`, atualização ao vivo ao alterar idioma `pt`/`en`/`es` ou tamanho `20/26/34 px` no `PauseMenu`). Ruído de fala sintetizado deterministicamente por formantes vocais em `tools/blender/speech_audio.py` (`neide_01..06.wav`, `nondas_01..06.wav` e `pen_write.wav`), dispensando gravação externa ou instalação do Audacity.
- **Carimbo `AUTO DATE` (`TapeDeck`):** exibe `22:48` / `12. 06. 96` diretamente no `SubViewport` `Feed` durante os primeiros `5,5 s` de `Part.Shot`, passando pelo shader analógico da TV.
