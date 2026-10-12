
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using TestForge.Tests.Infrastructure;

namespace TestForge.Tests.Browser;

public class TasksBrowserTests : PageTest
{
    
    [Fact]
    public Task TasksPage_DisplaysHeading()
    {
        return RunWithDiagnosticsAsync(
            nameof(TasksPage_DisplaysHeading),
            async () =>
            {
                await using var app = await TestWebApplication.StartAsync();

                await Page.GotoAsync("http://127.0.0.1:5080/Tasks");

                await Expect(Page.GetByRole(
                    AriaRole.Heading,
                    new() { Name = "Tasks" }))
                    .ToBeVisibleAsync();
            });
    }


    
    [Fact]
    public Task CreateTask_ValidTask_AppearsInTaskList()
    {
        return RunWithDiagnosticsAsync(
            nameof(CreateTask_ValidTask_AppearsInTaskList),
            async () =>
            {
                await using var app = await TestWebApplication.StartAsync();

                await Page.GotoAsync("http://127.0.0.1:5080/Tasks/Create");

                await Page.GetByLabel("Title")
                    .FillAsync("TestForge browser test");

                await Page.GetByLabel("Description")
                    .FillAsync("Created automatically by Playwright.");

                await Page.GetByRole(
                    AriaRole.Button,
                    new() { Name = "Create" })
                    .ClickAsync();

                await Expect(
                    Page.GetByText("TestForge browser test"))
                    .ToBeVisibleAsync();
            });
    }

    [Fact]
    public Task EditTask_UpdatedTitle_AppearsInTaskList()
    {
        return RunWithDiagnosticsAsync(
            nameof(EditTask_UpdatedTitle_AppearsInTaskList),
            async () =>
            {
                await using var app = await TestWebApplication.StartAsync();

                await Page.GotoAsync("http://127.0.0.1:5080/Tasks/Create");

                await Page.GetByLabel("Title").FillAsync("Original task title");
                await Page.GetByLabel("Description").FillAsync("Original description");

                await Page.GetByRole(
                    AriaRole.Button,
                    new() { Name = "Create" })
                    .ClickAsync();

                await Page.GetByRole(
                    AriaRole.Link,
                    new() { Name = "Edit" })
                    .ClickAsync();

                await Page.GetByLabel("Title").FillAsync("Updated task title");

                await Page.GetByRole(
                    AriaRole.Button,
                    new() { Name = "Save" })
                    .ClickAsync();

                await Expect(
                    Page.GetByText("Updated task title"))
                    .ToBeVisibleAsync();

                await Expect(
                    Page.GetByText("Original task title"))
                    .ToHaveCountAsync(0);
            });
    }

    [Fact]
    public Task DeleteTask_RemovesTaskFromTaskList()
    {
        return RunWithDiagnosticsAsync(
            nameof(DeleteTask_RemovesTaskFromTaskList),
            async () =>
            {
                await using var app = await TestWebApplication.StartAsync();

                await Page.GotoAsync("http://127.0.0.1:5080/Tasks/Create");

                await Page.GetByLabel("Title").FillAsync("Task to delete");

                await Page.GetByLabel("Description")
                    .FillAsync("This task should be removed.");

                await Page.GetByRole(
                    AriaRole.Button,
                    new() { Name = "Create" })
                    .ClickAsync();

                Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

                await Page.GetByRole(
                    AriaRole.Button,
                    new() { Name = "Delete" })
                    .ClickAsync();

                await Expect(
                    Page.GetByText("Task to delete"))
                    .ToHaveCountAsync(0);
            });
    }


    
    private async Task RunWithDiagnosticsAsync(string testName, Func<Task> test)
    {
        var artifactDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "TestResults",
            "Playwright");

        Directory.CreateDirectory(artifactDirectory);

        var screenshotPath = Path.Combine(
            artifactDirectory,
            $"{testName}.png");

        var tracePath = Path.Combine(
            artifactDirectory,
            $"{testName}.zip");

        await Context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        try
        {
            await test();
            await Context.Tracing.StopAsync();
        }
        catch
        {
            // Preserve the original test failure if artifact capture fails.
            try
            {
                await Page.ScreenshotAsync(new()
                {
                    Path = screenshotPath,
                    FullPage = true
                });
            }
            catch
            {
                // Screenshot capture should not hide the test failure.
            }

            try
            {
                await Context.Tracing.StopAsync(new()
                {
                    Path = tracePath
                });
            }
            catch
            {
                // Trace capture should not hide the test failure.
            }

            throw;
        }
    }


}
