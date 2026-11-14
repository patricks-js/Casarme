import { jwtClient } from "better-auth/client/plugins";
import { createAuthClient } from "better-auth/react";

// Browser client for the auth service. With no `baseURL` it uses the page origin, where
// /api/auth/* is proxied to the auth service (Traefik in production, the dev server locally).
export const authClient = createAuthClient({
  plugins: [jwtClient()],
});
