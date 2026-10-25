import {
  createFileRoute,
  Link,
  Outlet,
  redirect,
} from "@tanstack/react-router";

import { getSession } from "#functions/auth.functions.ts";

// Minimal guard for the couple's area; redirect-back and logout land with #4
export const Route = createFileRoute("/_app")({
  beforeLoad: async () => {
    const session = await getSession();

    if (!session) {
      throw redirect({ to: "/sign-in" });
    }

    return { user: session.user };
  },
  component: AppLayout,
});

function AppLayout() {
  return (
    <div className="flex min-h-svh flex-col">
      <header className="border-b px-4 py-3">
        <Link to="/" className="font-serif text-xl font-semibold">
          Casarme
        </Link>
      </header>
      <main className="mx-auto w-full max-w-2xl flex-1 p-4">
        <Outlet />
      </main>
    </div>
  );
}
