using Blackbird.Applications.Sdk.Common;

namespace Apps.Mansa.Models;

public class TranslationResponse
{
    [Display("Translated text")]
    public string TranslatedText { get; set; }
}
