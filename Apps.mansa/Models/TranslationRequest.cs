using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Mansa.Models;

public class TranslationRequest
{
    [Display("Text")]
    public string Text { get; set; }

    [Display("Source language")]
    public string From { get; set; }

    [Display("Target language")]
    public string To { get; set; }

    [Display("Max tokens")]
    public int? MaxTokens { get; set; }

    [Display("Creativity")]
    public double? Creativity { get; set; }

    [Display("Context")]
    public string? Context { get; set; }
}
