import { migrate } from "drizzle-orm/bun-sql/migrator";

import { db } from "./index";

await migrate(db, { migrationsFolder: "./drizzle", migrationsSchema: "auth" });

await db.$client.close();
console.log("migrações do schema auth aplicadas");
