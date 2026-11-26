using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.Mansa.Models;

public class TextResponse : ITranslateTextOutput
{
    [Display("Translated text")]
    public string TranslatedText { get; set; } = string.Empty;

    public double Characters { get; set; }

    [Display("Input tokens")]
    public double InputTokens { get; set; }

    [Display("Output tokens")]
    public double OutputTokens { get; set; }

    [Display("Is valid")]
    public bool IsValid { get; set; }

    [Display("Needs review")]
    public bool NeedsReview { get; set; }

    public double Cost { get; set; }

    [Display("Cost (USD)")]
    public double? CostUsd { get; set; }

    [Display("Credits charged")]
    public double? CreditsCharged { get; set; }

    [Display("Balance before")]
    public double BalanceBefore { get; set; }

    [Display("Balance after")]
    public double BalanceAfter { get; set; }

    public string? Rate { get; set; }
}
