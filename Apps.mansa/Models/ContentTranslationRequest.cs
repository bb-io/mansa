using Apps.Mansa.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Handlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.Mansa.Models;

public class ContentTranslationRequest : ITranslateFileInput
{
    [Display("Content file")]
    public FileReference File { get; set; } = default!;

    [Display("Source language", Description = "The source language for translation"), StaticDataSource(typeof(LanguageDataHandler))]
    public string? SourceLanguage { get; set; }

    [Display("Target language", Description = "The target language for translation"), StaticDataSource(typeof(LanguageDataHandler))]
    public string TargetLanguage { get; set; } = string.Empty;

    [Display("Output file handling", Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps."), StaticDataSource(typeof(ProcessFileFormatHandler))]
    public string? OutputFileHandling { get; set; }

    [Display("Creativity", Description = "Randomness level (0.1–1.0, defaults to 0.7)")]
    public double? Creativity { get; set; }

    [Display("Context", Description = "Optional contextual information to help clarify ambiguities and improve translation accuracy (e.g., \"This phrase is from a medical document\")")]
    public string? Context { get; set; }

}
