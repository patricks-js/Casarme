import { env } from "@casarme/env/server/auth";
import { drizzle } from "drizzle-orm/bun-sql";

import { relations } from "./relations";

export const db = drizzle(env.DATABASE_URL, { relations });
