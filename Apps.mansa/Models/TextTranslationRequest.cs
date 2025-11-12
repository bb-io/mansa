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

    [Display("Max tokens", Description = "Maximum response length (defaults to 512)")]
    public int? MaxTokens { get; set; }

    [Display("Creativity", Description = "Randomness level (0.1–1.0, defaults to 0.7)")]
    public double? Creativity { get; set; }

    [Display("Context", Description = "Optional contextual information to help clarify ambiguities and improve translation accuracy (e.g., \"This phrase is from a medical document\")")]
    public string? Context { get; set; }
}
