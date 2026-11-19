# Prompt 06: fachada da locadora (`facade.py`)

- **Modelo:** Neon, raciocínio Médio (Alto se a resposta falhar).
- **Anexos:** nenhum. Script novo; todas as medidas da loja estão no prompt (o `store.py` não vai).
- **Conversa:** nova (não é a da loja nem a da rebobinadeira).
- **Resultado:** salvar o `.py` na pasta `Downloads`.

Copie tudo o que está dentro do bloco abaixo.

````text
Contexto: jogo de terror em primeira pessoa, feito no Godot 4, ambientado no centro de uma cidade pequena do interior de São Paulo, numa noite fria de junho de 1996, época de festa junina. O jogo renderiza o 3D em cerca de 480 linhas, sob um filtro de câmera VHS. Estilo semi-realista: proporções reais, geometria moderada, textura de resolução modesta. Detalhe fino não aparece; não gaste polígono nele. Tudo é fictício: nenhuma marca, logotipo ou texto legível em lugar nenhum.

Tarefa: escreva um script Python completo para o Blender 5.2 LTS, chamado facade.py, que gera por código a fachada noturna de uma locadora de vídeo de 1996 vista da rua: o sobrado da loja, o corredor lateral com portão, os vizinhos, a calçada, metade da rua, o poste e as bandeirinhas de festa junina. Devolva só o arquivo facade.py completo, com código, nomes e comentários em inglês. Depois do código, no máximo 5 linhas com as suposições que você fez.

COMO O CENÁRIO É USADO NO JOGO
- À noite, a porta de aço da loja está aberta e a loja acesa por dentro. O interior da loja é OUTRO arquivo, já pronto, e aparece pelo vão da porta: este script não modela nada do lado de dentro.
- A vista principal é do outro lado da rua, olhando a fachada de frente. Mais tarde o jogador vai andar por esta calçada e entrar no corredor, então tem de ser uma fachada de verdade (volumes, recuos, espessuras, retornos), não um painel pintado.
- As luzes NÃO entram no arquivo: o Godot põe as luzes. O script só marca o ponto da lâmpada do poste com um Empty.

EXECUÇÃO
- Roda sem interface, a partir da raiz do projeto do jogo: blender --background --python facade.py -- --out <arquivo.gltf> [--preview <pasta>]
- Leia os argumentos depois de "--" com argparse. --out tem padrão "game/assets/models/facade/facade.gltf". --preview é opcional.
- Comece com bpy.ops.wm.read_factory_settings(use_empty=True). Sem complementos, sem pip, sem rede, sem subprocess, sem ler ou importar arquivo de fora. Só bpy, bmesh, mathutils, numpy (vem com o Blender) e a biblioteca padrão.
- Crie as malhas com bpy.data.meshes.new + from_pydata (ou bmesh); nada de bpy.ops para modelar.
- Grave só dentro da pasta do --out e da pasta do --preview (crie se faltar). Nada em pasta temporária.
- Exporte glTF com arquivos separados, ANTES de montar qualquer coisa do preview:
  bpy.ops.export_scene.gltf(filepath=<out>, export_format="GLTF_SEPARATE", export_yup=True)
  Resultado na pasta do --out: facade.gltf + facade.bin + um PNG por textura, chamado facade_<nome>.png. Crie cada imagem com bpy.data.images.new("facade_<nome>", ...), preencha com pixels.foreach_set e salve direto na pasta do --out com esse nome (ou use image.pack() para o exportador gravar). Nada de caminho absoluto dentro do .gltf.
- Determinístico: duas execuções seguidas geram arquivos idênticos byte a byte. Sementes fixas (numpy.random.default_rng(<constante>) por textura, random.Random(<constante>) para variações de geometria). Nada de data, hora, uuid, nem laço sobre set (a ordem de um set muda entre execuções).
- No fim, imprima duas linhas:
  STATS triangles=<triângulos das malhas visíveis, sem colisores> materials=<materiais usados> colliders=<objetos terminados em -convcolonly>
  REPORT_JSON <json numa linha só; ver RELATÓRIO>

EIXOS, ORIGEM E UNIDADES (Blender)
- Metros, escala 1, Z para cima.
- X ao longo da rua: cresce da esquerda para a direita de quem está na rua olhando a fachada.
- Y é a profundidade: Y = 0 é a face INTERNA da parede da frente da loja. A loja fica em Y positivo; a calçada e a rua ficam em Y negativo.
- Z = 0 é o piso acabado dentro da loja.
- Todos os objetos com origem em (0; 0; 0), rotação 0 e escala 1, vértices em coordenadas do mundo. Exceções: Gate (origem na dobradiça) e o Empty StreetLamp_Light.
- Com export_yup=True, um ponto (x; y; z) do Blender vira (x; z; -y) no Godot. Use essa conversão no relatório.

O QUE JÁ EXISTE NO ARQUIVO DO INTERIOR (não modelar de novo, mas casar com ele)
- Paredes de alvenaria da loja, de Z 0 a 2,9: frente de Y -0,2 a 0; lateral esquerda de X -0,2 a 0; lateral direita de X 6,0 a 6,2. A face externa dessas paredes tem a pintura de dentro: a fachada deste script é um REVESTIMENTO por fora delas.
- Vão da porta de aço na parede da frente: X 0,6 a 3,6, Z 0 a 2,6. A porta de aço, as guias e a caixa do rolo ficam do lado de dentro (Y ≥ 0). O vão fica LIVRE: não modele porta, grade nem vidro nele.
- Fenda de devolução de fitas atravessando a parede da frente: X 3,875 a 4,125, Z 0,925 a 0,975. Ela já tem, dentro da espessura da parede (Y -0,2 a 0), um forro de chapa de 2 mm nas 4 faces (furo livre X 3,877 a 4,123, Z 0,927 a 0,973) e uma portinhola de metal em Y -0,19.
- Piso da loja em Z 0, de Y 0 para dentro. Teto da loja em Z 2,9.

REGRA DE ENCAIXE
- Nenhuma face visível pode coincidir com outra face visível no mesmo plano (pisca no Godot). Omita as faces que ninguém vê: costas do revestimento, faces de baixo apoiadas no chão, faces encostadas em outra peça.
- Por isso, no vão da porta, o revestimento dos batentes fica 5 mm para dentro do vão, por cima das faces de alvenaria que já existem (medidas abaixo).

LOTES, DA ESQUERDA PARA A DIREITA (todos com a frente para -Y)
| Lote | X | Plano da fachada (Y) | Topo (Z) | O que é |
|---|---|---|---|---|
| L3 | -14,6 a -8,4 | -0,24 | 4,6 | Comércio térreo com platibanda. Duas portas de aço de enrolar fechadas, 2,5 × 2,6 cada. Branco encardido, barrado cinza-chumbo pintado até 0,9 |
| L2 | -8,4 a -4,4 | -0,26 | 4,3 | Casa térrea com platibanda. Porta de madeira 0,8 × 2,1 e janela 1,2 × 1,1 (peitoril a 1,0) com grade de ferro e veneziana fechada. Azul-claro desbotado, barrado de chapisco cinza até 0,9 |
| L1 | -4,4 a -0,2 | -0,25 | 6,1 | Sobrado geminado à loja. Térreo: porta de aço fechada 3,0 × 2,6. Em cima: 2 janelas 1,0 × 1,2 com veneziana fechada. Rosa-salmão desbotado, barrado de filetes de pedra até 1,0 |
| Loja | -0,2 a 6,23 | -0,23 | 6,55 | O sobrado da locadora (detalhado abaixo) |
| Corredor | 6,23 a 7,23 | portão em Y -0,23 a -0,19 | — | Corredor lateral descoberto, 1,0 m livre, com portão (detalhado abaixo) |
| R1 | 7,23 a 11,43 | -0,24 | 4,4 | Comércio térreo com platibanda. A parede de X 7,23 a 7,43 é a lateral dele e a parede direita do corredor. Porta de aço fechada 3,2 × 2,6. Verde-claro desbotado |
| R2 | 11,43 a 15,93 | -0,22 | 6,3 | Sobrado. Térreo: porta de aço fechada 3,0 × 2,6 e porta de madeira 0,8 × 2,1 da escada. Em cima: uma janela 1,6 × 1,2 com veneziana fechada. Bege-caramelo |
| R3 | 15,93 a 21,43 | -0,25 | 4,2 | Casa térrea com platibanda. Porta de madeira 0,8 × 2,1 e duas janelas 1,2 × 1,1 com grade e veneziana fechada. Reboco sem pintura, cinza manchado, barrado de chapisco |

Vizinhos (L1 a L3, R1 a R3):
- Simples e escuros: tudo fechado, sem interior, sem luz. Distribua portas e janelas no lote do jeito que parecer natural.
- Portas de aço dos vizinhos: plano com textura de réguas, 5 cm atrás do plano da fachada, com guias laterais finas e barra de baixo em geometria. Janelas: vão com 10 cm de fundo até a veneziana, peitoril saliente.
- Os sobrados (L1, R2) têm a mesma borda de laje entre pavimentos que a loja (Z 2,9 a 3,05).
- Todo topo de platibanda tem capa (pingadeira) saliente 4 cm.
- Empenas: onde um prédio for mais alto que o vizinho do lado, modele a parede lateral exposta acima do vizinho, com 3,0 m de profundidade a partir do plano da fachada, e feche o topo com a capa. Vale para a loja também (acima de L1, de Z 6,1 a 6,55).

SOBRADO DA LOJA (X -0,2 a 6,23)
| Peça | Medidas |
|---|---|
| Revestimento do térreo (reboco pintado) | Y -0,23 a -0,2; X -0,2 a 6,23; Z -0,03 a 2,9; com os vãos da porta (X 0,6 a 3,6, até Z 2,6) e da fenda abertos |
| Revestimento do vão da porta | chapas de reboco de 5 mm, de Y -0,23 a 0: batente esquerdo X 0,600 a 0,605 e batente direito X 3,595 a 3,600 (Z 0 a 2,6); teto do vão Z 2,595 a 2,600 (X 0,6 a 3,6) |
| Soleira de granito cinza | X 0,605 a 3,595; Y -0,27 a 0; Z -0,03 a 0 (topo rente ao piso da loja; frente 4 cm saliente, degrau de 3 cm para a calçada) |
| Fenda (lado de fora) | furo livre igual ao de dentro (X 3,877 a 4,123, Z 0,927 a 0,973); forro de chapa de 2 mm nas 4 faces do furo de Y -0,23 a -0,2; moldura de chapa de 25 mm de largura em volta (X 3,85 a 4,15, Z 0,90 a 1,00), Y -0,234 a -0,23 |
| Borda da laje (concreto pintado) | X -0,2 a 6,23; Y -0,27 a -0,23; Z 2,9 a 3,05 |
| Parede do pavimento de cima | maciça, Y -0,23 a 0; X -0,2 a 6,23; Z 2,9 a 6,5. Não há interior |
| Janelas de cima (2) | vãos de 1,2 × 1,1: X 0,9 a 2,1 e X 3,9 a 5,1; Z 4,15 a 5,25. Veneziana de madeira pintada (verde-escuro ou marrom), duas folhas fechadas, no plano Y -0,12, com as faces do vão revestidas. Peitoril saliente: 5 cm além do vão de cada lado, Y -0,27 a -0,12, Z 4,11 a 4,15 |
| Borda da laje de cobertura | X -0,2 a 6,23; Y -0,25 a -0,23; Z 5,75 a 5,9 |
| Platibanda | parte de cima da parede, Z 5,9 a 6,5, com dois frisos horizontais rasos em geometria (1,5 cm de fundo, 3 cm de altura) em Z 6,15 e 6,30 |
| Capa da platibanda | X -0,24 a 6,27; Y -0,27 a 0,04; Z 6,5 a 6,55 |
| Lateral direita (sobre o corredor) | face em X 6,23, de Y -0,23 a 2,8, de Z -0,03 a 6,55. Até Z 2,9 é revestimento de 3 cm (X 6,2 a 6,23) sobre a parede existente; acima de Z 2,9, parede maciça de X 6,0 a 6,23. Fechar o topo e o fim (Y 2,8) |
| Empena esquerda | acima de L1: X -0,2 a 0, Y -0,23 a 2,8, Z 6,1 a 6,55 |
| Condutor de água (PVC cinza, 10 cm) | no canto esquerdo, centro X -0,10, Y -0,33 (afastado da parede para passar à frente das bordas de laje, com abraçadeiras); sai da platibanda em Z 5,95 com um cotovelo e desce até Z 0,12, onde um cotovelo de 45° despeja na calçada |
| Caixa do relógio de luz | X 5,75 a 6,15; Z 1,30 a 1,90; Y -0,38 a -0,23. Chapa cinza com visor de vidro escuro, sem texto. Eletroduto cinza de 3 cm de diâmetro sobe do topo da caixa (centro X 5,95, Y -0,29, à frente da borda da laje) até Z 5,6, onde fica um suporte com isolador para o fio que vem do poste |
| Ganchos das bandeirinhas | dois ganchos pequenos na fachada, em (0,15; -0,24; 3,5) e (6,15; -0,24; 3,5) |

LETREIRO (objeto SignBoard)
- Caixa de chapa pintada presa na parede, acima da borda da laje: X 0,3 a 5,7; Z 3,10 a 3,90; Y -0,29 a -0,23.
- Moldura de 4 cm em volta, numa cor forte e desbotada (vermelho-escuro ou azul-escuro).
- Painel liso no meio, face no plano Y -0,29: X 0,34 a 5,66, Z 3,14 a 3,86. Creme claro, desbotado, com sujeira leve nas bordas. SEM texto: o nome da loja é escrito por cima pelo Godot.
- O painel tem UV de 0 a 1 vista de frente (u da esquerda para a direita, v de baixo para cima), num material próprio.

CORREDOR LATERAL (X 6,23 a 7,23)
| Peça | Medidas |
|---|---|
| Piso cimentado (objeto Corridor) | X 6,23 a 7,23; Y -0,23 a 2,8; topo Z -0,03 (rente à calçada) |
| Parede direita (lateral de R1, objeto Corridor) | X 7,23 a 7,43; Y -0,24 a 2,8; Z -0,03 a 4,4; topo com capa |
| CorridorEnd_Temp (parede provisória, objeto próprio) | X 6,23 a 7,23; Y 2,8 a 3,0; Z -0,03 a 4,4; reboco liso cinza |
| Gate (portão de ferro, folha única) | X 6,24 a 7,22; Y -0,23 a -0,19; Z -0,02 a 2,10 |

Portão (objeto Gate):
- Desenho típico de casa dos anos 70 a 90: quadro de tubo de 4 cm; chapa lisa embaixo até 0,9; em cima, barras verticais de 2 cm a cada 12 cm; uma travessa no topo; trinco e um cadeado pequeno do lado esquerdo. Pintado de verde-escuro, com ferrugem nas bordas e embaixo.
- Origem do objeto EXATAMENTE na dobradiça: (7,22; -0,21; 0), vértices relativos a ela. Dobradiças do lado direito, presas na parede de R1. Abre para dentro do corredor (+Y), girando em Z. No arquivo ele está fechado (rotação 0).
- Folga visual de no máximo 1 cm entre o portão e as paredes e entre o portão e o chão.

CALÇADA E RUA (toda a largura: X -14,6 a 21,43)
| Peça | Medidas |
|---|---|
| Calçada | Y -2,08 até o plano da fachada de cada lote (no corredor, até Y -0,23); topo Z -0,03; corpo até Z -0,2 |
| Guia (meio-fio) de concreto | Y -2,23 a -2,08; topo Z -0,03; face da rua de Z -0,03 a -0,18; chanfro de 2 cm na aresta de cima. Pintada de cal branca, gasta e suja |
| Sarjeta de concreto | Y -2,63 a -2,23; Z -0,18 junto à guia, subindo até -0,15 na junta com o asfalto |
| Asfalto | Y -9,83 a -2,63; Z -0,15 nas bordas e -0,07 no eixo (Y -6,23), abaulado |
| Sarjeta do outro lado | Y -10,23 a -9,83, espelho da de cá. A guia e a calçada do outro lado NÃO entram |
| Largura da rua | 8,0 m de guia a guia (Y -2,23 a -10,23) |

- A calçada muda de lote para lote, como era comum (cada dono fazia a sua), sempre no mesmo nível, sem degrau entre lotes: Loja e corredor: ladrilho hidráulico de 20 × 20 cm, desenho geométrico simples em vermelho-óxido e cinza, gasto, alguns quebrados e remendados com cimento. L1, R1 e L3: cimento desempenado com juntas a cada 1 m, em tons diferentes. L2 e R3: piso de caquinho (cacos de cerâmica vermelha assentados em cimento).
- Nenhuma boca de lobo, grelha, tampa ou buraco em lugar nenhum. Nenhuma faixa pintada no asfalto.
- Asfalto gasto: remendos mais escuros e mais claros, trincas largas, agregado aparente, terra e folhas acumuladas na sarjeta. Uma ou duas manchas onde o asfalto gastou e aparece o paralelepípedo antigo por baixo.
- Não ponha paredes invisíveis nos limites: o jogo cuida disso.

POSTE (objeto StreetLamp)
- Poste de concreto de seção duplo T, centro da base em X -1,2, Y -1,9, base em Z -0,03, topo em Z 9,0. Seção 0,30 (ao longo de X) × 0,20 na base, afinando até 0,16 × 0,12 no topo, com um sulco raso de 4 cm nas duas faces largas. Concreto cinza manchado.
- Cruzeta no topo, ao longo de Y, de Y -3,0 a -1,0, em Z 8,65 a 8,75, seção 9 × 11 cm, com 3 isoladores de porcelana em Y -2,8, -2,3 e -1,2.
- Armação secundária na face do poste voltada para a rua (Y -2,05), com 3 roldanas em Z 7,4, 7,15 e 6,9.
- Braço de iluminação: tubo galvanizado de 5 cm saindo do poste em Z 6,4, na face voltada para a rua, curvando para cima até a ponta em (X -1,2; Y -3,9; Z 7,0).
- Luminária pública da época, pendurada na ponta do braço: refletor aberto de alumínio em forma de cuia, 0,45 de diâmetro, 0,20 de altura, boca para baixo, com o bulbo visível dentro (vidro leitoso). O bulbo tem material próprio com Emission Color branco levemente azulado e Emission Strength 1,0.
- StreetLamp_Light: um Empty (sem malha) no centro do bulbo, em (-1,2; -3,9; 6,86), rotação 0.

FIOS (objeto PowerLines)
- Todos correm ao longo de X por toda a largura modelada (X -14,6 a 21,43), com a barriga de um fio de poste: suponha postes vizinhos (não modelados) em X -36,2 e X 33,8; entre dois postes, curva parabólica com 0,9 m de barriga no meio. Diâmetro de 2,5 cm (mais grosso que o real, para não sumir a 480 linhas), tubo de 4 lados, um segmento por metro, preto fosco.
- 3 fios no topo dos isoladores da cruzeta (Z 8,85); 3 fios nas roldanas da armação secundária; 1 cabo de telefone de 4 cm em Z 5,3, em Y -1,75.
- Ramal de entrada: um fio da roldana de baixo (Z 6,9) até o suporte do eletroduto da loja (X 5,95, Y -0,30, Z 5,6), com 0,3 m de barriga.

BANDEIRINHAS DE FESTA JUNINA (objetos JuneFlags e JuneFlags_String)
- Dois varais atravessando a rua em diagonal, cruzando-se em X. Cada varal é calculado como se fosse até um gancho do outro lado da rua (que ainda não existe), mas é cortado antes, terminando no ar em Y -10,0:
  - Varal A: do gancho (0,15; -0,24; 3,5) até o ponto virtual (6,6; -10,6; 3,8), barriga de 0,6 m.
  - Varal B: do gancho (6,15; -0,24; 3,5) até o ponto virtual (-0,6; -10,6; 4,1), barriga de 0,35 m.
  - Curva: P(t) = início + (fim - início)·t, com Z(t) diminuído de 4·barriga·t·(1 - t); t vai de 0 até o t em que Y = -10,0. Com esses números, no cruzamento o varal B fica uns 40 cm acima do A e as bandeirinhas não se tocam.
- Barbante: tubo de 4 lados com 1 cm de diâmetro, cor de barbante cru (objeto JuneFlags_String).
- Bandeirinha: retângulo de 20 cm de largura por 28 cm de altura, pendurado pela borda de cima no barbante, com um corte em V de 7 cm na borda de baixo (5 vértices, 3 triângulos). Uma a cada 25 cm ao longo do varal. Papel de seda colorido: vermelho, amarelo, azul, verde, laranja e rosa, em ordem aleatória com semente fixa. Cada uma com giro aleatório de até ±15° em torno do barbante e leve inclinação, como se o vento tivesse parado.
- Um material só, de dois lados (use_backface_culling = False), com a cor vinda de uma pequena textura de paleta (ex.: 64 × 16, um bloco por cor) e o UV de cada bandeirinha dentro do seu bloco.

COLISÃO
- O Godot 4 transforma todo objeto com sufixo "-convcolonly" num corpo estático de forma convexa e descarta a malha. Crie caixas simples (8 vértices, 6 faces) com esse sufixo e nome único em inglês (ex.: Facade_Left-convcolonly, Sidewalk_L2-convcolonly). Malhas visíveis não têm colisão própria.
- Uma caixa por peça: cada trecho de revestimento da loja (à esquerda da porta, sobre a porta, entre a porta e a fenda, abaixo da fenda, acima da fenda, à direita da fenda, lateral do corredor), cada batente, a soleira, cada trecho de parede de vizinho (respeitando o recuo das portas de aço), cada trecho de calçada, a guia, cada sarjeta, o asfalto, o poste, o condutor, a caixa do relógio, o piso do corredor, a parede direita do corredor, CorridorEnd_Temp e o portão.
- A sarjeta e o asfalto abaulado podem ter colisor de prisma convexo que siga o perfil (o topo do asfalto é convexo), em vez de caixa.
- Colisor do condutor e da caixa do relógio: do plano da parede até a frente da peça, de Z -0,03 para cima, sem vão atrás nem embaixo.
- Colisão só até Z 3,0 (ninguém alcança acima disso). Fios, bandeirinhas, braço e luminária, letreiro e janelas de cima não têm colisão.
- Portão: colisão SÓLIDA cobrindo todo o vão, de X 6,23 a 7,23, Y -0,23 a -0,19, Z -0,03 a 2,10, mesmo com a grade: uma fita VHS chutada (caixa de 19 × 10,5 × 2,5 cm) não pode passar entre as barras nem por baixo. Esse colisor (Gate-convcolonly) é FILHO do objeto Gate, em coordenadas locais, para girar junto quando o portão abrir.
- Nenhuma fresta de 3 cm ou mais entre colisores onde uma fita possa entrar e sumir: portão e chão, portão e paredes, calçada e fachada, guia e sarjeta, sarjeta e asfalto. Colisores podem se sobrepor; não podem deixar vão.
- Regra geral do projeto: 2 cm livres acima de qualquer item em prateleira (aqui não há prateleira).

MATERIAIS E TEXTURAS
- Só Principled BSDF. Nomes com prefixo "Facade_" (ex.: Facade_StorePlaster, Facade_SignPanel, Facade_GatePaint). Imagem de cor ligada direto no Base Color; se fizer imagem de rugosidade, use uma imagem não-cor ligada no Roughness. Valores de cor de superfície até uns 0,7 em sRGB (é noite e a luz vem do Godot).
- Toda textura gerada por código com numpy, semente fixa, no máximo 1024 × 1024. UV por projeção plana calculada no próprio script a partir das posições dos vértices (nada de smart_project ou unwrap), para o resultado ser sempre igual.
- Formas grandes: manchas, escorridos e remendos de 3 cm ou mais. Risco fino some a 480 linhas.
- Texturas sugeridas (pode juntar ou dividir, até umas 12 imagens):
  | Imagem | Tamanho | Uso |
  |---|---|---|
  | facade_store_plaster | 1024 × 1024 | Fachada da loja com mapeamento único (X -0,24 a 6,27 por Z -0,03 a 6,55). Reboco pintado de amarelo-palha claro, envelhecido; barrado verde-água escuro pintado a óleo até Z 1,0, com um friso escuro em cima; respingo de chuva e sujeira na base; escorridos de chuva descendo da capa da platibanda e saindo das pontas dos peitoris ("bigodes"); duas ou três trincas finas saindo dos cantos dos vãos; áreas descascadas mostrando a pintura antiga por baixo. A lateral do corredor e as empenas usam a mesma imagem, projetada em (Y, Z) na mesma escala |
  | facade_neighbours | 1024 × 1024 | Atlas de 3 × 2 regiões, uma por vizinho (L3, L2, L1, R1, R2, R3), cada fachada com mapeamento único na sua região, com as cores e barrados da tabela de lotes e o mesmo envelhecimento |
  | facade_steel_door | 512 × 512 | Porta de aço de enrolar fechada, réguas horizontais de 7,7 cm, galvanizado opaco com ferrugem e sujeira embaixo, repete |
  | facade_shutter | 256 × 256 | Veneziana de madeira pintada, palhetas horizontais, tinta descascando, repete |
  | facade_sidewalk_cement, facade_sidewalk_tile, facade_sidewalk_caquinho | 512 × 512 cada | Os três pisos de calçada, repetindo (cimento: 2 × 2 m por imagem; ladrilho e caquinho: 1 × 1 m) |
  | facade_concrete | 512 × 512 | Guia caiada, sarjeta, poste, borda da laje |
  | facade_asphalt | 1024 × 1024 | 8 × 8 m por imagem, repete |
  | facade_flags | 64 × 16 | Paleta das bandeirinhas |
- O resto pode ser cor sólida: portão, moldura e painel do letreiro, granito da soleira (cinza com pontilhado), PVC, chapa galvanizada (metálico, rugosidade média), porcelana, fios, barbante, bulbo.

OBJETOS (nomes exatos, em inglês; o código do jogo usa os marcados com *)
- Facade* (revestimento da loja, batentes, soleira, bordas de laje, pavimento de cima, platibanda, capa, empenas, lateral do corredor)
- Facade_Shutters (venezianas e peitoris da loja)
- Facade_Fixtures (moldura e forro da fenda, condutor, caixa do relógio, eletroduto, suporte, ganchos)
- SignBoard* (só o letreiro)
- Neighbours (todos os vizinhos)
- Corridor (piso e parede direita do corredor)
- CorridorEnd_Temp* (só a parede provisória, com o seu colisor CorridorEnd_Temp-convcolonly; os dois serão apagados depois)
- Gate* (o portão; colisor filho)
- Sidewalk (calçada)
- Street (guia, sarjetas, asfalto)
- StreetLamp (poste, cruzeta, isoladores, armação, braço, luminária, bulbo)
- StreetLamp_Light* (Empty)
- PowerLines (fios e ramal)
- JuneFlags e JuneFlags_String

ORÇAMENTO
- Uns 15.000 triângulos visíveis no total, no máximo. Se sobrar, não gaste.
- Texturas de até 1024 × 1024.

RELATÓRIO (linha REPORT_JSON, coordenadas JÁ convertidas para o Godot: (x; y; z) do Blender → [x, z, -y], arredondadas a 4 casas)
{"sign": {"center": <centro da face do painel>, "normal": <normal da face do painel>, "panel_size": [largura, altura]},
 "street_lamp": {"position": <posição do StreetLamp_Light>},
 "gate": {"hinge": <origem do Gate>, "size": [largura, altura], "opens_towards": <direção de abertura>},
 "camera_main": {"position": <posição da câmera a_main_view>, "target": <alvo>}}

PREVIEW (só se --preview for passado; nada disto entra no glTF, que já foi exportado antes)
- EEVEE (engine "BLENDER_EEVEE"), 32 amostras, view_transform "Standard", JPEG qualidade 90. Esconda no render os objetos -convcolonly.
- Câmera com sensor_fit = "VERTICAL", clip_start 0,05, apontada com to_track_quat("-Z", "Y").
- Iluminação só da prévia: mundo azul-escuro quase preto (cerca de 0,01; 0,012; 0,02); uma luz spot no StreetLamp_Light apontada para baixo, branca levemente azulada, iluminando a calçada e a fachada sem estourar; uma luz de área de 3,0 × 2,6 m em Y 1,0 (dentro da loja), voltada para -Y, branca fria, para simular a loja acesa vista pelo vão; uma luz sun bem fraca e azulada, de lua.
- Quatro imagens na pasta do --preview:
  1. a_main_view.jpg: 854 × 480, angle_y 75°, câmera em (3,2; -11,7; 1,5) olhando para (3,0; -0,2; 2,2). É a vista principal, do outro lado da rua.
  2. b_main_view_narrow.jpg: 640 × 480, angle_y 36°, mesma posição e mesmo alvo.
  3. c_door.jpg: 854 × 480, angle_y 75°, câmera em (4,4; -1,8; 1,6) olhando para (2,1; -0,2; 1,3). De pé na calçada, perto da porta e da fenda.
  4. d_gate.jpg: 854 × 480, angle_y 75°, câmera em (6,73; -2,0; 1,6) olhando para (6,73; 2,8; 1,0). De frente para o portão do corredor.
- Na a_main_view, a ponta solta dos varais (Y -10,0) tem de ficar FORA do quadro, acima da borda de cima; e a calçada, a rua e os vizinhos têm de preencher o quadro de lado a lado.
````
