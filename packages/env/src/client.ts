import { createEnv } from "@t3-oss/env-core";

// The web only calls the api from the server (ADR 0007), so there are no client variables yet.
export const env = createEnv({
  client: {},
  clientPrefix: "VITE_",
  isServer: false,
  runtimeEnv: import.meta.env,
  emptyStringAsUndefined: true,
});
