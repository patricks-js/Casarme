import { env } from "@casarme/env/server/auth";
import { betterAuth } from "better-auth";
import { drizzleAdapter } from "better-auth/adapters/drizzle";
import { jwt } from "better-auth/plugins/jwt";
import { db } from "../db";
import { accounts, jwks, sessions, users, verifications } from "../db/schema";

export const auth = betterAuth({
  database: drizzleAdapter(db, {
    provider: "pg",
    schema: { user: users, session: sessions, account: accounts, verification: verifications, jwks },
  }),
  basePath: "/api/auth",
  socialProviders: {
    google: {
      clientId: env.GOOGLE_CLIENT_ID ?? "",
      clientSecret: env.GOOGLE_CLIENT_SECRET ?? "",
    },
  },
  // Emite o JWT de Conta que a api valida pelo JWKS.
  plugins: [jwt()],
});
