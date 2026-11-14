# Design 02: Stack e arquitetura

Etapa 2 do projeto. Status: **aprovado pelo dono em 2026-11-11**, com um ajuste já incorporado (áudio pode usar bases existentes).

## 1. Stack decidida

| Item | Decisão | Observação |
|---|---|---|
| Motor | Godot 4, edição .NET | Versão estável em 2026-11-11: 4.7.2 (fonte: godotengine.org) |
| Linguagem | C# | Exige .NET SDK 8 ou superior (fonte: documentação oficial do Godot) |
| Estilo visual | Semi-realista sob filtro VHS | Proporções reais, geometria moderada, textura de resolução modesta |
| Cenário e objetos | 100% próprios, gerados por script no Blender | Texturas procedurais, sem fotos |
| Personagens humanos | Malha base de gerador gratuito (MPFB/MakeHuman) + rosto, roupa, textura e animação próprios | Licença confirmada em 2026-11-12: assets CC0, ver `docs/credits.md` |
| Paçoca | Modelado do zero | |
| Áudio | Bases existentes + edição própria | Efeitos genéricos (lata, latido, chuva, passos) vêm de acervos gratuitos. **Somente licença CC0**, porque o repositório é público e publicar o arquivo conta como redistribuição. Cada som é registrado com fonte e licença em `docs/credits.md`. Os "ruídos de fala" dos personagens são próprios |
| Microfone | Opcional, desligado por padrão | Última entrega do MVP |
| Plataforma | Windows (Steam) | C# no Godot 4 não exporta para web; não afeta o projeto |

### Linguagens comparadas (registro da decisão)

| Linguagem | Prós | Contras |
|---|---|---|
| **C# (escolhida)** | O dono já domina; tipagem forte; refatoração e depuração no VS Code; desempenho de sobra | Edição .NET do Godot e .NET SDK; tutoriais da comunidade majoritariamente em GDScript; recompilar para ver novos campos exportados no editor |
| GDScript | Embutida; iteração mais rápida; quase todo tutorial usa | Linguagem nova para o dono; refatoração fraca; erros de tipo tardios |
| C++ | Desempenho máximo | O dono não conhece bem; compilação lenta; toolchain de vários GB; nenhum gargalo de CPU no jogo que justifique |

### Ferramentas a instalar na Etapa 3

Cada item exige autorização explícita do dono no momento da instalação, com tamanho confirmado na fonte. Os três primeiros foram instalados na entrega 1 (2026-11-11); Blender e MPFB na entrega 4 (2026-11-12).

| Ferramenta | Origem oficial | Para quê | Tamanho |
|---|---|---|---|
| Godot 4.7.2, edição .NET (instalado) | godotengine.org | Motor do jogo | 111 MB |
| .NET SDK 10.0.404, LTS (instalado) | dotnet.microsoft.com | Compilar o C# | 208 MB |
| Extensão C# 2.160.4 para VS Code (instalada) | Marketplace do VS Code (Microsoft) | Autocompletar e depurar | 130 MB |
| Blender 5.2.4 LTS, zip portátil (instalado) | blender.org | Modelos, rig, animação. Já inclui o Python usado pelos scripts | 387 MiB |
| MPFB 2.0.18, complemento do Blender (instalado) | extensions.blender.org (mantido pela comunidade MakeHuman) | Malha base dos humanos | 43,8 MiB |
| Audacity | audacityteam.org | Cortar e tratar os sons baixados; gravar os "ruídos de fala" | A confirmar |

## 2. Arquitetura: o Godot traduzido para quem vem de C# e React

| No Godot | Equivale a | Como usamos |
|---|---|---|
| Cena (`.tscn`) | Componente React | Uma cena por coisa reutilizável: jogador, fita, porta, NPC |
| Nó | Elemento dentro do componente | Cada nó faz uma coisa só |
| `[Export]` | Props | Configuração ajustável no editor, sem mexer em código |
| Sinal | Evento / callback (`event` do C#) | O filho avisa, o pai decide |
| Autoload | Serviço singleton da injeção de dependência, ou um Context global | Poucos, só para estado global de verdade |
| Resource (`.tres`) | DTO tipado / arquivo de configuração | Dados de missão, de diálogo e de final |
| Grupo | Tag / classe CSS | Marcar "esconderijos", "fontes de ruído" |
| `_Process(delta)` | Loop de renderização | Só onde for preciso; o resto reage a sinais |

### Regras de arquitetura

1. **Composição, não herança.** Comportamento novo é um nó pequeno acoplado à cena, como um hook ou um componente filho.
2. **Chamadas descem, sinais sobem.** O pai chama métodos do filho; o filho emite sinal. Igual a "props descem, eventos sobem".
3. **Uma fonte de verdade para a história.** `GameState` guarda as flags (fita guardada, contou ao Moreira, leu o bilhete). É o equivalente a uma store do Redux: ninguém mais guarda cópia.
4. **Lógica de história em C# puro, sem tipos do Godot.** A função que decide o final recebe as flags e devolve o final, como um reducer. Testada com xUnit, sem abrir o jogo.
5. **Dados fora do código.** Missões de cada noite, falas e condições de final ficam em arquivos de dados. Texto nunca fica escrito no código: sempre uma chave de tradução.
6. **Nada especulativo.** Um sistema só é criado quando a entrega atual precisa dele.

### Sistemas

| Sistema | O que faz | Analogia |
|---|---|---|
| **Player** | Movimento em primeira pessoa, corpo visível, mão que se estica | Componente raiz com filhos especializados |
| **Interaction** | Raio que sai da câmera; o objeto atingido implementa `IInteractable` | Interface + handler de evento |
| **Story** | Lista de missões da noite (máquina de estados), flags, resolução do final | Wizard de etapas + reducer |
| **Dialogue** | Legenda, "ruído de fala", gesto; falas por chave de tradução | i18n com chaves, como `next-intl` |
| **Noise** | Todo barulho (correr, lata, poça, microfone opcional) vira um evento de ruído com posição e volume | Um barramento de eventos com vários produtores |
| **Stalker AI** | Máquina de estados do Freguês: rondar, investigar ruído, procurar, perseguir | Máquina de estados finitos (como XState) |
| **Save** | Checkpoint automático: grava `GameState` em JSON | `localStorage` com um DTO serializado |
| **Settings** | Volume, sensibilidade, legenda, idioma, intensidade do VHS, microfone | Context de preferências |
| **VFX** | Filtro VHS em tela cheia, chuva, ondulação de poça | Camada de pós-processamento, como um filtro CSS global |

O sistema **Noise** amarra as mecânicas exigidas ao clímax: a lata chutada e a poça pisada são divertidas nas noites 1 e 2 e viram perigo na noite 3, porque o Freguês ouve.

O microfone opcional é só mais um produtor de eventos de ruído. Com ele desligado, nada muda no resto do jogo.

### Autoloads (4)

`GameState`, `Settings`, `SaveService`, `LevelLoader`. Nenhum outro sem justificativa registrada aqui.

### Estrutura de pastas

```
game/                 projeto Godot (project.godot e .csproj)
  core/               autoloads
  player/
  interaction/
  story/              missões, flags, resolução de finais (C# puro)
  dialogue/
  noise/
  npc/
  levels/             store/, street/, kitchenette/
  ui/                 pausa, opções, legenda, epílogo
  vfx/                shaders VHS, chuva, poça
  assets/             models/, textures/, audio/, fonts/
  localization/       pt, en, es (CSV)
tests/                xUnit para a lógica de história
tools/blender/        scripts que geram cenário, objetos e personagens (repositório privado
                      aninhado, ignorado pelo público; ver CONTEXT.md, Licença)
docs/
```

Organização por funcionalidade, como pastas de feature em um projeto Next.js, e não por tipo de arquivo.

## 3. Pipeline de arte

1. **Cenário e objetos:** um script Python por objeto em `tools/blender/`, executado pelo Blender em modo de linha de comando. O script cria a geometria e o material procedural, assa a textura em PNG e exporta `.glb`. O modelo é reproduzível a partir do código, como infraestrutura como código.
2. **Personagens:** malha base do gerador, depois rosto, roupa e textura ajustados; esqueleto padrão; animações feitas por quadros-chave. Poucas animações por personagem: parado, falando com gesto, andando. O Freguês tem também procurar e correr.
3. **Formato de troca:** glTF 2.0 (`.glb`), aberto e importado nativamente pelo Godot.
4. **Áudio:** efeitos genéricos baixados de acervos CC0 (o agente de pesquisa indica a fonte e confere a licença de cada arquivo); "ruídos de fala" gravados pelo dono. Tudo editado no Audacity e exportado em OGG. Registro em `docs/credits.md`.

## 4. Renderização e desempenho

- Alvo: 60 quadros por segundo na RTX 3050 de 4 GB do dono, com folga para PCs mais fracos.
- **Resolução interna baixa (em torno de 480 linhas), ampliada para a tela.** É fiel ao VHS e reduz muito o custo de vídeo. É também o que torna o "semi-realista" viável: a baixa resolução e o filtro escondem a simplicidade dos modelos.
- Filtro VHS como pós-processamento único, com intensidade ajustável no menu.
- Renderizador e motor de física: decididos na entrega 1 (2026-11-11), depois de medir na máquina do dono. Ver abaixo.

### Decisão da entrega 1: renderizador, física e versão do .NET

Medido na RTX 3050 Laptop do dono, com Godot 4.7.2, sala cinza de teste, 1920x1080, V-Sync desligado.

| Renderizador | Driver | Quadros por segundo |
|---|---|---|
| **Forward+ (escolhido)** | Vulkan | cerca de 495 |
| Forward+ | Direct3D 12 | cerca de 590 |
| Mobile | Vulkan | cerca de 850 |
| Compatibility | OpenGL 3.3 | cerca de 2440 |

- **Renderizador: Forward+.** Todos passam de 8 vezes a meta de 60 quadros por segundo, então a escolha é por recurso, não por velocidade. Só o Forward+ tem neblina volumétrica e não limita a quantidade de luzes por objeto, e a noite 3 (chuva, postes, rua molhada) precisa disso. A resolução interna baixa (entrega 2) reduz ainda mais o custo.
- **Driver: o padrão do motor (Vulkan).** O Direct3D 12 mediu 19% a mais, mas numa sala vazia. Reavaliar quando existir uma cena de verdade (entrega 5).
- **Física: Jolt Physics** (embutido no Godot, `physics/3d/physics_engine`). Teste descartável com 400 cilindros do tamanho de uma lata caindo num piso fino:

| Motor | Tempo médio de física por quadro | Latas que atravessaram o piso | Latas ainda tremendo após 8 s |
|---|---|---|---|
| Godot Physics | 2,4 ms | 217 de 400 | 179 |
| **Jolt (escolhido)** | 1,9 ms | 101 de 400 | 42 |

  Nenhum dos dois segura objeto pequeno e rápido sem detecção contínua de colisão. Na entrega 3, a lata precisa de `continuous_cd` ligado.
- **Versão do .NET: `net10.0`**, e não o `net8.0` que o Godot gera por padrão. Motivos: o suporte ao .NET 8 terminou em 2026-11-10; o SDK 10 (LTS até novembro de 2028) já traz o que o `net10.0` precisa, e o `net8.0` exigiria baixar um pacote extra. Editor e jogo carregaram sem erro. **A validar na entrega 19:** exportação para Windows com `net10.0`.
- **A GPU usada é a NVIDIA**, não a Intel integrada do notebook (conferido no log do Godot).
- Limite da medição: uma sala cinza não tem textura, personagem nem filtro. Os números provam folga, não o desempenho final.

### Decisão da entrega 2: resolução interna, filtro VHS e interpolação

Medido em 2026-11-11 na mesma máquina, sala cinza de teste, tela cheia 1920x1080, V-Sync desligado.

| Configuração | Quadros por segundo |
|---|---|
| Entrega 1: 3D a 1080 linhas, sem filtro | cerca de 495 |
| Entrega 2: 3D a 480 linhas, primeira versão do filtro (recusada; 10 leituras da imagem por pixel) | cerca de 780 (de 763 a 818) |
| **Entrega 2: 3D a 480 linhas, filtro VHS aprovado (21 leituras por pixel)** | cerca de 595 (de 575 a 641) |

- **Resolução interna:** só o 3D é reduzido, pela escala 3D do viewport (`Scaling3DScale` = 480 / altura da janela, ampliação bilinear). Menus e textos continuam na resolução da janela. Alternativas descartadas: o modo de esticamento `viewport` (amplia sem suavização e deixaria também o texto em 480 linhas) e um `SubViewport` (mais peças: encaminhamento de entrada e troca de fases dentro dele).
- **Limite:** o código não deixa a escala 3D passar abaixo de 0,25, o mínimo oferecido nas configurações de projeto. Em janela com mais de 1920 pixels de altura (4K), o 3D fica com 1/4 da altura (540 linhas em 2160), e não 480. **Não conferido:** o limite real do motor pode ser 0,1; se for, o piso pode cair e o 4K teria 480 linhas. (Conferido na entrega 3: o limite é 0,1 e o piso caiu. Ver abaixo.)
- **Intensidade na medição:** a medição do filtro aprovado usou a intensidade que estava salva no `settings.cfg` do dono naquele momento, não registrada. O shader faz as mesmas 21 leituras em qualquer intensidade, então o custo é o mesmo.
- **Filtro VHS:** um único shader de tela cheia (`game/vfx/vhs_filter.gdshader`) desenhado por cima da imagem pronta. Aparência de câmera antiga, sem nada que pisque ou corra pela tela: ondulação lenta, desvio fixo nas últimas linhas, contorno duro nas bordas, cor larga que borra para a direita e vaza das linhas de cima, granulação, pretos lavados e cor forte. A intensidade (0 a 1) multiplica todos os efeitos; em 0 a imagem passa intacta.
- **Como o filtro foi escolhido:** a primeira versão (tremor aleatório por linha, faixa de tracking permanente) foi recusada pelo dono. Cinco opções foram comparadas numa página de teste descartável, sobre uma cena colorida; o dono aprovou a mistura da "fita limpa" (sem faixa, sem riscos, sem piscadas) com a "fita gasta" (imagem de câmera antiga). Regra que fica: defeitos que piscam ou atravessam a tela ficam fora do filtro geral. Eles podem voltar nas fitas assistidas na TV (entrega 6).
- **O menu de pausa fica acima do filtro**, para continuar legível com a intensidade no máximo.
- **Interface:** esticamento `canvas_items` com base 1280x720, para menus e legendas manterem o tamanho relativo em qualquer janela.
- **Interpolação de física ligada** (`physics/common/physics_interpolation`). O corpo do jogador anda a 60 ticks por segundo e o motor suaviza entre eles. A cabeça (câmera) é destacada do corpo (`top_level`, sem interpolação) e posicionada a cada quadro: posição do corpo interpolada, rotação direto do mouse. É o padrão de câmera em primeira pessoa da documentação do Godot. A tela do dono é de 120 Hz ou mais, então sem isso o movimento tremeria.
- Limite da medição: a sala continua sem textura e sem personagem. O ganho sobre a entrega 1 vem da resolução menor. O filtro aprovado custa cerca de 0,4 ms por quadro a 1080p (diferença entre 780 e 595 quadros por segundo), contra os 16,7 ms disponíveis a 60 quadros por segundo.

### Decisão da entrega 3: objetos soltos, poça e interação

Feito em 2026-11-12, na sala de teste, só com formas primitivas do Godot (sem Blender).

**Camadas de física** (nomeadas em `project.godot`). Funcionam como um par de máscaras de bits: a camada diz "o que eu sou", a máscara diz "o que eu enxergo".

| Quem | Camada | Enxerga |
|---|---|---|
| Cenário (paredes, piso, móveis) | 1 `world` | (estático) |
| Jogador (`CharacterBody3D`) | 2 `player` | 1 |
| Objetos soltos (lata, fita) | 3 `props` | 1 e 3 |
| Pés do jogador (entrega 4; substituíram a `KickZone`) | 4 `feet` | 3 |
| Raio de interação (`Interactor`) | nenhuma | 1 e 3 |
| Poça (`Area3D`) | nenhuma | 3 e 4 (até a entrega 3: 2 e 3) |

- **O jogador não colide com objetos soltos.** Na primeira versão colidia: o teste mostrou o jogador subindo 0,58 m em cima da lata espremida contra a parede, porque a cápsula escorrega por cima de objetos baixos. Agora ele passa por eles e os chuta.
- **Chute** (`PlayerController.KickLooseObjects`; na entrega 4 a zona deu lugar aos pés, ver abaixo): a `KickZone` é uma área na altura das pernas (cilindro de 0,3 m de raio, do piso até 0,5 m). Todo corpo rígido dentro dela recebe um impulso para longe do jogador, proporcional à velocidade com que jogador e objeto se aproximam (`KickStrength`, 1,8), mais um pequeno levantamento (`KickLift`) e um giro para a frente (`KickSpin`). Sem o giro, a lata deslizava em pé. A velocidade do jogador usada na conta é a que ele realmente andou: barrado por uma parede, ele não chuta um objeto parado. Um objeto que vem na direção do jogador parado é rebatido.
- **Por que só as pernas:** com a zona do tamanho do corpo inteiro, soltar a fita andando para a frente a arremessava 2,25 m, porque o tronco a alcançava ainda no ar (achado do `reviewer`). Com a zona nas pernas ela cai reta.
- **Objetos pequenos usam `continuous_cd`.** Lata (6,6 cm por 12,2 cm, 20 g) e fita (19 por 10,5 por 2,5 cm, 200 g).
- **Poça** (`game/vfx/puddle.tscn`, `Puddle.cs`, `puddle.gdshader`): um plano com shader. O script guarda até 8 ondulações (posição e progresso de 0 a 1) e as envia ao shader a cada quadro enquanto houver alguma viva. Nasce uma ondulação onde um corpo entra na água e outra a cada 0,5 m andado dentro dela. O shader desenha cada uma como um trem curto de anéis que se afasta e some, inclina a superfície (o reflexo da lâmpada treme) e clareia as cristas. A malha continua plana. O material é local a cada cópia da cena, para que duas poças não ondulem juntas. Para uma poça maior, escalar o nó.
- **Interação** (`game/interaction/`): `Interactor` é um `RayCast3D` de 2 m preso à câmera. O que ele atinge e implementa `IInteractable` pode ser usado com a ação `interact` (tecla E). É uma interface com um handler, como um `onClick` tipado. A dica na tela sobe por sinal (`PromptChanged`) e é ligada ao `Label` na própria cena do jogador, como uma prop de callback ligada pelo componente pai.
- **Pegar e soltar** (`Pickable`): ao ser pego, o corpo rígido é congelado, perde a camada de colisão e vira filho do `HoldPoint`, um nó sob a câmera. Ao ser solto, volta ao pai original e à física. Não há inventário: só um item na mão.
- **O item na mão não atravessa parede.** A cada quadro o `Interactor` varre a forma do item dos olhos até a mão, só contra a camada `world`, e o posiciona onde ele pararia, com 2 cm de folga. Perto da parede o item recua para junto da câmera, como um braço dobrando. Por isso soltar é sempre permitido: o item nunca está dentro da parede.
- **A dica fica acima do filtro VHS** (camada 5, entre o filtro e o menu de pausa), pelo mesmo motivo do menu: legibilidade com o filtro no máximo.
- **Escala 3D:** o limite real do motor é 0,1 (conferido: pedir 0,2 devolve 0,2; pedir 0,05 devolve 0,1). O piso no código passou de 0,25 para 0,1. Em 4K o 3D fica com 480 linhas.
- **Como foi testado sem o dono:** roteiros GDScript descartáveis rodados com `--headless --script`, que carregam a sala, simulam as teclas e imprimem posições. A física roda igual sem janela. Depois, uma execução com janela e `--write-movie` para conferir o shader e a aparência.
- Limites: a ondulação nasce na posição do corpo, não de cada pé (não há pés até a entrega 4). A lata tomba sempre para a frente e segue deitada. Um objeto pequeno pode afundar alguns milímetros no piso em repouso (tolerância de penetração padrão do Jolt, 2 cm). Quadros por segundo não foram medidos de novo nesta entrega.

### Decisão da entrega 4 (parte 1): corpo do Nondas

Feito em 2026-11-12 e 2026-11-13. Primeiro personagem do pipeline. A mão que se estica fica para a parte 2.

**Modelo** (`game/assets/models/nondas.glb`, gerado por `tools/blender/nondas_body.py`, repositório privado):

- Malha base, alvos de forma e esqueleto `game_engine` do MPFB (CC0). Homem de 19 anos (`age` 0,366), magro, pele parda. Escalado para os olhos ficarem a 1,6 m, a altura da câmera.
- Roupa pintada sobre a pele por região, pelo osso que mais move cada vértice: camiseta de manga curta, jeans, Kichute preto. Cada pano é estufado alguns milímetros na direção da normal.
- **Sapato:** os pés da malha são trocados pelo casco convexo de cada pé, subdividido e suavizado, preso aos ossos do pé e dos dedos. Suavizar a própria malha do pé não apagou os dedos, e puxar os vértices para o casco criou pontas.
- **Cabeça e pescoço** viram uma malha à parte (`Head`), desenhada no jogo só como sombra. A sombra no chão mostra o corpo inteiro.
- O dono escolheu a roupa entre três opções renderizadas no Blender, da câmera do jogo, numa página de comparação: jeans e tênis, bermuda e chinelo, jeans e Kichute.

**Animação** (gerada pelo script, sem quadros-chave à mão):

- Cinco ações de 1 s: `idle`, `walk_forward`, `walk_backward` (a de frente tocada ao contrário), `walk_left` e `walk_right`.
- Cada pé segue uma trajetória: no chão, desliza para trás em relação ao quadril, com o calcanhar subindo no fim; no ar, volta para a frente. O joelho sai de um cálculo de dois ossos (lei dos cossenos), dobrando para a frente. Quadril baixa e balança, braços pendem e balançam contra as pernas, palmas viradas para as coxas.
- Para a frente o corpo anda 1,05 m por ciclo (`StrideLength` em `PlayerBody.cs`); o jogo toca o ciclo a `velocidade / 1,05` vezes por segundo, e o pé não desliza. Para o lado o passo é curto, para os pés não se cruzarem, e o pé desliza cerca de 60%.
- No Godot, uma `AnimationTree` montada em código: um `BlendSpace2D` (como um joystick: direção de andar no espaço do jogador) com as cinco ações sincronizadas, seguido de um `TimeScale`. A direção é suavizada (10 por segundo), para a pose não pular ao trocar de tecla.

**Na cena do jogador:**

- O nó `Body` fica destacado do jogador (`top_level`, sem interpolação de física) e é posicionado a cada quadro com a rotação do mouse, como a cabeça. Girando a 60 ticks por segundo, as pernas ficariam atrasadas em relação à câmera ao girar olhando para baixo. Também é posicionado a cada tick de física, para pés e chute não usarem a posição antiga quando os quadros ficam mais lentos que a física (achado do `reviewer`).
- O modelo fica 32 cm atrás da câmera. Mais perto, olhando para baixo, a câmera via o buraco do pescoço e o peito cobria metade da tela.
- **Velocidade de andar: 3,0 → 2,0 m/s** (decisão do dono). A 3,0 m/s as pernas davam 5 passos por segundo.

**Pés:** cada pé tem uma `Area3D` do tamanho do sapato (11 x 28 x 9 cm), presa ao osso por um `BoneAttachment3D`, na camada 4 `feet`, enxergando a camada 3 `props`.

- **Chute pelos pés** (substitui a `KickZone` da entrega 3): com o cilindro em volta do jogador, a lata voava quando o pé visível ainda estava 15 a 40 cm atrás dela. Agora o objeto só é chutado quando um sapato encosta nele, na direção que sai do tornozelo. A força continua a da entrega 3 (velocidade real do jogador vezes `KickStrength`). Um raio do jogador até o objeto, contra a camada `world`, impede chutar através de parede: o corpo fica atrás da câmera, e o calcanhar alcança o outro lado de uma parede fina (achado do `reviewer`).
- **Poça pelos pés:** a poça sente áreas além de corpos. Uma coisa só toca a água a menos de 10 cm de altura (`TouchHeight`, medida em metros mesmo com a poça escalada): pé no ar e lata voando não fazem ondulação. Resultado: uma ondulação por pisada, nascendo no pé.

**Como foi testado:** roteiros `--headless --fixed-fps 60` (ondulações por pisada, chute por cada pé, lata entre os pés, fita solta andando, andar para os lados e para trás, lata atrás de parede fina) e uma gravação com janela de 240 quadros. Defeitos achados assim antes do dono jogar: a primeira chave de cada ação herdava a pose do `idle` (faixa de NLA tocando durante a gravação) e fazia o pé quicar e a poça ondular duas vezes; a perna se esticava 2 cm além do comprimento ao pisar.

**Limites:** quadros por segundo não medidos; sem textura (cores lisas); a lata centrada entre os pés não é chutada; o calcanhar às vezes chuta a lata depois de o pé passar por cima dela.

## 5. Qualidade

- Lógica de história coberta por testes xUnit (cada final, cada condição).
- Cada entrega tem um roteiro de teste manual de 2 minutos para o dono.
- Checkpoint e menu de pausa/opções existem desde a primeira versão jogável (lição 3 da pesquisa).
- Lista de softlocks conhecidos revisada a cada entrega (reclamação número 1 do gênero).

## 6. Riscos assumidos (registro franco)

| Risco | Origem | Mitigação |
|---|---|---|
| Personagens semi-realistas são o item mais caro do projeto | Escolha do dono | Baixa resolução, filtro e pouca luz; poucas animações; primeiro personagem validado cedo no MVP |
| Textura procedural pode parecer artificial em material brasileiro (chapisco, caquinho) | Escolha do dono | Validar uma parede e um piso na primeira entrega de cenário; se não convencer, reabrir a decisão |
| Licença da malha base | Resolvido em 2026-11-12 | Assets do MPFB são CC0; só pacotes CC0 são usados (`docs/credits.md`) |
| Scripts que usam a API do Blender teriam de ser GPL se publicados (FAQ do blender.org; questão jurídica aberta) | Descoberto na entrega 4 | Scripts num repositório privado; só os `.glb` vão para o público |
| Repositório público expõe todos os assets | Requisito | Arte 100% própria. Áudio de terceiros somente CC0, que permite redistribuir. Sons de sites "gratuitos" com licença própria (que proíbem republicar o arquivo) ficam de fora |
| Escopo no teto: 6 personagens, armadilha, final secreto, microfone | Escolhas do dono | Ordem do MVP deixa armadilha, final secreto e microfone por último |
