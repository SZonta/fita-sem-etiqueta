---
name: fse-godot-testes
description: >-
  Runbook operacional de build C#, importação headless, testes automatizados sem janela (GDScript sobre SceneTree), gravação de quadros e regras de execução do Godot 4.7.2 para Fita Sem Etiqueta. Usar ao compilar, testar física/interação/esqueleto, validar shaders ou rodar o jogo.
---

# Build, Execução e Testes no Godot 4.7.2 (`fse-godot-testes`)

## 1. Ferramentas e Comandos Sem Janela (Livres)

Estes comandos não abrem janela nem capturam o mouse do dono — podem rodar diretamente:

- **Compilar C# (`net10.0`):**
  ```powershell
  dotnet build game/FitaSemEtiqueta.sln
  ```
  *(Nota Antigravity: se o sandbox bloquear leitura de `NuGet.Config` em `%APPDATA%`, executar com `BypassSandbox: true`. Quando `tests/` existir, rodar também `dotnet test`.)*
- **Importar assets/traduções novas ou alteradas (`.csv`, `.glb`, `.gltf`, `.import`):**
  ```powershell
  C:/Tools/Godot/4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe --headless --import --path game
  ```
- **Testes automatizados de cena/física sem janela (`--headless`):**
  ```powershell
  C:/Tools/Godot/4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe --headless --fixed-fps 60 --path game --script <Artifact Directory>/scratch/test_run.gd
  ```
- **NUNCA usar** `--headless --build-solutions --quit`: no Godot 4.7.2 o processo compila mas não encerra sozinho.

## 2. Trava Obrigatória para Comandos COM Janela

Qualquer execução do Godot **sem `--headless`** (incluindo `--write-movie` e `--print-fps`) abre janela e **captura o mouse do dono**:

1. **Nunca abra a janela do jogo sem o dono escrever explicitamente `"pode rodar"` na conversa.**
2. Antes de pedir permissão: conclua todo o código, `dotnet build` e testes `--headless`, informe **quantas execuções** serão feitas e o **tempo estimado**, encerre o turno e aguarde.
3. Estimativa de tempo em `--write-movie`: **~0,18 s por quadro** na sala de teste e **~0,40 s por quadro** na locadora (ex.: 300 quadros na loja ≈ 2 min). Sempre definir timeout seguro. A liberação vale apenas para a bateria anunciada.
4. Lembrete: **shaders `.gdshader` só são compilados com a janela aberta** (erros de shader não aparecem em `dotnet build` nem em `--headless`).

## 3. Receitas e Armadilhas de Roteiros `--headless` (GDScript em `<Artifact Directory>/scratch/`)

Os scripts e quadros de verificação ficam exclusivamente em `<Artifact Directory>/scratch/` (fora do repositório):

- **Ciclo de vida do `SceneTree`:** o script estende `SceneTree` e deve retornar `false` ao final de `_process(delta)` (retornar `true` encerra imediatamente). Nós instanciados só ficam utilizáveis a partir do 1º quadro de `_process`, não em `_initialize`. O `_process` do `SceneTree` roda **antes** do `_process` dos nós no mesmo quadro.
- **Taxas de quadros de teste:**
  - `--fixed-fps 60`: 1 tick de física por quadro → execução 100% determinística.
  - `--fixed-fps 144` (quadros sem tick de física) e `--fixed-fps 30` ou `40` (múltiplos ticks): obrigatórios ao mexer em câmera, corpo ou mão para detectar tremores e dessincronias entre física e renderização.
- **Leitura de pose com IK (`PlayerBody`):** ler `GetBoneGlobalPose` somente dentro do sinal `skeleton_updated` do `Skeleton3D` (em `_process`, a pose ainda não passou por `TwoBoneIK3D`, `HandAim` e `FingerCurl`).
- **Ponte C# ↔ GDScript:** propriedades C# de tipos não-Variant (como `IReadOnlyList<Area3D>`) não ficam visíveis ao GDScript. Propriedades Variant C# leem-se via `node.get("Bend")`.
- **Simular olhar vertical sem mouse capturado:** em `--headless` o modo capturado não gira a câmera. Usar um nó auxiliar com `process_priority = 1` escrevendo `Head.rotation.x` em `_process` **e** no sinal `physics_frame` do `SceneTree` (com `Interactor` em `process_priority = 2`); giro horizontal define-se no `transform` do jogador antes de entrar na árvore.
- **Inspeção de `--write-movie`:** ao gravar sequências longas em `scratch/`, gere uma folha de contato (ex.: grade 3×3 em um único JPEG via Python do Blender) para inspecionar com `read_file` sem carregar dezenas de PNGs no contexto. Descartar o 1º quadro após saltos bruscos de ângulo feitos por script.
- **Shaders em `SubViewport`:** a textura chega em sRGB; ler sem `source_color`, aplicar ruído/efeitos em sRGB e converter com `pow(c, 2.2)` ao final para não esmagar tons escuros. Custo de GPU mede-se com `RenderingServer.viewport_set_measure_render_time(rid, true)` e `viewport_get_measured_render_time_gpu(rid)`.
