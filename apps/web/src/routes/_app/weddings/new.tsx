import {
  createFileRoute,
  redirect,
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
import { createWedding, getMyWedding } from "#functions/wedding.functions.ts";

export const Route = createFileRoute("/_app/weddings/new")({
  beforeLoad: async () => {
    // one wedding per user in the MVP
    const wedding = await getMyWedding();

    if (wedding) {
      throw redirect({
        to: "/weddings/$weddingId",
        params: { weddingId: wedding.id },
      });
    }
  },
  component: NewWedding,
});

function NewWedding() {
  const router = useRouter();
  const navigate = useNavigate();

  return (
    <Card>
      <CardHeader>
        <CardTitle>Cadastre seu casamento</CardTitle>
        <CardDescription>
          Comece pelo básico. Depois você adiciona eventos, convidados e
          convites.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <WeddingForm
          suggestSlugFromNames
          submitLabel="Criar casamento"
          submittingLabel="Criando..."
          onSubmit={(data) => createWedding({ data })}
          onSuccess={async ({ wedding }) => {
            toast.success("Casamento criado!");
            await router.invalidate();
            await navigate({
              to: "/weddings/$weddingId",
              params: { weddingId: wedding!.id },
            });
          }}
        />
      </CardContent>
    </Card>
  );
}
