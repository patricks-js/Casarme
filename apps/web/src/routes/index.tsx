import { createFileRoute, Link } from "@tanstack/react-router";

import { getMe } from "#functions/user.functions.ts";

export const Route = createFileRoute("/")({
  loader: () => getMe(),
  component: Home,
});

function Home() {
  const me = Route.useLoaderData();

  return (
    <main className="flex flex-col gap-4 p-4">
      <h1 className="text-3xl font-bold">Casarme</h1>
      {me ? (
        <p>Olá, {me.email}</p>
      ) : (
        <p>
          Sem sessão. <Link to="/sign-in">Entrar</Link>
        </p>
      )}
    </main>
  );
}
