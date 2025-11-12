using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.Mansa.Models;

public class FileResponse : ITranslateFileOutput
{
    public FileReference File { get; set; } = default!;
    public double Characters { get; set; }
    public double Cost { get; set; }
}
