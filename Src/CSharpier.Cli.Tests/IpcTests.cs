namespace CSharpier.Cli.Tests;

using System.Diagnostics;
using System.Net.Http.Json;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class IpcTests
{
    private static readonly HttpClient httpClient = new HttpClient();

    // TODO server add other tests
    // starting on port
    // ignore file
    // option file
    [Test]
    [Ignore("not working on GH/linux")]
    public async Task Stuff()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "dotnet-csharpier.dll");

        var processStartInfo = new ProcessStartInfo("dotnet", $"{path} --ipc")
        {
            UseShellExecute = false,
            ErrorDialog = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            EnvironmentVariables = { ["DOTNET_NOLOGO"] = "1" },
        };

        var process = new Process { StartInfo = processStartInfo };
        try
        {
            process.Start();

            var portString = (await process.StandardOutput.ReadLineAsync() ?? string.Empty).Replace(
                "Started on ",
                string.Empty
            );
            var port = int.Parse(portString);
            var data = new FormatFileDto
            {
                FileName = "/Temp/test.cs",
                FileContents = "public class TestClass    { }"
            };

            var response = await httpClient.PostAsJsonAsync(
                $"http://localhost:{port}/format",
                data
            );
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<FormatFileResult>();
            if (result == null)
            {
                Assert.Fail("Result is null");
            }

            result!.FormattedFile!.TrimEnd().Should().Be("public class TestClass { }");
        }
        finally
        {
            process.Kill();
        }
    }
}
