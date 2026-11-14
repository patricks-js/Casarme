import { createClient, createConfig } from "./generated/client";

export * from "./generated";

export type ApiClient = ReturnType<typeof createApiClient>;

/** Api client. Must only be used on the server (ADR 0007). Pass it as `client` to the generated functions. */
export function createApiClient(baseUrl: string, init?: { headers?: HeadersInit }) {
  return createClient(createConfig({ baseUrl, headers: init?.headers }));
}
