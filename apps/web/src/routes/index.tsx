import { createFileRoute, useRouter } from "@tanstack/react-router";
import { type FormEvent, useState } from "react";

import { GuestManager } from "#/components/guest-manager";
import { loadCasamento, saveCasamento } from "#/lib/casamento-server";

export const Route = createFileRoute("/")({
  loader: () => loadCasamento(),
  component: Home,
});

function suggestIdentifier(name: string) {
  return name
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-|-$/g, "")
    .slice(0, 80)
    .replace(/-$/, "");
}

function Home() {
  const { signedIn, casamento } = Route.useLoaderData();
  const router = useRouter();
  const [coupleName, setCoupleName] = useState(casamento?.coupleName ?? "");
  const [identifier, setIdentifier] = useState(casamento?.identifier ?? "");
  const [customIdentifier, setCustomIdentifier] = useState(!!casamento);
  const [date, setDate] = useState(casamento?.date ?? "");
  const [timeZone, setTimeZone] = useState(
    casamento?.timeZone ?? "America/Sao_Paulo",
  );
  const [errors, setErrors] = useState<Record<string, string[]>>({});
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  async function submit(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setErrors({});
    setSaved(false);
    try {
      const result = await saveCasamento({
        data: {
          id: casamento?.id,
          values: { coupleName, identifier, date, timeZone },
        },
      });
      if (result.errors) setErrors(result.errors);
      else {
        setSaved(true);
        await router.invalidate();
      }
    } catch {
      setErrors({
        form: ["Não foi possível salvar o Casamento. Tente novamente."],
      });
    } finally {
      setSaving(false);
    }
  }
  if (!signedIn)
    return (
      <main>
        <h1>Casarme</h1>
        <p>Entre na sua Conta para organizar seu Casamento.</p>
      </main>
    );
  return (
    <main>
      <h1>
        {casamento ? "Configurações do Casamento" : "Cadastre seu Casamento"}
      </h1>
      <form onSubmit={submit}>
        <label htmlFor="coupleName">Nome do casal</label>
        <input
          id="coupleName"
          required
          maxLength={200}
          value={coupleName}
          aria-describedby="coupleName-error"
          onChange={(event) => {
            setCoupleName(event.target.value);
            if (!customIdentifier)
              setIdentifier(suggestIdentifier(event.target.value));
          }}
        />
        <p id="coupleName-error">{errors["coupleName"]?.join(" ")}</p>
        <label htmlFor="date">Data do casamento</label>
        <input
          id="date"
          type="date"
          required
          value={date}
          onChange={(event) => setDate(event.target.value)}
          aria-describedby="date-error"
        />
        <p id="date-error">{errors["date"]?.join(" ")}</p>
        <label htmlFor="identifier">Identificador do Casamento</label>
        <input
          id="identifier"
          required
          maxLength={80}
          value={identifier}
          onChange={(event) => {
            setCustomIdentifier(true);
            setIdentifier(event.target.value);
          }}
          aria-describedby="identifier-error"
        />
        <p id="identifier-error">{errors["identifier"]?.join(" ")}</p>
        <label htmlFor="timeZone">Fuso horário</label>
        <input
          id="timeZone"
          required
          value={timeZone}
          onChange={(event) => setTimeZone(event.target.value)}
          aria-describedby="timeZone-error"
        />
        <p id="timeZone-error">{errors["timeZone"]?.join(" ")}</p>
        <div role="alert">
          {errors["form"]?.join(" ")}
          {errors["account"]?.join(" ")}
        </div>
        <button disabled={saving} type="submit">
          {saving ? "Salvando…" : "Salvar Casamento"}
        </button>
        {saved && <p role="status">Casamento salvo.</p>}
      </form>
      {casamento && <GuestManager casamentoId={casamento.id!} />}
    </main>
  );
}
