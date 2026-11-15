using Casarme.Api.Casamentos;

namespace Casarme.Api.Convidados;

public class Grupo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CasamentoId { get; set; }
    public Casamento Casamento { get; set; } = null!;
    public string? Name { get; set; }
    public string? NameKey { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<Convidado> Members { get; set; } = [];
    public string? DisplayName => Name ?? Members.SingleOrDefault(x => x.IsRepresentative)?.Name;
    public void Normalize(DateTimeOffset now)
    {
        if (Members.Count > 0 && !Members.Any(x => x.IsRepresentative))
        {
            var first = Members.OrderBy(x => x.JoinedOrder).ThenBy(x => x.Id).First();
            first.IsRepresentative = true;
            first.UpdatedAt = now;
        }
        NameKey = DisplayName?.Trim().ToUpperInvariant();
        UpdatedAt = now;
    }
}

public class Convidado
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GrupoId { get; set; }
    public Grupo Grupo { get; set; } = null!;
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsRepresentative { get; set; }
    public long JoinedOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
