import { createEnv } from "@t3-oss/env-core";
import { z } from "zod";

export const env = createEnv({
  server: {
    DATABASE_URL: z.url().startsWith("postgresql://"),
    BETTER_AUTH_URL: z.url().min(1),
    BETTER_AUTH_SECRET: z.string().min(32),
    API_URL: z.url().min(1),
  },
  isServer: true,
  runtimeEnv: Bun.env,
  emptyStringAsUndefined: true,
});
