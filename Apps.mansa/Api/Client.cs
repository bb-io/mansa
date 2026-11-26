using Apps.Mansa.Constants;
using Apps.Mansa.Models;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Mansa.Api;

public class Client : BlackBirdRestClient
{
    public Client(IEnumerable<AuthenticationCredentialsProvider> creds, string? baseUrl = null)
        : base(new()
        {
            BaseUrl = new Uri(baseUrl ?? "https://api.mymansa.ai"),
        })
    {
        var apiKey = creds.Get(CredsNames.Token).Value;
        this.AddDefaultHeader("Content-Type", "application/json");
        this.AddDefaultHeader("x-api-key", apiKey);
    }

    public async Task<T> ExecuteJsonAsync<T>(RestRequest request, object body)
    {
        request.AddJsonBody(body);

        var response = await ExecuteAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw ConfigureErrorException(response);
        }

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            throw new PluginApplicationException("Mansa API returned an empty response.");
        }

        return JsonConvert.DeserializeObject<T>(response.Content)
            ?? throw new PluginApplicationException("Failed to deserialize Mansa API response.");
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        if (!string.IsNullOrWhiteSpace(response.Content))
        {
            try
            {
                var error = JsonConvert.DeserializeObject<HttpErrorDto>(response.Content);
                if (error != null && (!string.IsNullOrWhiteSpace(error.Detail) || !string.IsNullOrWhiteSpace(error.Title)))
                {
                    var parts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(error.Title)) parts.Add(error.Title);
                    if (!string.IsNullOrWhiteSpace(error.Detail)) parts.Add(error.Detail);
                    if (error.InvalidParams is { Count: > 0 })
                    {
                        var paramDetails = string.Join("; ", error.InvalidParams.Select(p => $"{p.Name}: {p.Reason}"));
                        parts.Add($"Invalid parameters: {paramDetails}");
                    }
                    return new PluginApplicationException($"({response.StatusCode}): {string.Join(" - ", parts)}");
                }
            }
            catch (JsonException)
            {
                // Fall back to raw response content when body is not HttpErrorDto.
            }
        }

        var message = $"({response.StatusCode}): {response.Content}";
        return new PluginApplicationException(message);
    }
}

