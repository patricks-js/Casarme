import { IconCalendarHeart, IconLink, IconPencil } from "@tabler/icons-react";
import { createFileRoute, Link } from "@tanstack/react-router";
import { cn } from "cn";

import { buttonVariants } from "#components/ui/button.tsx";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "#components/ui/card.tsx";
import { getWedding } from "#functions/wedding.functions.ts";
import { formatWeddingDate, PUBLIC_HOST } from "#lib/wedding.ts";

export const Route = createFileRoute("/_app/weddings/$weddingId/")({
  loader: ({ params }) => getWedding({ data: { id: params.weddingId } }),
  component: WeddingSpace,
  notFoundComponent: () => <p>Casamento não encontrado.</p>,
});

function WeddingSpace() {
  const wedding = Route.useLoaderData();

  return (
    <Card>
      <CardHeader>
        <CardDescription>Espaço do casamento</CardDescription>
        <CardTitle className="font-serif text-3xl">
          {wedding.partnerOneName} &amp; {wedding.partnerTwoName}
        </CardTitle>
        <CardAction>
          <Link
            to="/weddings/$weddingId/edit"
            params={{ weddingId: wedding.id }}
            className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
          >
            <IconPencil />
            Editar
          </Link>
        </CardAction>
      </CardHeader>
      <CardContent className="flex flex-col gap-2 text-sm">
        <p className="flex items-center gap-2">
          <IconCalendarHeart className="text-muted-foreground size-4" />
          {formatWeddingDate(wedding.primaryDate)}
        </p>
        <p className="flex items-center gap-2">
          <IconLink className="text-muted-foreground size-4" />
          {PUBLIC_HOST}/{wedding.slug}
        </p>
      </CardContent>
    </Card>
  );
}
