import { createEnv } from "@t3-oss/env-core";
import { z } from "zod";

export const env = createEnv({
  server: {
    PORT: z.coerce.number().int().positive().default(3001),
    DATABASE_URL: z.url().startsWith("postgresql://"),
    // Origem pública: o web repassa /api/auth/* para o auth, então a URL é a do web.
    BETTER_AUTH_URL: z.url().min(1),
    BETTER_AUTH_SECRET: z.string().min(32),
    GOOGLE_CLIENT_ID: z.string().min(1).optional(),
    GOOGLE_CLIENT_SECRET: z.string().min(1).optional(),
  },
  isServer: true,
  runtimeEnv: Bun.env,
  emptyStringAsUndefined: true,
});
