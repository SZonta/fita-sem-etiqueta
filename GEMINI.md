# GEMINI.md — Regras e Índice do Projeto

**Fita Sem Etiqueta**: terror 3D em 1ª pessoa (*Fears to Fathom*), interior de SP em junho de 1996.
Stack: **Godot 4.7.2 (.NET / C# `net10.0`)** + **Jolt Physics** + **Blender 5.2.4 LTS** + **MPFB 2.0.18 (CC0)**. Alvo: Windows (Steam).

> **Economia de tokens:** NÃO leia toda a documentação de uma vez. Carregue sob demanda apenas os itens do índice abaixo e as skills `fse-*` conforme a tarefa.

## 1. Regras Inegociáveis

- **Idiomas:**
  - **Português (PT-BR):** conversa, docs (`STATUS.md`, `docs/`, `.agents/skills/`), **commits** e **comentários de código novos/editados** *(sem retraduzir comentários legados intocados)*.
  - **Inglês:** código-fonte (identificadores, classes, métodos, sinais), chaves de tradução (`SNAKE_CASE`), nomes de arquivos/pastas do jogo e branches.
- **Git:** **Conventional Commits em português** (`feat:`, `fix:`, `refactor:`, `docs:`, `chore:`), atômicos. Identidade Git **somente** local (`SZonta`, zero `.gitconfig` global). Branch: `main`.
- **Ferramentas (Google AI Pro + Gratuitas):**
  - Priorizar recursos Google do plano **AI Pro** no Antigravity (modelos Gemini, `image-generator` para referências).
  - Demais ferramentas restritas a opções gratuitas consolidadas (Godot, Blender, MPFB, .NET SDK, Audacity) e assets **CC0 1.0** em `docs/credits.md`.
  - **Proibido instalar/baixar qualquer ferramenta ou pacote sem autorização explícita prévia** (informar: nome, fonte oficial, tamanho confirmado e motivo).
- **Janela do Godot:** nunca rodar sem `--headless` antes de o dono escrever `"pode rodar"` (`fse-godot-testes`).
- **Arte Privada (`tools/blender/`):** repo Git privado aninhado (`SZonta/fita-sem-etiqueta-art`), ignorado pelo público. Jamais expor `.py` dele no repo público.
- **Sessão (`1 entrega = 1 sessão`):** desenvolver (visível/jogável) → testar sem janela → revisar (`dev-entrega` + `fse-revisao-jogo`) → **aguardar OK do dono** → atualizar `STATUS.md` → commit + push → prompt da próxima sessão (estado, entrega e modelo). **Sem commit/push antes do OK.**

## 2. Fluxo (`dev-team`) e Distribuição de Modelos

Usar `dev-team` (`dev-workflow` P/M/G → `dev-planejamento` → `dev-implementacao` / `dev-depuracao` → `dev-entrega`) + skills `fse-*`. Em arquivos grandes (`tools/blender/*.py`, logs), delegar ao subagente `research` (`flash_lite` / `flash`).

Alterne o modelo no seletor do Antigravity conforme a etapa (`code-reviewer` e `security-auditor` herdam via `inherit`; em `/swarm`, fixe por worker):

- **Gemini (Pro / Carbon High + `flash` / `flash_lite`):** scripts Blender (`fse-arte-blender`), texturas procedurais, MPFB, `.gdshader`, `.tscn`, renders 854×480, `image-generator` e `research`. *(Prioriza cota Google AI Pro e visão multimodal.)*
- **Claude Sonnet:** sistemas C# (`dialogue/`, `story/`, `noise/`, `npc/`), testes xUnit (`tests/`), `dev-depuracao`, textos (`fse-localizacao`) e `code-reviewer` P/M. *(Precisão em C# e escrita concisa poupando Gemini e Opus.)*
- **Claude Opus:** arquitetura **Porte G** (`GameState`/`LevelLoader`, IA do Freguês, Finais) e **Auditoria Anti-Softlock / Marcos 1–3** (`code-reviewer` + `security-auditor` + `fse-revisao-jogo`). *(Raciocínio profundo contra softlocks.)*

## 3. Índice Modular Sob Demanda

| Arquivo | Quando ler |
|---|---|
| `STATUS.md` | Iniciar/encerrar entrega (estado atual, próxima entrega e pendências) |
| `docs/design/01-story.md` | História, missões das 3 noites, NPCs, pistas e finais |
| `docs/design/02-stack-architecture.md` | Sistemas C#, cenas, camadas de física, alcance/colisão e arquitetura |
| `docs/design/03-mvp.md` | Roteiro macro das 19 entregas e 3 marcos |
| `docs/design/04-decisions-log.md` | Histórico pontual de benchmarks e decisões passadas |
| `docs/credits.md` / `LICENSE*` | Auditoria/registro CC0 e licenciamento dual |
| `docs/research/*.md` | Pesquisas concluídas (`01-fears-to-fathom.md`, `06-home-video-1996.md`) |
