using Apps.Mansa.Handlers.Static;
using Tests.Mansa.Base;

namespace Tests.Mansa;

[TestClass]
public class HandlerTests : TestBase
{
    [TestMethod]
    public void Language_handler_returns_supported_languages()
    {
        var handler = new LanguageDataHandler();

        var result = handler.GetData().ToList();

        Assert.AreEqual(35, result.Count);
        Assert.IsTrue(result.Any(x => x.Value == "English"));
        Assert.IsTrue(result.Any(x => x.Value == "Swahili"));
        Assert.IsTrue(result.Any(x => x.Value == "Twi"));
    }
}
