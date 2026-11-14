import { getStatus } from "@casarme/api-client";
import { getStatusQueryKey } from "@casarme/api-client/react-query";
import { noop, queryOptions, useQuery } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { createServerFn } from "@tanstack/react-start";

import { apiClient } from "#/lib/api-client";

// The browser never talks to the api: the query goes through a server function (ADR 0007).
const fetchApiStatus = createServerFn({ method: "GET" }).handler(async () => {
  const { data } = await getStatus({ client: apiClient, throwOnError: true });
  return data;
});

const statusQueryOptions = queryOptions({
  queryKey: getStatusQueryKey(),
  queryFn: () => fetchApiStatus(),
});

export const Route = createFileRoute("/")({
  // Warms the cache on the server; a failed fetch is not fatal, the component shows the fallback.
  loader: ({ context }) =>
    context.queryClient.query(statusQueryOptions).catch(noop),
  component: Home,
});

function Home() {
  const { data: status } = useQuery(statusQueryOptions);

  return (
    <main>
      <h1>Casarme</h1>
      {status ? (
        <p>
          A api respondeu: <strong>{status.service}</strong> (
          {status.environment}), às{" "}
          {new Date(status.now).toLocaleString("pt-BR")}.
        </p>
      ) : (
        <p>Não foi possível falar com a api.</p>
      )}
    </main>
  );
}
