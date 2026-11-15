namespace Casarme.Api.Casamentos;

public class Casamento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CoupleName { get; set; } = "";
    public string Identifier { get; set; } = "";
    public string TimeZone { get; set; } = "America/Sao_Paulo";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<Noivo> Noivos { get; set; } = [];
    public List<Evento> Events { get; set; } = [];
}

public class Noivo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AccountId { get; set; } = "";
    public Guid CasamentoId { get; set; }
    public Casamento Casamento { get; set; } = null!;
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class Evento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CasamentoId { get; set; }
    public Casamento Casamento { get; set; } = null!;
    public string Name { get; set; } = "Cerimônia";
    public DateTime StartsAt { get; set; }
    public bool IsPrincipal { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

// All event time-zone decisions go through this resolver.
public static class EventTimeZone
{
    public static string Resolve(Evento value) => value.Casamento.TimeZone;
}
