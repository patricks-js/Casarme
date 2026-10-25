import { createServerFn } from "@tanstack/react-start";
import { getRequestHeaders } from "@tanstack/react-start/server";

import { auth } from "#lib/auth/server.ts";
import { env } from "#lib/env/server.ts";

type Me = { id: string; email: string };

export const getMe = createServerFn({ method: "GET" }).handler(async () => {
  const headers = getRequestHeaders();
  const session = await auth.api.getSession({ headers });
  if (!session) {
    return null;
  }

  const { token } = await auth.api.getToken({ headers });

  const response = await fetch(new URL("/me", env.API_URL), {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    throw new Error(`GET /me failed with status ${response.status}`);
  }

  return (await response.json()) as Me;
});
