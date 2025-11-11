using Apps.Mansa.Actions;
using Tests.Mansa.Base;

namespace Tests.Mansa;

[TestClass]
public class ActionTests : TestBase
{
    [TestMethod]
    public async Task Dynamic_handler_works()
    {
        var actions = new Actions(InvocationContext);

        await actions.Action();
    }
}
