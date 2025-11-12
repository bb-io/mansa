using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.Mansa.Models;

public class TextResponse : ITranslateTextOutput
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
