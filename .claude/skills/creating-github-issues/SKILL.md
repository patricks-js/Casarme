---
name: creating-github-issues
description: Use when asked to create, open or file a new issue, ticket or task for the Casarme project on GitHub, or to add one to the Casarme Project board with a priority and size.
---

# Creating GitHub issues

## Overview

Every ticket is a GitHub issue in `patricks-js/Casarme`, added to the **Casarme Project** board (user project #7) with **Priority** and **Size** set. No other board field is filled in; leave Estimate, Status and the rest alone.

## Requirements

`gh` needs the `project` scope. If a `gh project` command fails with `missing required scopes`, ask the user to run `! gh auth refresh -s project`, then retry.

## Steps

1. **Write the body** in Portuguese, following the template below, to a file in the scratchpad. Ground it in `docs/PRD.md` (cite `FR-xx`/`BR-xx`) and in the current code; read the code before saying what exists or is missing.
2. **Choose Priority and Size** (tables below). Tell the user what you picked and why, in one line each.
3. **Create the issue and put it on the board:**

```sh
url=$(gh issue create -R patricks-js/Casarme -t "<título>" -F <body.md>)
item=$(gh project item-add 7 --owner patricks-js --url "$url" --format json --jq .id)

gh project item-edit --project-id PVT_kwHOBYIlc84BgXNE --id "$item" \
  --field-id PVTSSF_lAHOBYIlc84BgXNEzhajpU0 --single-select-option-id <priority-option-id>
gh project item-edit --project-id PVT_kwHOBYIlc84BgXNE --id "$item" \
  --field-id PVTSSF_lAHOBYIlc84BgXNEzhajpU4 --single-select-option-id <size-option-id>
```

4. **Verify** and report the link plus the values that were set:

```sh
gh project item-list 7 --owner patricks-js --format json \
  --jq '.items[] | select(.content.url=="'"$url"'") | "#\(.content.number) \(.priority) \(.size)"'
```

## Field IDs

| Priority | Option ID | Use for |
|---|---|---|
| P0 | `79628723` | Blocks the main MVP flow (wedding → events → guests → RSVP → check-in) or is a bug in it |
| P1 | `0a877460` | MVP scope that doesn't block the main flow |
| P2 | `da944a9c` | Nice to have, polish, outside the MVP |

| Size | Option ID | Rough scope |
|---|---|---|
| XS | `6c6483d2` | One-line fix or config change |
| S | `f784b110` | Small change in one app |
| M | `7515a9f1` | A feature slice: API changes + one screen, or similar |
| L | `817d0097` | Full feature across API, DB and web |
| XL | `db339eb2` | Several features; consider splitting the issue |

If an ID is rejected, the board was changed. Refresh the IDs with:

```sh
gh project field-list 7 --owner patricks-js --format json \
  --jq '.fields[] | select(.name=="Priority" or .name=="Size") | {id, name, options}'
```

## Body template

```markdown
## Contexto
<Qual feature do PRD (§5.1) e qual fluxo (FW-xx). Por que isso importa.>

### Requisitos e regras relacionados
- **FR-xx:** ...
- **BR-xx:** ...

### Já implementado
- <O que já existe no código, com arquivos. Omitir se nada existir.>

## O que falta
### Backend (`apps/backend`)
- [ ] <tarefa concreta, citando arquivo/classe quando houver>
### Web (`apps/web`)
- [ ] ...

## Critérios de aceite
- <Comportamento observável e testável.>
```

Leave out sections that don't apply (for example, no Web section for an API-only bug).

## Common mistakes

- **Issue created but not on the board:** `gh issue create` alone doesn't add it to the project. Always run `item-add`.
- **Passing the issue number to `item-edit`:** it needs the board item ID (`PVTI_...`) returned by `item-add`, not `#n`.
- **Duplicates:** search first with `gh issue list -R patricks-js/Casarme --state all --search "<termo>"`.
- **Features out of scope:** check PRD §5.2 (Fora do Escopo) before writing tasks. Social login, for example, is out.
