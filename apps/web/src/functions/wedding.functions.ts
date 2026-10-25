import { notFound } from "@tanstack/react-router";
import { createServerFn } from "@tanstack/react-start";
import { getRequestHeaders } from "@tanstack/react-start/server";
import { z } from "zod";

import { auth } from "#lib/auth/server.ts";
import { env } from "#lib/env/server.ts";
import {
  slugSchema,
  type Wedding,
  type WeddingInput,
  weddingInputSchema,
} from "#lib/wedding.ts";

/** Field the API rejected, matching the form field names. */
export type WeddingField = keyof WeddingInput;

export type WeddingMutationResult =
  | { ok: true; wedding: Wedding | null }
  | {
      ok: false;
      error: "validation" | "slug-taken" | "already-has-wedding" | "unknown";
      field?: WeddingField;
    };

async function apiFetch(path: string, init: RequestInit = {}) {
  const headers = getRequestHeaders();
  const { token } = await auth.api.getToken({ headers });

  return fetch(new URL(path, env.API_URL), {
    ...init,
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
      ...init.headers,
    },
  });
}

async function toMutationError(
  response: Response,
): Promise<WeddingMutationResult> {
  const body = (await response.json().catch(() => ({}))) as {
    error?: string;
    field?: string;
  };

  if (response.status === 400) {
    // the API reports PascalCase property names, e.g. "PrimaryDate"
    const field = body.field
      ? ((body.field.charAt(0).toLowerCase() +
          body.field.slice(1)) as WeddingField)
      : undefined;
    return { ok: false, error: "validation", field };
  }

  if (response.status === 409) {
    // the only 409 not caused by the slug is the one-wedding-per-user rule
    return body.error?.includes("already has a wedding")
      ? { ok: false, error: "already-has-wedding" }
      : { ok: false, error: "slug-taken", field: "slug" };
  }

  console.error(`Wedding API responded ${response.status}`, body);
  return { ok: false, error: "unknown" };
}

/** The signed-in user's wedding (one per user in the MVP), or null. */
export const getMyWedding = createServerFn({ method: "GET" }).handler(
  async () => {
    const response = await apiFetch("/weddings");
    if (!response.ok) {
      throw new Error(`GET /weddings failed with status ${response.status}`);
    }

    const weddings = (await response.json()) as Wedding[];
    return weddings[0] ?? null;
  },
);

export const getWedding = createServerFn({ method: "GET" })
  .validator(z.object({ id: z.uuid() }))
  .handler(async ({ data }) => {
    const response = await apiFetch(`/weddings/${data.id}`);
    if (response.status === 404) {
      throw notFound();
    }
    if (!response.ok) {
      throw new Error(
        `GET /weddings/{id} failed with status ${response.status}`,
      );
    }

    return (await response.json()) as Wedding;
  });

export const createWedding = createServerFn({ method: "POST" })
  .validator(weddingInputSchema)
  .handler(async ({ data }): Promise<WeddingMutationResult> => {
    const response = await apiFetch("/weddings", {
      method: "POST",
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      return toMutationError(response);
    }

    return { ok: true, wedding: (await response.json()) as Wedding };
  });

export const updateWedding = createServerFn({ method: "POST" })
  .validator(weddingInputSchema.extend({ id: z.uuid() }))
  .handler(async ({ data }): Promise<WeddingMutationResult> => {
    const { id, ...body } = data;
    const response = await apiFetch(`/weddings/${id}`, {
      method: "PUT",
      body: JSON.stringify(body),
    });
    if (response.status === 404) {
      throw notFound();
    }
    if (!response.ok) {
      return toMutationError(response);
    }

    return { ok: true, wedding: null };
  });

/** True when the slug is free or already belongs to the user's own wedding. */
export const checkSlugAvailability = createServerFn({ method: "GET" })
  .validator(z.object({ slug: slugSchema }))
  .handler(async ({ data }) => {
    const params = new URLSearchParams({ slug: data.slug });
    const response = await apiFetch(`/weddings/slug-availability?${params}`);
    if (!response.ok) {
      throw new Error(
        `GET /weddings/slug-availability failed with status ${response.status}`,
      );
    }

    const { available } = (await response.json()) as { available: boolean };
    return available;
  });
