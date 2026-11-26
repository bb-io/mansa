using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.Mansa.Models;

public class FileResponse : ITranslateFileOutput
{
    public FileReference File { get; set; } = default!;
    public double Characters { get; set; }

    [Display("Input tokens")]
    public double InputTokens { get; set; }

    [Display("Output tokens")]
    public double OutputTokens { get; set; }

    [Display("Invalid segments")]
    public int InvalidSegments { get; set; }

    [Display("Segments needing review")]
    public int NeedsReviewSegments { get; set; }

    public double Cost { get; set; }
}
