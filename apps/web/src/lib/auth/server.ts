import { drizzleAdapter } from "better-auth/adapters/drizzle";
import { betterAuth } from "better-auth/minimal";
import { jwt, openAPI } from "better-auth/plugins";
import { tanstackStartCookies } from "better-auth/tanstack-start";

import { db } from "#db/index.ts";
import * as schema from "#db/schema.ts";

export const auth = betterAuth({
  database: drizzleAdapter(db, {
    provider: "pg",
    schema: {
      user: schema.users,
      account: schema.accounts,
      session: schema.sessions,
      verification: schema.verifications,
      jwks: schema.jwks,
    },
  }),
  emailAndPassword: {
    enabled: true,
    password: {
      hash: async (password: string) => Bun.password.hash(password),
      verify: async (data) => Bun.password.verify(data.password, data.hash),
    },
  },
  advanced: {
    database: {
      generateId: () => Bun.randomUUIDv7(),
    },
  },
  plugins: [
    jwt({
      jwks: {
        // .NET (Microsoft.IdentityModel) can't verify EdDSA, the default
        keyPairConfig: { alg: "ES256" },
      },
      jwt: {
        audience: "http://localhost:5264",
      },
    }),
    openAPI(),
    tanstackStartCookies(),
  ],
});
