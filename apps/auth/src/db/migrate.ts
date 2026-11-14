import { migrate } from "drizzle-orm/bun-sql/migrator";
import { db } from "./index";

await migrate(db, { migrationsFolder: "./drizzle" });

await db.$client.close();
console.log("migrações do schema auth aplicadas");
