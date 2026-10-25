import { drizzle } from "drizzle-orm/bun-sql";

import { env } from "#lib/env/server.ts";

import { relations } from "./relations";

export const db = drizzle(env.DATABASE_URL, { relations });
