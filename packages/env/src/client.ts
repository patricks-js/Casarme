import { createEnv } from "@t3-oss/env-core";

// O web só chama a api pelo servidor (ADR 0007), então ainda não há variáveis de cliente.
export const env = createEnv({
  client: {},
  clientPrefix: "VITE_",
  isServer: false,
  runtimeEnv: import.meta.env,
  emptyStringAsUndefined: true,
});
