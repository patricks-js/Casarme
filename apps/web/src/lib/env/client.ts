import { createEnv } from "@t3-oss/env-core";
import { z } from "zod";

export const env = createEnv({
  client: {
    VITE_API_URL: z.url().min(1),
  },
  clientPrefix: "VITE_",
  isServer: false,
  runtimeEnv: import.meta.env,
  emptyStringAsUndefined: true,
});
