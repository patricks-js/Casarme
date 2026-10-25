import { createFileRoute, redirect } from "@tanstack/react-router";

import { getMyWedding } from "#functions/wedding.functions.ts";

// Entry point after sign-in: users without a wedding go to the registration flow
export const Route = createFileRoute("/_app/")({
  beforeLoad: async () => {
    const wedding = await getMyWedding();

    if (!wedding) {
      throw redirect({ to: "/weddings/new" });
    }

    throw redirect({
      to: "/weddings/$weddingId",
      params: { weddingId: wedding.id },
    });
  },
});
