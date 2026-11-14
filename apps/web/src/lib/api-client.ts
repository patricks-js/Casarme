import { createApiClient } from "@casarme/api-client";
import { env } from "@casarme/env/server/web";

// Server-only: the api is only called by the web server, over the internal network (ADR 0007).
export const apiClient = createApiClient(env.API_URL);
