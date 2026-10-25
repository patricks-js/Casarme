import {
  createFileRoute,
  useNavigate,
  useRouter,
} from "@tanstack/react-router";
import { toast } from "sonner";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "#components/ui/card.tsx";
import { WeddingForm } from "#components/wedding-form.tsx";
import { getWedding, updateWedding } from "#functions/wedding.functions.ts";

export const Route = createFileRoute("/_app/weddings/$weddingId/edit")({
  loader: ({ params }) => getWedding({ data: { id: params.weddingId } }),
  component: EditWedding,
  notFoundComponent: () => <p>Casamento não encontrado.</p>,
});

function EditWedding() {
  const wedding = Route.useLoaderData();
  const router = useRouter();
  const navigate = useNavigate();

  return (
    <Card>
      <CardHeader>
        <CardTitle>Editar casamento</CardTitle>
        <CardDescription>
          Atualize os nomes, a data principal e o link do casamento.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <WeddingForm
          defaultValues={{
            partnerOneName: wedding.partnerOneName,
            partnerTwoName: wedding.partnerTwoName,
            slug: wedding.slug,
            primaryDate: wedding.primaryDate,
          }}
          submitLabel="Salvar alterações"
          submittingLabel="Salvando..."
          onSubmit={(data) =>
            updateWedding({ data: { ...data, id: wedding.id } })
          }
          onSuccess={async () => {
            toast.success("Alterações salvas!");
            await router.invalidate();
            await navigate({
              to: "/weddings/$weddingId",
              params: { weddingId: wedding.id },
            });
          }}
        />
      </CardContent>
    </Card>
  );
}
