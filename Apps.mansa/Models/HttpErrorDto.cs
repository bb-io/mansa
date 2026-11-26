using Newtonsoft.Json;

namespace Apps.Mansa.Models;

public class HttpErrorDto
{
    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("isError")]
    public bool IsError { get; set; }

    [JsonProperty("detail")]
    public string? Detail { get; set; }

    [JsonProperty("invalidParams")]
    public List<InvalidParamDto>? InvalidParams { get; set; }

    [JsonProperty("context")]
    public string? Context { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }
}

public class InvalidParamDto
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("reason")]
    public string Reason { get; set; } = string.Empty;
}
