import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";

import { getSession } from "#functions/auth.functions.ts";

export const Route = createFileRoute("/_auth")({
  beforeLoad: async () => {
    const session = await getSession();

    if (session) {
      throw redirect({ to: "/" });
    }
  },
  component: AuthLayout,
});

function AuthLayout() {
  return (
    <main className="flex min-h-svh items-center justify-center p-4">
      <div className="w-full max-w-sm">
        <Outlet />
      </div>
    </main>
  );
}
