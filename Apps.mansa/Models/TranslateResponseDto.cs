
using Newtonsoft.Json;

namespace Apps.Mansa.Models;

public class TranslateResponseDto
{
    [JsonProperty("context")]
    public string Context { get; set; } = string.Empty;

    [JsonProperty("data")]
    public TranslateDataDto Data { get; set; } = default!;

    [JsonProperty("meta")]
    public object? Meta { get; set; }
}

public class TranslateDataDto
{
    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    [JsonProperty("translation")]
    public string? Translation { get; set; }

    [JsonProperty("inputTokens")]
    public double InputTokens { get; set; }

    [JsonProperty("outputTokens")]
    public double OutputTokens { get; set; }

    [JsonProperty("isValid")]
    public bool IsValid { get; set; }

    [JsonProperty("needsReview")]
    public bool NeedsReview { get; set; }

    [JsonProperty("billing")]
    public BillingInfo? Billing { get; set; }
}

public class BillingInfo
{
    [JsonProperty("cost")]
    public double Cost { get; set; }

    [JsonProperty("costUsd")]
    public double? CostUsd { get; set; }

    [JsonProperty("creditsCharged")]
    public double? CreditsCharged { get; set; }

    [JsonProperty("balanceBefore")]
    public double BalanceBefore { get; set; }

    [JsonProperty("balanceAfter")]
    public double BalanceAfter { get; set; }

    [JsonProperty("pricing")]
    public PricingInfo? Pricing { get; set; }
}

public class PricingInfo
{
    [JsonProperty("rate")]
    public string Rate { get; set; } = string.Empty;
}