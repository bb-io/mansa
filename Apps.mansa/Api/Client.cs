using Apps.Mansa.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Mansa.Api;

public class Client : BlackBirdRestClient
{
   
     private readonly string token;

    public Client(IEnumerable<AuthenticationCredentialsProvider> creds)
        : base(new()
        {
            BaseUrl = new Uri("https://all-lab-portal.com/api"),
        })
    {
        token = creds.Get(CredsNames.Token).Value;
        this.AddDefaultHeader("Content-Type", "application/json");
    }

    public async Task<T> ExecuteWithTokenAsync<T>(RestRequest request, Dictionary<string, object> bodyParams)
    {
        if (bodyParams != null && bodyParams.Any())
        {
            bodyParams.Add("token", token);
            request.AddJsonBody(bodyParams);
        }
        else
        {
            request.AddJsonBody(new { token = token });
        }

        var response = await ExecuteAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            ConfigureErrorException(response);
        }
        return JsonConvert.DeserializeObject<T>(response.Content);
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var message = $"({response.StatusCode}): {response.Content}";
        throw new PluginApplicationException(message);
    }
}

