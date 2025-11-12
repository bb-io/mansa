using Blackbird.Applications.Sdk.Common;

namespace Apps.Mansa.Models;

public class TranslationResponse
{
    [Display("Translated text")]
    public string TranslatedText { get; set; }

    public double Characters { get; set; }
    public double Cost { get; set; }

    [Display("Balance before")]
    public double BalanceBefore { get; set; }

    [Display("Balance after")]
    public double BalanceAfter { get; set; }

    public string? Rate { get; set; }

}
