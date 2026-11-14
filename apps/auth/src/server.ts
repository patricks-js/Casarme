import { env } from "@casarme/env/server/auth";
import { sql } from "drizzle-orm";
import { auth } from "./lib/auth";
import { db } from "./db";

const server = Bun.serve({
  port: env.PORT,
  routes: {
    "/health": async () => {
      try {
        await db.execute(sql`SELECT 1`);
        return Response.json({ status: "Healthy" });
      } catch {
        return Response.json({ status: "Unhealthy" }, { status: 503 });
      }
    },
    "/api/auth/*": (req) => auth.handler(req),
  },
  fetch: () => new Response("Not Found", { status: 404 }),
});

console.log(`auth ouvindo em ${server.url}`);
