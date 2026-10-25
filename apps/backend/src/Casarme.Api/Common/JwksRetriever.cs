using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Casarme.Api.Common;

public sealed class JwksRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
{
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
        string address,
        IDocumentRetriever retriever,
        CancellationToken ct)
    {
        var json = await retriever.GetDocumentAsync(address, ct);
        var configuration = new OpenIdConnectConfiguration();

        foreach (var key in new JsonWebKeySet(json).GetSigningKeys())
            configuration.SigningKeys.Add(key);

        return configuration;
    }
}
