using System.Net;
using System.Net.Sockets;
using System.Text;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Extensions.Configuration;

namespace Tests.Mansa.Base;

public class TestBase
{
    public IEnumerable<AuthenticationCredentialsProvider> Creds { get; set; }

    public InvocationContext InvocationContext { get; set; }

    public FileManager FileManager { get; set; }

    public TestBase()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        Creds = config.GetSection("ConnectionDefinition").GetChildren()
            .Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value ?? string.Empty))
            .ToList();

        var relativePath = config.GetSection("TestFolder").Value ?? "TestFiles";
        var projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
        _ = Path.Combine(projectDirectory, relativePath);

        InvocationContext = new InvocationContext
        {
            AuthenticationCredentialsProviders = Creds,
        };

        FileManager = new FileManager();
    }

    protected static async Task WriteJsonResponseAsync(HttpListenerResponse response, int statusCode, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        await response.OutputStream.WriteAsync(bytes);
        response.Close();
    }

    protected static async Task RunWithMockServerAsync(Func<HttpListenerContext, Task> handler, Func<string, Task> testAction)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        var baseUrl = $"http://127.0.0.1:{port}/";
        using var http = new HttpListener();
        http.Prefixes.Add(baseUrl);
        http.Start();

        var serverTask = Task.Run(async () =>
        {
            var ctx = await http.GetContextAsync();
            await handler(ctx);
        });

        try
        {
            await testAction(baseUrl);
            await serverTask;
        }
        finally
        {
            http.Stop();
        }
    }
}
