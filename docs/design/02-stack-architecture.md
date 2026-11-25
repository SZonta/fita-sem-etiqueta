# Design 02: Stack, Arquitetura e Regras Técnicas

Etapa 2 do projeto (aprovado em 2026-11-11; atualizado em 2026-11-25 para condução integral no Antigravity/Gemini).
Histórico detalhado de benchmarks e decisões por entrega: `docs/design/04-decisions-log.md`.

---

## 1. Stack e Requisitos Técnicos

| Item | Decisão | Observação |
|---|---|---|
| Motor | Godot 4.7.2, edição .NET | Renderizador **Forward+ (Vulkan)**; física **Jolt Physics** com interpolação ativa |
| Linguagem | C# (`net10.0`) | SDK .NET 10 LTS (suporte ao .NET 8 encerrou em 2026-11-10) |
| Resolução e VHS | 3D a ~480 linhas (`Scaling3DScale`, mín. `0.1`) | UI e legendas na resolução nativa (`canvas_items`, base 1280×720). Filtro VHS limpo (padrão `0.7`), sem faixas/piscadas |
| Cenário e objetos | 100% próprios via Python no Blender 5.2.4 LTS | Texturas procedurais determinísticas (`numpy` com semente fixa), sem fotos externas |
| Personagens humanos | Malha base MPFB 2.0.18 (CC0) + roupa/rig/animações por script | Licença CC0 registrada em `docs/credits.md` (pacotes CC-BY não são usados) |
| Paçoca (cachorro) | Modelado e animado do zero por script no Blender | Nunca sofre dano no jogo |
| Áudio | Acervos gratuitos **somente CC0** + gravações próprias | Todo som externo registrado em `docs/credits.md`; "ruídos de fala" gravados e editados no Audacity |
| Idiomas | Português (original), Inglês e Espanhol | Troca dentro do jogo; falas curtas sem voz dublada |
| Microfone | Opcional, desligado por padrão | Última entrega de mecânica (Marco 3) |
| Plataforma-alvo | Windows (Steam) | Meta mínima: 60 FPS estáveis na RTX 3050 Laptop 4 GB (atual: ~410 FPS na loja) |

### Mecânicas de Imersão Obrigatórias (Já Implementadas)
1. Objetos soltos reagem fisicamente ao chute dos pés do jogador.
2. Corpo visível em primeira pessoa ao olhar para baixo.
3. Poça d'água ondula a cada pisada.
4. Mão direita se estica com cinemática inversa (IK) e agachamento para pegar e pôr itens.

---

## 2. Licenciamento Dual e Repositórios

| Conteúdo | Licença | Onde fica |
|---|---|---|
| Código-fonte (C#, shaders, config de projeto/build, testes) | MIT (`LICENSE`) | Repositório público `SZonta/fita-sem-etiqueta` |
| Modelos, texturas, áudio, cenas, textos, história e `docs/` | Todos os direitos reservados (`LICENSE-ASSETS.md`) | Repositório público (visível para estudo, vedada redistribuição/compilação comercial ou pessoal com a arte) |
| Scripts geradores do Blender (`tools/blender/`) e referências | Privado / Não publicado | Repositório privado aninhado `SZonta/fita-sem-etiqueta-art` (ignorado no `.gitignore` público para evitar efeito GPL da API `bpy`) |

---

## 3. Arquitetura de Software (Analogia C# / React)

| No Godot | Equivale a | Uso no projeto |
|---|---|---|
| Cena (`.tscn`) | Componente React | Uma cena por elemento reutilizável: jogador, fita, porta, NPC |
| Nó | Elemento interno | Responsabilidade única por nó |
| `[Export]` | Props | Configuração ajustável no editor |
| Sinal | Evento / callback | O filho avisa, o pai decide |
| Autoload | Singleton / Context global | Restrito aos 4 oficiais abaixo |
| Resource (`.tres`) | DTO / config tipada | Dados de missão, diálogo e final |
| Grupo | Tag / seletor | Marcar esconderijos e fontes de ruído |

### Regras Invariantes de Arquitetura
1. **Composição, não herança:** comportamento novo entra como nó pequeno acoplado à cena.
2. **Chamadas descem, sinais sobem:** pai invoca métodos do filho; filho emite sinais para cima.
3. **Fonte única de verdade da história:** `GameState` concentra todas as flags narrativas (estilo store Redux).
4. **Domínio da história em C# puro:** resolução de missões e finais em `game/story/` sem tipos do Godot, 100% testada com **xUnit** em `tests/`.
5. **Zero texto visível no código:** toda string de interface, placa ou fala usa chave de tradução em `game/localization/`.
6. **Nada especulativo (YAGNI):** pastas e sistemas só nascem na entrega que os consome.
7. **Apenas 4 Autoloads permitidos:** `GameState`, `Settings`, `SaveService`, `LevelLoader` (qualquer novo exige justificativa documentada aqui).

### Sistemas e Estrutura Alvo

| Sistema | Papel |
|---|---|
| `player/` | Movimento 1ª pessoa (`PlayerController`), corpo visível (`PlayerBody`) e IK da mão (`HandAim`, `FingerCurl`) |
| `interaction/` | Raio/esfera da câmera (`Interactor`), itens pegáveis (`Pickable`) e alvos (`IInteractable`, `IReceiver`) |
| `tapes/` | Fitas (`TapeInfo`), VCR/TV (`TapeDeck`), rebobinadeira (`Rewinder`), vãos de gênero (`ShelfGap`) e gravação ao vivo |
| `story/` | Máquina de missões por noite, flags e resolução pura dos 4 finais |
| `dialogue/` | Legenda curta, "ruído de fala" e gatilho de gesto por chave de tradução |
| `noise/` | Barramento único de ruído (correr, lata, poça, microfone opcional) consumido pela IA no clímax |
| `npc/` | NPCs de rotina e máquina de estados do Freguês (rondar, investigar ruído, procurar, perseguir) |
| `ui/` e `vfx/` | Menu de pausa/opções, legendas, epílogo, filtro VHS, poças e chuva |

---

## 4. Camadas de Física e Regras de Montagem de Fase

### Matriz de Colisão 3D (`project.godot`)

| Quem | Camada (`collision_layer`) | Enxerga (`collision_mask`) |
|---|---|---|
| Cenário estático (paredes, piso, móveis) | 1 `world` (`1`) | — |
| Jogador (`CharacterBody3D`) | 2 `player` (`2`) | 1 (`1`) |
| Objetos soltos (lata, fita) | 3 `props` (`4`) | 1 e 3 (`5`) |
| Sapatos do jogador (`BoneAttachment3D`) | 4 `feet` (`8`) | 3 (`4`) |
| Lugares de encaixe (`IReceiver`: VCR, rebobinadeira, estante) | 5 `slots` (`16`) | — (detectados por raio de áreas) |
| Raio de interação (`Interactor`) / Poça (`Area3D`) | nenhuma (`0`) | Raio: 1 e 3 / Poça: 3 e 4 |

### Regras Obrigatórias para Novos Objetos e Fases (Prevenção de Softlock)
- **Objetos soltos pequenos:** ligar sempre `continuous_cd = true`, `collision_layer = 4` e `collision_mask = 5` (o jogador atravessa `props` e os chuta pelos pés; objetos que devem bloquear passagem ficam em `world`).
- **Alcance real da mão do Nondas:** até ~65 cm do eixo do jogador sobre balcão de 1 m (~30 cm da borda), até ~50 cm à frente no chão e até **1,70 m de altura** em prateleiras. Posicionar itens interativos sempre dentro desse envelope.
- **Folga vertical em prateleiras:** manter **pelo menos 2 cm livres acima** de qualquer item pegável sob tábua (abaixo disso, a varredura Jolt ignora a tábua sobreposta na saída e a mão a atravessa). Fitas em estantes ficam em pé, lombada para fora, na borda.
- **Zero frestas >= 3 cm:** vedar vãos entre móveis e paredes onde uma fita de missão chutada possa cair fora de alcance.
- **Teleporte do jogador (troca de fase / checkpoint):** com interpolação de física ligada, chamar obrigatoriamente `ResetPhysicsInterpolation()` após reposicionar o jogador.

---

## 5. Pipeline de Arte no Antigravity

Todo o pipeline visual roda integrado no Antigravity (modelos Gemini + Blender 5.2.4 LTS + MPFB CC0):

- **Cenários, props e personagens (`tools/blender/*.py`):** gerados por código Python determinístico (`--background`), apenas Principled BSDF e texturas procedurais com semente fixa. Props/personagens exportam `.glb`; cenários grandes exportam **glTF separado** (`.gltf` + `.bin` + PNG) com colisores `-convcolonly`.
- **Regras operacionais completas** (parâmetros `.import` de mipmaps/compressão, `vertex_color_use_as_albedo`, referências visuais e páginas de comparação de opções): seguir obrigatoriamente a skill `fse-arte-blender`.

---

## 6. Riscos Mapeados e Mitigações

| Risco | Mitigação |
|---|---|
| Softlocks (reclamação nº 1 do gênero) | Envelope estrito de alcance/frestas, testes headless automatizados e auditoria dedicada (`fse-revisao-jogo`) a cada entrega |
| Custo de personagens semi-realistas | Resolução interna de 480 linhas + filtro VHS + iluminação noturna + conjunto enxuto de animações por NPC |
| Escopo no teto (6 NPCs, armadilha, final secreto, microfone) | MVP fatiado em 3 marcos: no fim do Marco 2 (Entrega 14) o jogo já é completo e publicável |
| Cláusula GPL sobre scripts `bpy` e exposição pública de assets | Scripts isolados no repo privado `fita-sem-etiqueta-art`; áudio de terceiros restrito a CC0 auditado em `docs/credits.md` |
