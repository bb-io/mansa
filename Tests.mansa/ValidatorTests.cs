using System.Text;
using Apps.Mansa.Connections;
using Apps.Mansa.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Newtonsoft.Json.Linq;
using Tests.Mansa.Base;

namespace Tests.Mansa;

[TestClass]
public class ConnectionValidatorTests : TestBase
{
    [TestMethod]
    public async Task ValidateConnection_ValidData_ShouldBeSuccessful()
    {
        var token = Creds.FirstOrDefault(x => x.KeyName == CredsNames.Token)?.Value;
        if (string.IsNullOrWhiteSpace(token))
        {
            Assert.Inconclusive("Skipped live validation test: no API key configured in appsettings.json.");
        }

        var validator = new ConnectionValidator();

        var result = await validator.ValidateConnection(Creds, CancellationToken.None);
        Console.WriteLine(result.Message);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateConnection_InvalidData_ShouldFail()
    {
        var validator = new ConnectionValidator();
        var newCredentials = new[]
        {
            new AuthenticationCredentialsProvider(CredsNames.Token, "invalid_api_key_for_test")
        };

        var result = await validator.ValidateConnection(newCredentials, CancellationToken.None);
        Console.WriteLine(result.Message);
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateConnection_MockServer_SendsApiKeyHeaderToV1Translate()
    {
        string? capturedPath = null;
        string? capturedApiKey = null;
        JObject? capturedBody = null;

        await RunWithMockServerAsync(async context =>
        {
            capturedPath = context.Request.Url?.AbsolutePath;
            capturedApiKey = context.Request.Headers["x-api-key"];
            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            capturedBody = JObject.Parse(await reader.ReadToEndAsync());

            const string responseJson = """{"context":"OK","data":{"text":"Hello","translation":"Jambo","inputTokens":1,"outputTokens":1,"isValid":true,"needsReview":false},"meta":null}""";
            await WriteJsonResponseAsync(context.Response, 200, responseJson);
        }, async baseUrl =>
        {
            var validator = new ConnectionValidator(baseUrl);
            var creds = new[] { new AuthenticationCredentialsProvider(CredsNames.Token, "test-secret-key") };

            var result = await validator.ValidateConnection(creds, CancellationToken.None);

            Assert.IsTrue(result.IsValid, result.Message);
            Assert.AreEqual("/v1/translate", capturedPath);
            Assert.AreEqual("test-secret-key", capturedApiKey);
            Assert.IsNotNull(capturedBody);
            Assert.IsNull(capturedBody["token"]);
            Assert.AreEqual("Hello", capturedBody["text"]?.Value<string>());
            Assert.AreEqual("Swahili", capturedBody["to"]?.Value<string>());
            Assert.AreEqual("English", capturedBody["from"]?.Value<string>());
        });
    }
}
