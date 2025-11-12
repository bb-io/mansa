using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.Mansa;

public class Application : IApplication, ICategoryProvider
{
    public IEnumerable<ApplicationCategory> Categories
    {
        get => [ApplicationCategory.MachineTranslationAndMtqe];
        set { }
    }
    public string Name
    {
        get => "Mansa";
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}
