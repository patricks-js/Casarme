import {
  type CasamentoResponse,
  createCasamento,
  getCurrentCasamento,
  type SaveCasamento,
  updateCasamento,
} from "@casarme/api-client";
import { createServerFn } from "@tanstack/react-start";

import { accountClient } from "#/lib/account.server";

export const loadCasamento = createServerFn({ method: "GET" }).handler(
  async () => {
    const client = await accountClient();
    if (!client)
      return { signedIn: false, casamento: null as CasamentoResponse | null };
    const result = await getCurrentCasamento({ client });
    if (result.response?.status === 404)
      return { signedIn: true, casamento: null };
    if (!result.response?.ok)
      throw new Error("Não foi possível carregar o Casamento.");
    return { signedIn: true, casamento: result.data! };
  },
);

export const saveCasamento = createServerFn({ method: "POST" })
  .validator((data: { id?: string; values: SaveCasamento }) => data)
  .handler(async ({ data }) => {
    const client = await accountClient();
    if (!client) throw new Error("Entre na sua Conta para continuar.");
    const result = data.id
      ? await updateCasamento({
          client,
          path: { id: data.id },
          body: data.values,
        })
      : await createCasamento({ client, body: data.values });
    if (!result.response?.ok) {
      const problem = result.error as { errors?: Record<string, string[]> };
      return {
        errors: problem?.errors ?? {
          form: ["Não foi possível salvar. Tente novamente."],
        },
      };
    }
    return { errors: null };
  });
