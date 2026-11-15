import {
  cancelEvento,
  createEvento,
  deleteEvento,
  listEventos,
  type SaveEvento,
  updateEvento,
} from "@casarme/api-client";
import { createServerFn } from "@tanstack/react-start";

import { accountClient } from "#/lib/account.server";

export const loadEventos = createServerFn({ method: "GET" })
  .validator((data: { casamentoId: string }) => data)
  .handler(async ({ data }) => {
    const client = await accountClient();
    if (!client) throw new Error("Entre na sua Conta para continuar.");
    const result = await listEventos({
      client,
      path: { casamentoId: data.casamentoId },
    });
    if (!result.response?.ok)
      throw new Error("Não foi possível carregar os Eventos.");
    return result.data!;
  });

export const mutateEvento = createServerFn({ method: "POST" })
  .validator(
    (data: {
      casamentoId: string;
      id?: string;
      action: "save" | "cancel" | "delete";
      values?: SaveEvento;
    }) => data,
  )
  .handler(async ({ data }) => {
    const client = await accountClient();
    if (!client) throw new Error("Entre na sua Conta para continuar.");
    const path = { casamentoId: data.casamentoId, id: data.id! };
    const result =
      data.action === "cancel"
        ? await cancelEvento({ client, path })
        : data.action === "delete"
          ? await deleteEvento({ client, path })
          : data.id
            ? await updateEvento({ client, path, body: data.values! })
            : await createEvento({
                client,
                path: { casamentoId: data.casamentoId },
                body: data.values!,
              });
    if (!result.response?.ok) {
      const problem = result.error as { errors?: Record<string, string[]> };
      return {
        errors: problem?.errors ?? {
          form: ["Não foi possível salvar o Evento."],
        },
      };
    }
    return { errors: null };
  });
