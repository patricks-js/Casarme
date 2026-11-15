import {
  type ConvidadoResponse,
  type GrupoResponse,
} from "@casarme/api-client";
import { type FormEvent, useEffect, useState } from "react";

import { changeGuests, loadGroups } from "#/lib/convidado-server";

type Action = Parameters<typeof changeGuests>[0]["data"]["action"];

export function GuestManager({ casamentoId }: { casamentoId: string }) {
  const [groups, setGroups] = useState<GrupoResponse[]>([]);
  const [error, setError] = useState("");
  const [status, setStatus] = useState("");
  const [busy, setBusy] = useState(false);
  const [editing, setEditing] = useState<ConvidadoResponse | null>(null);
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [groupId, setGroupId] = useState("");
  const [groupName, setGroupName] = useState("");
  const [renaming, setRenaming] = useState<string | null>(null);
  async function refresh() {
    setGroups(await loadGroups({ data: { casamentoId } }));
  }
  useEffect(() => {
    let mounted = true;
    loadGroups({ data: { casamentoId } })
      .then((value) => {
        if (mounted) setGroups(value);
      })
      .catch(() => {
        if (mounted) setError("Não foi possível carregar os Grupos.");
      });
    return () => {
      mounted = false;
    };
  }, [casamentoId]);
  async function change(action: Action) {
    setBusy(true);
    setError("");
    setStatus("");
    try {
      const result = await changeGuests({ data: { casamentoId, action } });
      if (result.error) {
        setError(result.error);
        return false;
      }
      await refresh();
      setStatus("Alteração salva.");
      return true;
    } catch {
      setError("Não foi possível salvar. Tente novamente.");
      return false;
    } finally {
      setBusy(false);
    }
  }
  function reset() {
    setEditing(null);
    setName("");
    setEmail("");
    setPhone("");
    setGroupId("");
  }
  async function submitGuest(event: FormEvent) {
    event.preventDefault();
    const values = {
      name,
      email: email || null,
      phone: phone || null,
      groupId: groupId || null,
    };
    if (
      await change(
        editing
          ? { kind: "updateGuest", id: editing.id!, values }
          : { kind: "createGuest", values },
      )
    )
      reset();
  }
  async function submitGroup(event: FormEvent) {
    event.preventDefault();
    if (
      await change(
        renaming
          ? { kind: "renameGroup", id: renaming, name: groupName }
          : { kind: "createGroup", name: groupName },
      )
    ) {
      setRenaming(null);
      setGroupName("");
    }
  }
  return (
    <section aria-labelledby="guests-heading">
      <h2 id="guests-heading">Convidados e Grupos</h2>
      <p role="alert">{error}</p>
      <p role="status">{status}</p>
      <form onSubmit={submitGroup}>
        <h3>{renaming ? "Renomear Grupo" : "Criar Grupo"}</h3>
        <label>
          Nome do Grupo (opcional)
          <input
            maxLength={200}
            value={groupName}
            onChange={(event) => setGroupName(event.target.value)}
          />
        </label>
        <p>Sem nome, o Grupo mostra o nome do representante.</p>
        <button disabled={busy}>
          {renaming ? "Salvar nome" : "Criar Grupo"}
        </button>
        {renaming && (
          <button
            type="button"
            onClick={() => {
              setRenaming(null);
              setGroupName("");
            }}
          >
            Cancelar
          </button>
        )}
      </form>
      <form onSubmit={submitGuest}>
        <h3>{editing ? "Editar Convidado" : "Cadastrar Convidado"}</h3>
        <label>
          Nome
          <input
            required
            maxLength={200}
            value={name}
            onChange={(event) => setName(event.target.value)}
          />
        </label>
        <label>
          E-mail (opcional)
          <input
            type="email"
            maxLength={254}
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </label>
        <label>
          Telefone com DDI (opcional)
          <input
            type="tel"
            placeholder="+5581999999999"
            value={phone}
            onChange={(event) => setPhone(event.target.value)}
          />
        </label>
        <label>
          Grupo
          <select
            value={groupId}
            onChange={(event) => setGroupId(event.target.value)}
          >
            <option value="">Criar Grupo só deste Convidado</option>
            {groups.map((group) => (
              <option key={group.id} value={group.id}>
                {group.displayName ?? "Grupo sem nome"}
              </option>
            ))}
          </select>
        </label>
        <button disabled={busy}>
          {editing ? "Salvar Convidado" : "Cadastrar Convidado"}
        </button>
        {editing && (
          <button type="button" onClick={reset}>
            Cancelar
          </button>
        )}
      </form>
      {groups.length === 0 && <p>Nenhum Grupo cadastrado.</p>}
      {groups.map((group) => (
        <article key={group.id}>
          <h3>{group.displayName ?? "Grupo sem nome"}</h3>
          <button
            disabled={busy}
            onClick={() => {
              setRenaming(group.id!);
              setGroupName(group.name ?? "");
            }}
          >
            Renomear Grupo
          </button>
          {(group.members?.length ?? 0) === 0 && (
            <button
              disabled={busy}
              onClick={() => {
                if (window.confirm("Apagar este Grupo vazio?"))
                  void change({ kind: "deleteGroup", id: group.id! });
              }}
            >
              Apagar Grupo vazio
            </button>
          )}
          <ul>
            {group.members?.map((guest) => (
              <li key={guest.id}>
                {guest.name}{" "}
                {guest.isRepresentative && <strong> — Representante</strong>}{" "}
                {guest.email} {guest.phone}
                <button
                  disabled={busy}
                  onClick={() => {
                    setEditing(guest);
                    setName(guest.name ?? "");
                    setEmail(guest.email ?? "");
                    setPhone(guest.phone ?? "");
                    setGroupId(group.id!);
                  }}
                >
                  Editar ou mover
                </button>
                {!guest.isRepresentative && (
                  <button
                    disabled={busy}
                    onClick={() =>
                      void change({
                        kind: "representative",
                        id: group.id!,
                        guestId: guest.id!,
                      })
                    }
                  >
                    Definir representante
                  </button>
                )}
                <button
                  disabled={busy}
                  onClick={() => {
                    if (window.confirm(`Apagar ${guest.name}?`))
                      void change({ kind: "deleteGuest", id: guest.id! });
                  }}
                >
                  Apagar Convidado
                </button>
              </li>
            ))}
          </ul>
        </article>
      ))}
    </section>
  );
}
