
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TestForge.Tests.Infrastructure;
using TestForge.Web.Data;
using TestForge.Web.Models;

namespace TestForge.Tests.Integration;

public class TasksIntegrationTests(
    TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task TasksPage_ReturnsSuccessAndDisplaysHeading()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Tasks", html);
        Assert.Contains("Create task", html);
    }

    [Fact]
    public async Task CreateTask_ValidTask_SavesToDatabase()
    {
        var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        // Load the form first so we can retrieve its antiforgery token.
        var formResponse = await client.GetAsync("/Tasks/Create");
        Assert.Equal(HttpStatusCode.OK, formResponse.StatusCode);

        var html = await formResponse.Content.ReadAsStringAsync();

        var tokenMatch = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");

        Assert.True(tokenMatch.Success, "Antiforgery token was not found.");

        var token = tokenMatch.Groups[1].Value;

        using var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Title", "Integration test task"),
            new KeyValuePair<string, string>("Description", "Created by an automated test"),
            new KeyValuePair<string, string>("IsCompleted", "false"),
            new KeyValuePair<string, string>("__RequestVerificationToken", token)
        ]);

        var response = await client.PostAsync("/Tasks/Create", content);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var savedTask = db.Tasks.SingleOrDefault(
            task => task.Title == "Integration test task");

        Assert.NotNull(savedTask);
        Assert.Equal("Created by an automated test", savedTask.Description);
        Assert.False(savedTask.IsCompleted);
    }

    [Fact]
    public async Task CreateTask_EmptyTitle_DoesNotSaveTask()
    {
        var client = factory.CreateClient();

        var formResponse = await client.GetAsync("/Tasks/Create");
        var html = await formResponse.Content.ReadAsStringAsync();

        var tokenMatch = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");

        Assert.True(tokenMatch.Success, "Antiforgery token was not found.");

        using var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Title", ""),
            new KeyValuePair<string, string>("Description", "Invalid task"),
            new KeyValuePair<string, string>("IsCompleted", "false"),
            new KeyValuePair<string, string>(
                "__RequestVerificationToken",
                tokenMatch.Groups[1].Value)
        ]);

        var response = await client.PostAsync("/Tasks/Create", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.DoesNotContain(
            db.Tasks,
            task => task.Description == "Invalid task");
    }
}
