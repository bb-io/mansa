using Apps.Mansa.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Mansa;

public class MansaInvocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected Client Client { get; }

    public MansaInvocable(InvocationContext invocationContext) : this(invocationContext, null)
    {
    }

    internal MansaInvocable(InvocationContext invocationContext, string? baseUrl) : base(invocationContext)
    {
        Client = new(Creds, baseUrl);
    }
}
