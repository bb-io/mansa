
using Newtonsoft.Json;

namespace Apps.Mansa.Models;

public class TranslateResponseDto
{
    
    [JsonProperty("translation")]
    public string Translation { get; set; }

    [JsonProperty("billing")]
    public BillingInfo Billing { get; set; }
}

public class BillingInfo
{
    [JsonProperty("characters")]
    public int Characters { get; set; }

    [JsonProperty("cost")]
    public double Cost { get; set; }

    [JsonProperty("balanceBefore")]
    public double BalanceBefore { get; set; }

    [JsonProperty("balanceAfter")]
    public double BalanceAfter { get; set; }

    [JsonProperty("pricing")]
    public PricingInfo Pricing { get; set; }
}

public class PricingInfo
{
    [JsonProperty("rate")]
    public string Rate { get; set; }
}