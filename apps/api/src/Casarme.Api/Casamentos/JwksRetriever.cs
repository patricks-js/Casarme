using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Casarme.Api.Casamentos;

// better-auth exposes a JWKS endpoint, not an OpenID discovery document.
public sealed class JwksRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
{
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
    {
        var document = await retriever.GetDocumentAsync(address, cancel);
        var configuration = new OpenIdConnectConfiguration();
        foreach (var key in new JsonWebKeySet(document).GetSigningKeys()) configuration.SigningKeys.Add(key);
        return configuration;
    }
}
