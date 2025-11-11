using Apps.Mansa.Api;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.Mansa.Connections;

public class ConnectionValidator: IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new Client(authenticationCredentialsProviders);
            var request = new RestRequest("translate", Method.Post);

            var jsonBody = new
            {
                token = authenticationCredentialsProviders.First(x => x.KeyName == "token").Value,
                text = "Hello",
                to = "Swahili",
                from = "English"
            };
            var response = await client.ExecuteWithErrorHandling(request);

            return new()
            {
                IsValid = true
            };

        } catch(Exception ex)
        {
            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }

    }
}
