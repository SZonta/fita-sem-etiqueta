# Prompt 06: rebobinadeira (`rewinder.py`)

- **Modelo:** Neon, raciocínio Médio (Alto se a resposta falhar).
- **Anexos:** nenhum. Script novo; as medidas da fita já estão no prompt.
- **Conversa:** nova (não é a da loja).
- **Resultado:** salvar o `.py` na pasta `Downloads`.

Copie tudo o que está dentro do bloco abaixo.

````text
Contexto: jogo de terror em primeira pessoa, feito no Godot 4, numa locadora de vídeo do interior de São Paulo em 1996. O jogo renderiza o 3D em cerca de 480 linhas, sob um filtro de câmera VHS. Estilo semi-realista: proporções reais, geometria moderada, textura de resolução modesta. Detalhe fino não aparece; não gaste polígono nele.

Tarefa: escreva um script Python completo para o Blender 5.2 LTS, chamado rewinder.py, que modela uma rebobinadeira de fitas VHS de balcão de locadora dos anos 90 (caixa de plástico escuro com tampa articulada) e exporta um .glb. Devolva só o arquivo rewinder.py completo, com código, nomes e comentários em inglês.

COMO O OBJETO É USADO NO JOGO
O atendente põe uma fita VHS deitada dentro da rebobinadeira, a tampa fecha, ela rebobina alguns segundos, a tampa abre sozinha e ele tira a fita com a mão. A tampa, o botão e a luz são animados pelo código do Godot. O script só entrega as peças separadas, nomeadas e com a origem certa.

EXECUÇÃO
- Roda sem interface: blender --background --python rewinder.py -- --out <arquivo.glb> [--preview <pasta>]
- Leia os argumentos depois de "--" com argparse. --out tem padrão "game/assets/models/rewinder.glb". --preview é opcional.
- Comece com bpy.ops.wm.read_factory_settings(use_empty=True). Sem complementos, sem pip, sem rede, sem subprocess, sem importar ou baixar nada de fora. Só bpy, bmesh, mathutils, numpy (vem com o Blender) e a biblioteca padrão.
- Grave só no caminho de --out e dentro da pasta de --preview. Crie a pasta se faltar.
- Determinístico: semente fixa (numpy.random.default_rng(<constante>)); duas execuções seguidas geram arquivos idênticos byte a byte. Nada de data, hora ou caminho absoluto dentro do arquivo.

EIXOS, ORIGEM E UNIDADES (Blender)
- Metros, escala 1. Z para cima.
- Origem do arquivo: centro da base, no plano do chão do aparelho (z = 0 é onde ele encosta no balcão).
- Frente (lado do atendente) voltada para -Y. Comprimento ao longo de X.
- Todos os objetos com rotação 0 e escala 1 no arquivo exportado (aplique transformações de malha; mantenha só a localização indicada abaixo).
- Exporte com bpy.ops.export_scene.gltf(filepath=..., export_format="GLB", export_yup=True, export_apply=True). Exporte ANTES de montar qualquer coisa do preview.

A FITA QUE ENTRA NELA
Caixa preta de fita VHS: 19,0 × 10,5 × 2,5 cm. Na rebobinadeira ela fica DEITADA: face grande para cima, comprimento de 19 cm ao longo de X, 10,5 cm ao longo de Y, 2,5 cm de espessura em Z, com a lombada comprida voltada para a frente (-Y).

MEDIDAS (todas em metros, coordenadas do Blender)
- Externo total: 0,24 (X) × 0,15 (Y) × 0,070 (Z). X de -0,12 a 0,12; Y de -0,075 a 0,075.
- Corpo da base: altura 0,055 (borda de cima da cavidade em z = 0,055).
- Faixa de comandos na frente: Y de -0,075 a -0,053, largura total, sobe até z = 0,070 (fica rente ao topo da tampa fechada). Ela é parte da Base.
- Cavidade aberta em cima, de paredes retas: X de -0,098 a 0,098 (19,6 cm), Y de -0,049 a 0,062 (11,1 cm), fundo plano em z = 0,020, aberta até z = 0,055. Isso dá 3 mm de folga em volta da fita. A fita deitada vai de z = 0,020 a 0,045, ou seja, 1 cm abaixo da borda.
- Paredes resultantes: laterais 2,2 cm, frente 2,6 cm (inclui a faixa de comandos), fundo 1,3 cm. O interior da cavidade precisa ter faces de verdade (paredes e fundo), sem buracos para o vazio.
- Tampa: X de -0,12 a 0,12, Y de -0,052 a 0,075 (1 mm de fresta para a faixa de comandos), Z de 0,055 a 0,070 (1,5 cm de espessura). Fechada, cobre toda a cavidade e fica rente à faixa de comandos. Dobradiça interna, invisível: só a fresta aparece.
- Eixo da dobradiça: linha paralela a X na borda de trás de baixo da tampa, em y = 0,075, z = 0,055.
- Chanfro de 2 a 3 mm nas arestas externas da base e da tampa (geometria, 1 ou 2 segmentos). Normais: arestas acima de 30° ficam duras, o resto suave.

PEÇAS (objetos separados, sem parentesco entre eles, nomes exatos)
1. Base: corpo com a cavidade e a faixa de comandos. Origem em (0, 0, 0).
2. Lid: a tampa. Origem EXATAMENTE no eixo da dobradiça, (0, 0,075, 0,055), com os vértices relativos a esse ponto, para o Godot girá-la em torno do X local. No arquivo exportado ela está fechada (rotação 0). Abrir = girar em X com ângulo NEGATIVO (a borda da frente sobe). Constante no topo do script: LID_OPEN_DEG = 75. A face de baixo da tampa é lisa; uma leve depressão retangular de 1 mm na face de cima, rente às bordas, é bem-vinda.
3. Button: botão de ejetar, retangular, cerca de 3,0 × 1,2 cm, 4 mm acima do topo da faixa de comandos, no lado direito (centro em x = 0,075, y = -0,064). Origem no centro da face de baixo do botão (z = 0,070), para o Godot afundá-lo em -Z. No máximo um triângulo de ejetar em leve relevo; nenhuma letra.
4. Led: luzinha redonda de 3 mm de raio, 8 lados, 2 mm acima do topo da faixa de comandos, no lado esquerdo (centro em x = -0,085, y = -0,064). Origem no seu centro.
5. TapeSpot: um Empty (sem malha) no centro da fita deitada: (0, 0,0065, 0,0325). Rotação 0: X local ao longo dos 19 cm, Z local para cima.

COLISÃO
- Godot 4 transforma objeto com sufixo "-convcolonly" num corpo estático de forma convexa e descarta a malha. Crie UMA caixa só: RewinderFloor-convcolonly, de X -0,12 a 0,12, Y -0,075 a 0,075, Z 0 a 0,020 (do chão do aparelho até o fundo da cavidade). Malha de caixa simples, 8 vértices, 6 faces, origem em (0, 0, 0).
- Paredes da cavidade, tampa, botão e luz NÃO têm colisão (a mão precisa tirar a fita sem esbarrar). Nenhuma outra peça de colisão.
- Nenhuma fresta de 3 cm ou mais entre peças. (Regra geral do projeto: 2 cm livres acima de item em prateleira; aqui não há prateleira.)

MATERIAIS (só Principled BSDF, nomes exatos, 4 no total)
- RewinderPlastic: Base (exterior) e Lid. Plástico cinza-carvão fosco, cor sRGB perto de (60, 60, 63), rugosidade em torno de 0,6. Precisa contrastar com a fita, que é quase preta.
- RewinderCavity: paredes internas e fundo da cavidade e face de baixo da tampa. Um pouco mais escuro e mais fosco que o RewinderPlastic.
- RewinderButton: plástico vermelho apagado, sRGB perto de (120, 25, 22), rugosidade 0,5.
- RewinderLed: vermelho escuro com aparência de lente, Base Color sRGB perto de (90, 12, 10), rugosidade 0,3, Emission Color vermelho vivo e Emission Strength 0 (o código acende). Material próprio, usado só pelo Led.

TEXTURA (gerada por código)
- Uma imagem de 256 × 256 para a cor do RewinderPlastic (e, se quiser, outra de 256 × 256 para a rugosidade), gerada com numpy e semente fixa. Crie com bpy.data.images.new, preencha com pixels.foreach_set e chame image.pack() antes de exportar, para ir embutida no .glb. Não grave PNG solto.
- Marcas de uso em formas grandes (manchas de 1 cm ou mais; risco fino some a 480 linhas): desgaste mais claro e brilhante na borda da frente da tampa, no centro (onde o dedo levanta), e nas quinas; poeira acinzentada e fosca perto da fresta e nas bordas de baixo; algumas manchas suaves de mão no topo. Discreto: o objeto continua sendo plástico escuro.
- UV por projeção plana calculada no próprio script a partir das posições dos vértices (não use smart_project nem operadores de unwrap), para o resultado ser sempre igual.
- Nada de marca, logotipo, adesivo ou texto legível.

ORÇAMENTO
- No máximo uns 1.500 triângulos visíveis (sem contar a colisão).
- No fim, imprima uma linha: STATS triangles=<triângulos das malhas visíveis> materials=<materiais usados> colliders=<objetos terminados em -convcolonly>

PREVIEW (só se --preview for passado; nada disto entra no .glb)
Depois de exportar, monte e renderize duas imagens JPEG 854 × 480 em EEVEE (engine "BLENDER_EEVEE"), 32 amostras:
- Câmera com sensor_fit = "VERTICAL" e angle_y de 75°, clip_start 0,05, na posição do olho do atendente: (0, -0,35, 0,60), olhando para (0, 0, 0,03).
- Um plano de 1 × 1 m em z = 0 como tampo de balcão, cinza-claro fosco; mundo quase preto; uma luz de área de 1,2 × 0,1 m a 1,5 m de altura, branca levemente fria, sem estourar.
- Esconda no render o objeto -convcolonly.
- rewinder_closed.jpg: tampa fechada, cavidade vazia.
- rewinder_open.jpg: Lid girada em X por -LID_OPEN_DEG, e uma caixa preta de 0,19 × 0,105 × 0,025 m centrada no TapeSpot, representando a fita.
````
