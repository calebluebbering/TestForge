
using System.ComponentModel.DataAnnotations;
using TestForge.Web.Models;

namespace TestForge.Tests.Models;

public class TaskItemValidationTests
{
    [Fact]
    public void ValidTask_PassesValidation()
    {
        var task = new TaskItem
        {
            Title = "Write automated tests",
            Description = "Add unit tests to TestForge."
        };

        var results = Validate(task);

        Assert.Empty(results);
    }

    [Fact]
    public void EmptyTitle_FailsValidation()
    {
        var task = new TaskItem
        {
            Title = ""
        };

        var results = Validate(task);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(TaskItem.Title)));
    }

    [Fact]
    public void TitleLongerThan100Characters_FailsValidation()
    {
        var task = new TaskItem
        {
            Title = new string('a', 101)
        };

        var results = Validate(task);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(TaskItem.Title)));
    }

    [Fact]
    public void DescriptionLongerThan1000Characters_FailsValidation()
    {
        var task = new TaskItem
        {
            Title = "Valid title",
            Description = new string('a', 1001)
        };

        var results = Validate(task);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(TaskItem.Description)));
    }

    private static List<ValidationResult> Validate(TaskItem task)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(task);

        Validator.TryValidateObject(
            task,
            context,
            results,
            validateAllProperties: true);

        return results;
    }
}
