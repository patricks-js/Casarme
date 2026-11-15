import {
  type CasamentoResponse,
  createApiClient,
  createCasamento,
  getCurrentCasamento,
  type SaveCasamento,
  updateCasamento,
} from "@casarme/api-client";
import { env } from "@casarme/env/server/web";
import { createServerFn } from "@tanstack/react-start";
import { getRequestHeaders } from "@tanstack/react-start/server";

async function accountClient() {
  const cookie = getRequestHeaders().get("cookie") ?? "";
  const response = await fetch(`${env.AUTH_URL}/api/auth/token`, {
    headers: { cookie },
  });
  if (response.status === 401) return null;
  if (!response.ok) throw new Error("Não foi possível verificar sua Conta.");
  const { token } = (await response.json()) as { token?: string };
  return token
    ? createApiClient(env.API_URL, {
        headers: { Authorization: `Bearer ${token}` },
      })
    : null;
}

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
