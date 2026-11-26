using System.Text;
using Apps.Mansa.Actions;
using Apps.Mansa.Constants;
using Apps.Mansa.Models;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json.Linq;
using Tests.Mansa.Base;

namespace Tests.Mansa;

[TestClass]
public class ActionTests : TestBase
{
    [TestMethod]
    public async Task TranslateText_AdheresToOpenApiRequestAndResponseSchema()
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

            const string responseJson = """
                {
                  "context": "OK",
                  "data": {
                    "text": "Good morning",
                    "translation": "Habari za asubuhi",
                    "inputTokens": 3,
                    "outputTokens": 5,
                    "isValid": true,
                    "needsReview": false,
                    "billing": {
                      "cost": 0.0025,
                      "costUsd": 0.0025,
                      "creditsCharged": 1,
                      "balanceBefore": 10.0,
                      "balanceAfter": 9.9975,
                      "pricing": {
                        "rate": "0.0001/char"
                      }
                    }
                  },
                  "meta": null
                }
                """;
            await WriteJsonResponseAsync(context.Response, 200, responseJson);
        }, async baseUrl =>
        {
            var actions = new TranslationActions(CreateInvocationContext("test-key"), FileManager, baseUrl);

            var result = await actions.TranslateText(new TextTranslationRequest
            {
                Text = "Good morning",
                From = "English",
                TargetLanguage = "Swahili",
                MaxTokens = 256,
                Creativity = 0.5,
                Context = "Greeting"
            });

            Assert.AreEqual("/v1/translate", capturedPath);
            Assert.AreEqual("test-key", capturedApiKey);
            Assert.IsNotNull(capturedBody);
            Assert.IsNull(capturedBody["token"]);
            Assert.IsNull(capturedBody["max_tokens"]);
            Assert.AreEqual("Good morning", capturedBody["text"]?.Value<string>());
            Assert.AreEqual("English", capturedBody["from"]?.Value<string>());
            Assert.AreEqual("Swahili", capturedBody["to"]?.Value<string>());
            Assert.AreEqual(256, capturedBody["maxTokens"]?.Value<int>());
            Assert.AreEqual(0.5, capturedBody["creativity"]?.Value<double>());
            Assert.AreEqual("Greeting", capturedBody["context"]?.Value<string>());

            Assert.AreEqual("Habari za asubuhi", result.TranslatedText);
            Assert.AreEqual(12, result.Characters);
            Assert.AreEqual(3, result.InputTokens);
            Assert.AreEqual(5, result.OutputTokens);
            Assert.IsTrue(result.IsValid);
            Assert.IsFalse(result.NeedsReview);
            Assert.AreEqual(0.0025, result.Cost);
            Assert.AreEqual(0.0025, result.CostUsd);
            Assert.AreEqual(1, result.CreditsCharged);
            Assert.AreEqual(10.0, result.BalanceBefore);
            Assert.AreEqual(9.9975, result.BalanceAfter);
            Assert.AreEqual("0.0001/char", result.Rate);
        });
    }

    [TestMethod]
    public async Task TranslateText_WithoutBilling_HandlesNullableBillingGracefully()
    {
        await RunWithMockServerAsync(async context =>
        {
            const string responseJson = """
                {
                  "context": "OK",
                  "data": {
                    "text": "Hello",
                    "translation": "Jambo",
                    "inputTokens": 1,
                    "outputTokens": 1,
                    "isValid": true,
                    "needsReview": false
                  },
                  "meta": null
                }
                """;
            await WriteJsonResponseAsync(context.Response, 200, responseJson);
        }, async baseUrl =>
        {
            var actions = new TranslationActions(CreateInvocationContext("test-key"), FileManager, baseUrl);

            var result = await actions.TranslateText(new TextTranslationRequest
            {
                Text = "Hello",
                From = "English",
                TargetLanguage = "Swahili"
            });

            Assert.AreEqual("Jambo", result.TranslatedText);
            Assert.AreEqual(0, result.Cost);
            Assert.AreEqual(0, result.BalanceBefore);
            Assert.AreEqual(0, result.BalanceAfter);
            Assert.IsNull(result.Rate);
        });
    }

    [TestMethod]
    public async Task TranslateText_HttpErrorDto_FormatsPluginApplicationException()
    {
        await RunWithMockServerAsync(async context =>
        {
            const string errorJson = """
                {
                  "type": "about:blank",
                  "title": "Bad Request",
                  "isError": true,
                  "detail": "Unsupported language pair",
                  "invalidParams": [
                    { "name": "to", "reason": "Language is not supported" }
                  ],
                  "context": "ValidationError",
                  "description": null
                }
                """;
            await WriteJsonResponseAsync(context.Response, 400, errorJson);
        }, async baseUrl =>
        {
            var actions = new TranslationActions(CreateInvocationContext("test-key"), FileManager, baseUrl);

            var ex = await Assert.ThrowsExceptionAsync<PluginApplicationException>(async () =>
                await actions.TranslateText(new TextTranslationRequest
                {
                    Text = "Hello",
                    From = "English",
                    TargetLanguage = "InvalidLang"
                }));

            StringAssert.Contains(ex.Message, "Bad Request");
            StringAssert.Contains(ex.Message, "Unsupported language pair");
            StringAssert.Contains(ex.Message, "to: Language is not supported");
        });
    }

    [TestMethod]
    public async Task TranslateContent_SetsNewMansaToolReferenceInXliff()
    {
        var memoryFiles = new InMemoryFileManagementClient(
            "sample.html",
            "<html><body><p>Hello world</p></body></html>");

        await RunWithMockServerAsync(async context =>
        {
            const string responseJson = """
                {
                  "context": "OK",
                  "data": {
                    "text": "Hello world",
                    "translation": "Habari dunia",
                    "inputTokens": 2,
                    "outputTokens": 2,
                    "isValid": true,
                    "needsReview": false,
                    "billing": {
                      "cost": 0.001,
                      "balanceBefore": 5.0,
                      "balanceAfter": 4.999,
                      "pricing": { "rate": "0.0001/char" }
                    }
                  },
                  "meta": null
                }
                """;
            await WriteJsonResponseAsync(context.Response, 200, responseJson);
        }, async baseUrl =>
        {
            var actions = new TranslationActions(CreateInvocationContext("test-key"), memoryFiles, baseUrl);

            var result = await actions.TranslateContent(new ContentTranslationRequest
            {
                File = new FileReference { Name = "sample.html" },
                SourceLanguage = "English",
                TargetLanguage = "Swahili"
            });

            Assert.AreEqual(0.001, result.Cost);
            Assert.AreEqual(2, result.InputTokens);
            Assert.AreEqual(2, result.OutputTokens);

            var xliffContent = memoryFiles.UploadedFiles[result.File.Name];
            StringAssert.Contains(xliffContent, "Habari dunia");
            StringAssert.Contains(xliffContent, "https://mymansa.ai");
            Assert.IsFalse(xliffContent.Contains("all-lab-portal.com", StringComparison.OrdinalIgnoreCase));
        });
    }

    private static InvocationContext CreateInvocationContext(string apiKey) => new()
    {
        AuthenticationCredentialsProviders =
        [
            new AuthenticationCredentialsProvider(CredsNames.Token, apiKey)
        ]
    };

    private sealed class InMemoryFileManagementClient(string fileName, string content) : IFileManagementClient
    {
        public Dictionary<string, string> UploadedFiles { get; } = [];

        public Task<Stream> DownloadAsync(FileReference reference)
        {
            Assert.AreEqual(fileName, reference.Name);
            return Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(content)));
        }

        public Task<FileReference> UploadAsync(Stream stream, string contentType, string uploadedFileName)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            UploadedFiles[uploadedFileName] = reader.ReadToEnd();
            return Task.FromResult(new FileReference { Name = uploadedFileName, ContentType = contentType });
        }
    }
}
