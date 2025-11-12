using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Apps.Mansa.Handlers.Static;

namespace Apps.Mansa.Models;

public class TranslationRequest
{
    [Display("Text")]
    public string Text { get; set; }

    [Display("Source language")]
    [StaticDataSource(typeof(LanguageDataHandler))]
    public string From { get; set; }

    [Display("Target language")]
    [StaticDataSource(typeof(LanguageDataHandler))]
    public string To { get; set; }

    [Display("Max tokens")]
    public int? MaxTokens { get; set; }

    [Display("Creativity")]
    public double? Creativity { get; set; }

    [Display("Context")]
    public string? Context { get; set; }
}
