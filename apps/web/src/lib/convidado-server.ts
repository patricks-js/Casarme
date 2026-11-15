import {
  createConvidado,
  createGrupo,
  deleteConvidado,
  deleteGrupo,
  listGrupos,
  type SaveConvidado,
  setGrupoRepresentative,
  updateConvidado,
  updateGrupo,
} from "@casarme/api-client";
import { createServerFn } from "@tanstack/react-start";

import { accountClient } from "./account.server";

export const loadGroups = createServerFn({ method: "GET" })
  .validator((data: { casamentoId: string }) => data)
  .handler(async ({ data }) => {
    const client = await accountClient();
    if (!client) throw new Error("Entre na sua Conta para continuar.");
    const result = await listGrupos({ client, path: data });
    if (!result.response?.ok)
      throw new Error("Não foi possível carregar os Grupos.");
    return result.data!;
  });

type Action =
  | { kind: "createGroup"; name: string }
  | { kind: "renameGroup"; id: string; name: string }
  | { kind: "deleteGroup"; id: string }
  | { kind: "representative"; id: string; guestId: string }
  | { kind: "createGuest"; values: SaveConvidado }
  | { kind: "updateGuest"; id: string; values: SaveConvidado }
  | { kind: "deleteGuest"; id: string };

export const changeGuests = createServerFn({ method: "POST" })
  .validator((data: { casamentoId: string; action: Action }) => data)
  .handler(async ({ data }) => {
    const client = await accountClient();
    if (!client) throw new Error("Entre na sua Conta para continuar.");
    const { action, casamentoId } = data;
    const options = { client, path: { casamentoId } };
    const result = await (() => {
      switch (action.kind) {
        case "createGroup":
          return createGrupo({
            ...options,
            body: { name: action.name || null },
          });
        case "renameGroup":
          return updateGrupo({
            client,
            path: { casamentoId, id: action.id },
            body: { name: action.name || null },
          });
        case "deleteGroup":
          return deleteGrupo({ client, path: { casamentoId, id: action.id } });
        case "representative":
          return setGrupoRepresentative({
            client,
            path: { casamentoId, id: action.id },
            body: { guestId: action.guestId },
          });
        case "createGuest":
          return createConvidado({ ...options, body: action.values });
        case "updateGuest":
          return updateConvidado({
            client,
            path: { casamentoId, id: action.id },
            body: action.values,
          });
        case "deleteGuest":
          return deleteConvidado({
            client,
            path: { casamentoId, id: action.id },
          });
      }
    })();
    if (!result.response?.ok) {
      const problem = result.error as { errors?: Record<string, string[]> };
      return {
        error:
          Object.values(problem?.errors ?? {})
            .flat()
            .join(" ") || "Não foi possível salvar a alteração.",
      };
    }
    return { error: null };
  });
