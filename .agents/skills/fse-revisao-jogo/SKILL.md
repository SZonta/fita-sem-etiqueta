---
name: fse-revisao-jogo
description: >-
  Checklist de auditoria específico de Fita Sem Etiqueta (prevenção de softlocks, física/montagem 3D, invariantes Godot C# e cobertura de finais) complementar ao dev-entrega e ao subagente code-reviewer do dev-team. Usar ao revisar entregas antes de pedir o OK do dono ou ao fechar marcos.
---

# Auditoria Anti-Softlock e Arquitetura do Jogo (`fse-revisao-jogo`)

Esta skill complementa `dev-entrega` e os subagentes `code-reviewer` / `security-auditor` do plugin `dev-team` (não repete checagens genéricas de código limpo ou OWASP — adiciona exclusivamente as invariantes críticas do jogo).

- **Política de Modelos na Revisão:**
  - Entregas **P / M** → executar revisão em **Claude Sonnet** (`code-reviewer` herda o modelo da sessão via `inherit`).
  - Entregas **G** (ex.: `GameState`/`LevelLoader` na 9, IA do Freguês nas 12–13, Finais na 14) e fechamento dos **Marcos 1, 2 e 3** → executar auditoria em **Claude Opus**.

## Checklist Obrigatório (em ordem de criticidade)

Ao invocar `code-reviewer` (somente leitura) no fim de uma entrega, inclua os pontos abaixo no briefing do subagente e valide na saída:

1. **Caça a Softlocks (Prioridade #1):**
   - Algum item de missão pode cair em fresta **>= 3 cm** ou parar fora do alcance real da mão (65 cm em balcão / 50 cm no chão / 1,70 m de altura / mínimo **2 cm livres** sob tábua)?
   - Alguma ordem de ações do jogador apaga pista essencial (ex.: rebobinar antes de assistir) ou trava porta/interação sem saída?
   - Trocar de cena ou recarregar checkpoint com item na mão (`Pickable`), assistindo TV (`IViewer`) ou com menu de pausa aberto deixa estado órfão/travado?
   - Um checkpoint pode ser salvo num momento em que a derrota já é inevitável?
2. **Invariantes Godot 4 + C# (`docs/design/02-stack-architecture.md`):**
   - Composição em vez de herança; chamadas descem e sinais sobem; sem novos Autoloads fora dos 4 previstos (`GameState`, `Settings`, `SaveService`, `LevelLoader`).
   - Lógica narrativa em `game/story/` 100% em C# puro (zero `using Godot;`) e todas as combinações de flags/finais cobertas por testes automatizados xUnit (`dotnet test`).
   - Zero texto visível ao jogador escrito direto em `.cs` ou `.tscn` (tudo por chave de tradução em `game/localization/` com paridade `pt`/`en`/`es`).
   - Objetos soltos novos com `continuous_cd = true`, `collision_layer = 4` (`props`) e `collision_mask = 5`; chamadas de teleporte do jogador seguidas de `ResetPhysicsInterpolation()`.
3. **Convenções e Fronteiras de Repositório:**
   - Código, identificadores e nomes de arquivos de jogo em inglês; **documentos, commits e comentários de código novos/editados em português** *(sem exigir retradução de comentários legados em inglês fora do trecho tocado)*.
   - Nenhum script gerador do Blender (`tools/blender/*.py`) nem arquivo temporário de `scratch/` incluído no repositório público.
