namespace Casarme.Api.Entities;

public sealed class Wedding
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid OwnerId { get; init; }
    public string PartnerOneName { get; set; }
    public string PartnerTwoName { get; set; }
    public string Slug { get; private set; }
    public DateOnly PrimaryDate { get; set; }

    public Wedding(Guid ownerId, string partnerOneName, string partnerTwoName, string slug, DateOnly primaryDate)
    {
        OwnerId = ownerId;
        PartnerOneName = partnerOneName;
        PartnerTwoName = partnerTwoName;
        Slug = slug;
        PrimaryDate = primaryDate;
    }

    public void Update(string partnerOneName, string partnerTwoName, string slug, DateOnly primaryDate)
    {
        PartnerOneName = partnerOneName;
        PartnerTwoName = partnerTwoName;
        Slug = slug;
        PrimaryDate = primaryDate;
    }
}
