using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Mansa.Actions;

[ActionList]
public class TextActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Action", Description = "Describes the action")]
    public async Task Action()
    {
        
    }
}
