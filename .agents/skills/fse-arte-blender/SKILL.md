---
name: fse-arte-blender
description: >-
  Direção de arte (interior de SP em 1996 sob filtro VHS 480p) e pipeline de geração 3D procedural no Blender 5.2.4 LTS e MPFB 2.0.18 para Fita Sem Etiqueta. Usar sempre que criar, alterar, renderizar ou importar modelos 3D, cenários, texturas procedurais, personagens ou referências visuais.
---

# Direção de Arte e Pipeline Blender (`fse-arte-blender`)

Modelo preferencial para esta skill: **Gemini** (geração espacial `bpy`, texturas procedurais e inspeção multimodal de renders).

## 1. Direção de Arte (Dois Córregos/SP, Junho de 1996)

- **Estética:** semi-realista pensado para **~480 linhas internas sob filtro VHS**. Proporções reais, geometria enxuta e marcas de desgaste grandes (>= 1 cm); detalhes minúsculos somem na resolução interna — não desperdice polígonos neles.
- **Brasilidade pelo cenário:** sobrado geminado, porta de aço de enrolar, cadeira de plástico, orelhão, bandeirinhas de festa junina, granilite, caquinho, muro chapiscado, barrado a óleo.
- **100% autoral e fictício:** proibido usar marcas, logotipos, capas de filmes ou pessoas reais. Nenhum modelo ou textura de terceiros entra no jogo (única exceção aprovada: malha base e rig CC0 do **MPFB 2.0.18**, registrados em `docs/credits.md`).
- **Referências visuais (`image-generator`):** servem apenas como guia para escrever o script Python; ficam em `tools/blender/reference/` (criar a subpasta sob demanda) e **nunca** entram como textura/asset do jogo.

## 2. Regra do Repositório Privado (`tools/blender/`)

- `tools/blender/` é um repositório Git **privado** independente (`SZonta/fita-sem-etiqueta-art`), ignorado pelo `.gitignore` público.
- **Nunca** copie scripts `.py` geradores de arte para fora de `tools/blender/` dentro da árvore pública rastreada. Rascunhos e renders temporários ficam exclusivamente em `<Artifact Directory>/scratch/` (fora do repositório).
- Ao concluir alterações em `tools/blender/`, faça commit (Conventional Commits em português) e push separadamente com `git -C tools/blender`.

## 3. Padrão Obrigatório dos Scripts Blender (`Blender 5.2.4 LTS`)

Executável local (sem interface, não exige liberação de janela):
```powershell
C:/Tools/Blender/5.2.4/blender-5.2.4-windows-x64/blender.exe --background --python tools/blender/<script>.py -- --out <destino> --preview <Artifact Directory>/scratch/preview
```

Regras de código do gerador (identificadores em inglês; comentários novos/editados em português, sem retraduzir comentários legados intocados):
1. Iniciar com `bpy.ops.wm.read_factory_settings(use_empty=True)`. Construir malhas com `from_pydata` / `bmesh` (evitar `bpy.ops` de modelagem). Sem rede, `pip` ou imports externos além de `bpy`, `bmesh`, `mathutils`, `numpy` e biblioteca padrão (`store.py` importa medidas da fita de `tape.py`).
2. **Determinismo absoluto:** usar sementes fixas (`numpy.random.default_rng(<constante>)` e `random.Random(<constante>)`), UV calculada por código e zero iteração sobre `set`, timestamps ou UUIDs. **Validar rodando duas vezes seguidas** e confirmando saída idêntica byte a byte.
3. **Materiais:** exclusivamente `Principled BSDF` com texturas geradas por código.
4. **Exportação (`export_yup=True`):**
   - Objetos soltos e personagens: `.glb` com `image.pack()` antes de exportar.
   - Cenários grandes: `GLTF_SEPARATE` (`.gltf` + `.bin` + PNGs nomeados `<cena>_<textura>.png`), exportando **antes** de montar luzes/câmeras de `--preview`.
5. **Colisões e prevenção de softlock:**
   - Criar caixas convexas nomeadas com sufixo `-convcolonly` por peça estrutural (cada tábua, lateral, fundo, tampo) e um único bloco contínuo por fileira estática de fitas/capas.
   - Deixar **>= 2 cm livres acima** de qualquer item pegável sob prateleira e **nenhuma fresta >= 3 cm** entre móveis/paredes.
   - Imprimir ao final `STATS triangles=... materials=... colliders=...`.

## 4. Personagens Humanos com MPFB 2.0.18

- Habilitar o módulo `bl_ext.user_default.mpfb` com `addon_utils.enable` **depois** de `read_factory_settings`.
- API validada: `HumanService.create_human`, `TargetService.bake_targets`, `HumanService.add_builtin_rig(body, "game_engine")`. Usar exclusivamente assets **CC0**.
- **Armadilhas conhecidas de animação no Blender:**
  - Criar faixas de NLA **somente depois** de gravar todas as ações (uma faixa ativa contamina a primeira chave da ação seguinte).
  - Ações que curvam a coluna (`stoop`) gravam `spine_01` a `spine_03`; as demais gravam explicitamente as costas retas (`stand_up_straight`) para não herdar pose anterior.

## 5. Validação Visual e Importação no Godot

1. Rodar primeiro com `--out` e `--preview` apontando para `<Artifact Directory>/scratch/`, renderizando vistas na câmera do jogo (**EEVEE, altura 1,6 m, FOV vertical 75°, 854×480**) e inspecionar os JPEGs/PNGs com `read_file`.
2. Conferir medidas, orçamento de triângulos, colisores, nomes de nós esperados pelo C# e determinismo nas 2 execuções.
3. Copiar/atualizar em `tools/blender/`, gerar o asset final em `game/assets/models/` e importar sem janela:
   ```powershell
   C:/Tools/Godot/4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe --headless --import --path game
   ```
4. **Ajustes pós-importação no Godot:**
   - Texturas PNG **novas** de cenário nascem sem mipmaps: ajustar manualmente no `.import` `compress/mode=2`, `mipmaps/generate=true`, `detect_3d/compress_to=0` e reimportar (evita cintilação a 480 linhas).
   - O importador glTF não liga cor de vértice: onde houver desgaste por vértice (ex.: piso), sobrepor material `.tres` com `vertex_color_use_as_albedo = true`.
   - Para escolhas subjetivas de aparência (cores, shaders, UI), apresentar 3 a 5 variações lado a lado sobre cena colorida antes de alterar o jogo definitivo.
