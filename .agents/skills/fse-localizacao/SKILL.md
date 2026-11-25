---
name: fse-localizacao
description: >-
  Redação de textos originais em português (interior paulista, 1996) e tradução para inglês e espanhol em game/localization/ para Fita Sem Etiqueta. Usar sempre que criar ou editar diálogos, legendas, placas, bilhetes, menus ou epílogos.
---

# Texto Narrativo e Localização PT/EN/ES (`fse-localizacao`)

Modelo preferencial para lotes de diálogo e tradução: **Claude Sonnet** (precisão estilística e concisão natural nos três idiomas). Referência narrativa obrigatória: `docs/design/01-story.md`.

## 1. Regras de Escrita (Português Original — SP, 1996)

- **Máximo de 2 linhas por legenda:** os personagens não têm voz dublada (usam "ruído de fala" + gesto corporal + legenda inferior). Se uma fala passar de duas linhas, enxugue ou divida em duas chaves.
- **Tom de época sem caricatura:** português natural do interior de São Paulo em junho de 1996, sem jargão caricato de "caipira" e sem gírias ou termos posteriores a 1996 (em caso de dúvida sobre expressão ou objeto da época, acionar `fse-pesquisa-epoca`).
- **Voz distinta por personagem:** respeitar o perfil de `docs/design/01-story.md` (ex.: o Freguês nunca ameaça abertamente — é educado, fala baixo e demonstra intimidade desconfortável).
- **Restrições de conteúdo:** epílogos e finais sem descrição gráfica explícita de violência; zero nomes de marcas, filmes, músicas ou pessoas reais.

## 2. Regras de Tradução (`en` e `es`)

- Traduzir **sentido e intenção**, nunca palavra por palavra, mantendo o limite de **2 linhas** também nas traduções.
- **Espanhol:** neutro latino-americano.
- **Preservação cultural:** nomes próprios, apelidos e "Dois Córregos" permanecem intactos; elementos brasileiros ("orelhão", "quermesse", "vira-lata caramelo") ganham equivalente curto e natural, sem notas explicativas na legenda.

## 3. Formato Técnico e Verificação

- **Proibido texto visível no código ou em `.tscn`:** todo texto exibido ao jogador vive em `game/localization/` (ex.: `ui.csv`), referenciado por chave em inglês `SNAKE_CASE` com prefixo de contexto (`N1_NEIDE_GREETING`, `MENU_LANGUAGE`, `SIGN_COMEDY`).
- **Paridade obrigatória:** toda chave adicionada ou editada deve ter preenchimento válido em `pt`, `en` e `es`.
- Após salvar `ui.csv`, regerar os binários `.translation` via comando headless (`fse-godot-testes`) e confirmar que não restou nenhuma chave vazia ou truncada em nenhum dos 3 idiomas.
