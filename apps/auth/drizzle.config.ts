import { env } from "@casarme/env/server/auth";
import { defineConfig } from "drizzle-kit";

export default defineConfig({
  out: "./drizzle",
  schema: "./src/db/schema.ts",
  dialect: "postgresql",
  dbCredentials: {
    url: env.DATABASE_URL,
  },
  // Keeps the migrations history in the `auth` schema instead of a separate `drizzle` schema.
  migrations: { schema: "auth" },
});
