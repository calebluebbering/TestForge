
using System.Diagnostics;
using Microsoft.Playwright;

namespace TestForge.Tests.Infrastructure;

public sealed class TestWebApplication : IAsyncDisposable
{
    private readonly Process _process;
    private readonly string _databasePath;

    private TestWebApplication(Process process, string databasePath)
    {
        _process = process;
        _databasePath = databasePath;
    }

    public static async Task<TestWebApplication> StartAsync()
    {
        var projectPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "../../../../TestForge.Web"));

        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"testforge-browser-{Guid.NewGuid():N}.db");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run --no-build --no-launch-profile --urls http://127.0.0.1:5080",
            WorkingDirectory = projectPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // Override the app's normal connection string for this test run.
        startInfo.Environment["ConnectionStrings__DefaultConnection"] =
            $"Data Source={databasePath}";

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException(
                "Could not start TestForge.Web.");

        var app = new TestWebApplication(process, databasePath);

        try
        {
            using var playwright = await Playwright.CreateAsync();
            var request = await playwright.APIRequest.NewContextAsync();

            try
            {
                for (var attempt = 0; attempt < 60; attempt++)
                {
                    if (process.HasExited)
                    {
                        throw new InvalidOperationException(
                            $"TestForge.Web exited early. Exit code: {process.ExitCode}");
                    }

                    try
                    {
                        var response = await request.GetAsync(
                            "http://127.0.0.1:5080/Tasks",
                            new() { Timeout = 1000 });

                        if (response.Ok)
                            return app;
                    }
                    catch (PlaywrightException)
                    {
                        // The server may not be ready yet.
                    }

                    await Task.Delay(500);
                }
            }
            finally
            {
                await request.DisposeAsync();
            }

            throw new TimeoutException(
                "TestForge.Web did not start in time.");
        }
        catch
        {
            await app.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
        finally
        {
            _process.Dispose();

            
            // Retry cleanup while SQLite releases its file handles.
            foreach (var suffix in new[] { "", "-journal", "-wal", "-shm" })
            {
                var path = _databasePath + suffix;

                for (var attempt = 0; attempt < 10; attempt++)
                {
                    try
                    {
                        if (File.Exists(path))
                            File.Delete(path);

                        break;
                    }
                    catch (IOException) when (attempt < 9)
                    {
                        await Task.Delay(200);
                    }
                }
            }

        }
    }
}
