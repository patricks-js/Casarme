import { createApiClient } from "@casarme/api-client";
import { env } from "@casarme/env/server/web";
import { getRequestHeaders } from "@tanstack/react-start/server";

export async function accountClient() {
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
