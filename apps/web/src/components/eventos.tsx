import { type EventoResponse } from "@casarme/api-client";
import { type FormEvent, useEffect, useState } from "react";

import { loadEventos, mutateEvento } from "#/lib/evento-server";

export function Eventos({
  casamentoId,
  timeZone,
  weddingDate,
  onSaved,
}: {
  casamentoId: string;
  timeZone: string;
  weddingDate: string;
  onSaved: () => Promise<void>;
}) {
  const [events, setEvents] = useState<EventoResponse[]>([]);
  const [editing, setEditing] = useState<EventoResponse | null>(null);
  const [name, setName] = useState("");
  const [startsAt, setStartsAt] = useState("");
  const [location, setLocation] = useState("");
  const [hasReception, setHasReception] = useState(false);
  const [deadline, setDeadline] = useState("");
  const [errors, setErrors] = useState<string[]>([]);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState("");
  async function refresh() {
    setEvents(await loadEventos({ data: { casamentoId } }));
  }
  useEffect(() => {
    refresh().catch(() => setErrors(["Não foi possível carregar os Eventos."]));
  }, [casamentoId, timeZone, weddingDate]);
  function edit(value: EventoResponse | null) {
    setEditing(value);
    setName(value?.name ?? "");
    setStartsAt(value?.startsAt?.slice(0, 16) ?? "");
    setLocation(value?.location ?? "");
    setHasReception(value?.hasReception ?? false);
    setDeadline(value?.rsvpDeadline?.slice(0, 16) ?? "");
    setErrors([]);
    setMessage("");
  }
  async function mutate(
    action: "save" | "cancel" | "delete",
    value?: EventoResponse,
  ) {
    setBusy(true);
    setErrors([]);
    setMessage("");
    try {
      const result = await mutateEvento({
        data: {
          casamentoId,
          id: action === "save" ? editing?.id : value?.id,
          action,
          values:
            action === "save"
              ? {
                  name,
                  startsAt,
                  location,
                  hasReception,
                  rsvpDeadline: deadline || null,
                }
              : undefined,
        },
      });
      if (result.errors) setErrors(Object.values(result.errors).flat());
      else {
        await refresh();
        await onSaved();
        edit(null);
        setMessage(
          action === "delete"
            ? "Evento apagado."
            : action === "cancel"
              ? "Evento cancelado."
              : "Evento salvo.",
        );
      }
    } catch {
      setErrors(["Não foi possível concluir. Tente novamente."]);
    } finally {
      setBusy(false);
    }
  }
  function submit(event: FormEvent) {
    event.preventDefault();
    void mutate("save");
  }
  return (
    <section aria-labelledby="events-heading">
      <h2 id="events-heading">Eventos</h2>
      <p>Datas e horários no fuso {timeZone}.</p>
      <ul>
        {events.map((value) => (
          <li key={value.id}>
            <strong>
              {value.name}
              {value.isPrincipal ? " — Evento principal" : ""}
            </strong>
            <p>
              {value.startsAt?.replace("T", " ")} ·{" "}
              {value.location || "Local a definir"} ·{" "}
              {value.hasReception ? "Com Recepção" : "Sem Recepção"} ·{" "}
              {value.status === "cancelled" ? "Cancelado" : "Ativo"}
            </p>
            <p>
              Prazo de RSVP: {value.effectiveRsvpDeadline?.replace("T", " ")}
            </p>
            <button type="button" disabled={busy} onClick={() => edit(value)}>
              Editar {value.name}
            </button>
            {!value.isPrincipal && (
              <>
                {value.status !== "cancelled" && (
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => {
                      if (
                        window.confirm(
                          "Cancelar este Evento? Ele não poderá ser reativado.",
                        )
                      )
                        void mutate("cancel", value);
                    }}
                  >
                    Cancelar {value.name}
                  </button>
                )}
                <button
                  type="button"
                  disabled={busy}
                  onClick={() => {
                    if (window.confirm("Apagar este Evento?"))
                      void mutate("delete", value);
                  }}
                >
                  Apagar {value.name}
                </button>
              </>
            )}
          </li>
        ))}
      </ul>
      <h3>{editing ? `Editar ${editing.name}` : "Novo Evento"}</h3>
      {editing?.status === "cancelled" && (
        <p>Este Evento permanece cancelado após a edição.</p>
      )}
      <form onSubmit={submit}>
        <label>
          Nome{" "}
          <input
            required
            maxLength={200}
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </label>
        <label>
          Data e horário{" "}
          <input
            type="datetime-local"
            required
            value={startsAt}
            onChange={(e) => setStartsAt(e.target.value)}
          />
        </label>
        <label>
          Local{" "}
          <input
            required
            maxLength={500}
            value={location}
            onChange={(e) => setLocation(e.target.value)}
          />
        </label>
        <label>
          <input
            type="checkbox"
            checked={hasReception}
            onChange={(e) => setHasReception(e.target.checked)}
          />
          Com Recepção
        </label>
        <label>
          Prazo de RSVP (opcional){" "}
          <input
            type="datetime-local"
            value={deadline}
            onChange={(e) => setDeadline(e.target.value)}
          />
        </label>
        <p>Sem prazo definido, vale o início do Evento.</p>
        <button disabled={busy} type="submit">
          {busy ? "Salvando…" : "Salvar Evento"}
        </button>
        {editing && (
          <button disabled={busy} type="button" onClick={() => edit(null)}>
            Novo Evento
          </button>
        )}
      </form>
      <div role="alert">
        {errors.map((error) => (
          <p key={error}>{error}</p>
        ))}
      </div>
      <p role="status">{message}</p>
    </section>
  );
}
