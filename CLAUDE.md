# CLAUDE.md (local do projeto)

**Fita Sem Etiqueta**: jogo de terror 3D em primeira pessoa, estilo "Fears to Fathom", ambientado no Brasil (1990–2000).

**Leia `CONTEXT.md` antes de qualquer trabalho.** Ele é a fonte de verdade: história, stack, arquitetura, etapa atual, próxima etapa e pendências. Este arquivo guarda apenas as regras de trabalho.

O `CLAUDE.md` global do dono continua valendo (idioma, simplicidade, alterações cirúrgicas, formato TDAH).

## Regras de trabalho

- Responder sempre em português do Brasil. Código, commits, nomes de arquivos, branches e comentários em inglês. Documentos de contexto (`CONTEXT.md`, `docs/`) em português.
- **Nunca instalar ou baixar ferramenta sem autorização explícita.** Antes de pedir, informar: o que é, de onde vem, tamanho aproximado e por que é necessária.
- Somente ferramentas gratuitas e de fonte confiável.
- Nada global: agentes, configurações e contexto ficam dentro deste repositório (`./CLAUDE.md`, `./.claude/agents/`). A identidade git também é local ao repositório.
- Subagentes usam o mesmo modelo e o mesmo nível de esforço da sessão que os chamou.
- A sessão principal orquestra, desenvolve e faz o commit. Os agentes de `./.claude/agents/` (`researcher`, `reviewer`, `artist`, `language`) são acionados por ela para tarefas específicas.
- Discordar com franqueza quando houver ideia melhor, explicando o motivo.
- Não presumir. Ambiguidade que impacta o projeto vira pergunta.
- Priorizar entregas visuais: toda entrega deve ter algo que o dono consiga ver ou jogar.

## Regras de sessão

**1 entrega = 1 sessão.**

1. Desenvolver.
2. Testar.
3. Esperar o OK do dono.
4. Atualizar `CONTEXT.md`.
5. Commit.
6. Push.
7. Enviar ao dono um prompt autossuficiente para a próxima sessão (aponta para `CONTEXT.md`, resume o estado atual e indica a próxima entrega).

Nunca fazer commit ou push antes do OK do dono.

## Etapas do projeto (em ordem, uma de cada vez)

1. Pesquisa sobre Fears to Fathom.
2. Planejamento, sem código de jogo: roteiro, clímax, finais, local, personagens, stack, arquitetura, duração, agentes e MVP. Última tarefa da etapa: definir o nome do jogo, renomear a pasta local, criar o repositório público no GitHub e fazer o primeiro commit.
3. Desenvolvimento, seguindo o MVP aprovado.

## Repositório

- GitHub: conta `SZonta`, repositório **público** `fita-sem-etiqueta` (`https://github.com/SZonta/fita-sem-etiqueta`).
- O nome do repositório e da pasta local é o nome do jogo em kebab-case: `fita-sem-etiqueta`.
- Durante o desenvolvimento, arquivos do Claude (`CLAUDE.md`, `.claude/`, `CONTEXT.md`) podem ser commitados.
- **Release final (obrigatório):** remover do repositório todos os arquivos relacionados ao Claude, mantendo somente o que pertence ao jogo.

## Qualidade de código

- Equilíbrio entre desempenho e legibilidade. Não sacrificar a clareza por performance.
- Padrões e arquitetura adotados ficam documentados em `CONTEXT.md`, explicados com analogias a C#/TypeScript/React.
