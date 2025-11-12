using Apps.Mansa.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.Mansa.Models;

public class TextTranslationRequest : ITranslateTextInput
{
    [Display("Text", Description = "Text to translate")]
    public string Text { get; set; }

    [Display("Source language")]
    [StaticDataSource(typeof(LanguageDataHandler))]
    public string From { get; set; }

    [Display("Target language")]
    [StaticDataSource(typeof(LanguageDataHandler))]
    public string TargetLanguage { get; set; }

    [Display("Max tokens")]
    public int? MaxTokens { get; set; }

    [Display("Creativity")]
    public double? Creativity { get; set; }

    [Display("Context")]
    public string? Context { get; set; }
}
