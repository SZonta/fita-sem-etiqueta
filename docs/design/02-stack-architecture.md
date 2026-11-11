# Design 02: Stack e arquitetura

Etapa 2 do projeto. Status: **aprovado pelo dono em 2026-11-11**, com um ajuste já incorporado (áudio pode usar bases existentes).

## 1. Stack decidida

| Item | Decisão | Observação |
|---|---|---|
| Motor | Godot 4, edição .NET | Versão estável em 2026-11-11: 4.7.2 (fonte: godotengine.org) |
| Linguagem | C# | Exige .NET SDK 8 ou superior (fonte: documentação oficial do Godot) |
| Estilo visual | Semi-realista sob filtro VHS | Proporções reais, geometria moderada, textura de resolução modesta |
| Cenário e objetos | 100% próprios, gerados por script no Blender | Texturas procedurais, sem fotos |
| Personagens humanos | Malha base de gerador gratuito (MPFB/MakeHuman) + rosto, roupa, textura e animação próprios | **Licença do gerador ainda não confirmada.** Confirmar antes de usar |
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

Nada foi instalado. Cada item exige autorização explícita do dono no momento da instalação, com tamanho confirmado na fonte.

| Ferramenta | Origem oficial | Para quê | Tamanho |
|---|---|---|---|
| Godot 4.7.x, edição .NET | godotengine.org | Motor do jogo | A confirmar |
| .NET SDK (LTS atual) | dotnet.microsoft.com | Compilar o C# | A confirmar |
| Extensão C# para VS Code | Marketplace do VS Code (Microsoft) | Autocompletar e depurar | A confirmar |
| Blender (LTS atual) | blender.org | Modelos, rig, animação. Já inclui o Python usado pelos scripts | A confirmar |
| MPFB (complemento do Blender) | makehumancommunity.org | Malha base dos humanos | A confirmar, junto com a licença |
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
tools/blender/        scripts que geram cenário e objetos
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
- Renderizador e motor de física: decididos e registrados na primeira entrega técnica, depois de medir na máquina do dono.

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
| Licença da malha base não confirmada | Pendência | Confirmar na fonte oficial antes de qualquer uso; plano B: personagens próprios mais simples |
| Repositório público expõe todos os assets | Requisito | Arte 100% própria. Áudio de terceiros somente CC0, que permite redistribuir. Sons de sites "gratuitos" com licença própria (que proíbem republicar o arquivo) ficam de fora |
| Escopo no teto: 6 personagens, armadilha, final secreto, microfone | Escolhas do dono | Ordem do MVP deixa armadilha, final secreto e microfone por último |
