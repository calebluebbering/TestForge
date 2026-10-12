# TestForge

TestForge is a small ASP.NET Core MVC application used to demonstrate automated testing with C#, xUnit, Entity Framework Core, SQLite, and Playwright.

The application manages tasks. The project focuses on testing application behavior at multiple levels and running the test suite automatically with GitHub Actions.

## Technology stack

- **C# / .NET 10**
- **ASP.NET Core MVC** for the web application
- **Entity Framework Core + SQLite** for persistence
- **xUnit** for automated tests
- **Microsoft.AspNetCore.Mvc.Testing** for integration testing
- **Microsoft.Playwright** for browser-based end-to-end tests
- **GitHub Actions** for continuous integration

## Repository structure

```text
.
├── .github/
│   └── workflows/
│       └── dotnet.yml
├── TestForge.Web/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   └── Views/
├── TestForge.Tests/
│   ├── Browser/
│   ├── Infrastructure/
│   ├── Integration/
│   └── Models/
├── playwright.runsettings
└── TestForge.slnx
```

The exact files in each folder may grow as the project develops.

## What is tested?

### Unit tests

The unit tests validate `TaskItem` model rules, including required titles and maximum lengths for titles and descriptions. These tests exercise model validation without starting the web server.

### Integration tests

The integration tests send HTTP requests to the ASP.NET Core application and verify behavior such as displaying the task page, creating valid tasks, and rejecting invalid input. They use a test-host setup with an in-memory SQLite connection rather than the development database.

### Browser tests

Playwright launches Chromium and interacts with the application through its real web UI. Current browser scenarios cover:

- Displaying the Tasks page heading
- Creating a task and verifying it appears in the list
- Editing a task and verifying the updated title
- Deleting a task and verifying it is removed

Browser tests start the web application automatically, so you do not need to start it manually before running the tests.

## Prerequisites

Install the following:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) (`pwsh`) for the Playwright install command shown below
- [Git](https://git-scm.com/downloads)

A Chromium browser installation is required for the Playwright browser tests.

## Run locally

Clone the repository and enter its directory:

```powershell
git clone https://github.com/calebluebbering/TestForge.git
cd TestForge
```

Restore and build the solution:

```powershell
dotnet restore TestForge.slnx
dotnet build TestForge.slnx
```

Install Playwright's Chromium browser:

```powershell
pwsh TestForge.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
```

Run the complete test suite:

```powershell
dotnet test TestForge.slnx --settings playwright.runsettings
```

Run only the browser tests:

```powershell
dotnet test TestForge.slnx --filter "FullyQualifiedName~TasksBrowserTests" --settings playwright.runsettings
```

Run only integration tests:

```powershell
dotnet test TestForge.slnx --filter "FullyQualifiedName~TasksIntegrationTests"
```

Run only model validation tests:

```powershell
dotnet test TestForge.slnx --filter "FullyQualifiedName~TaskItemValidationTests"
```

You can also run tests from Visual Studio or another IDE that supports .NET and xUnit.

## Browser test isolation

The web application uses SQLite for persistence. Browser tests configure a unique temporary SQLite database through the `ConnectionStrings__DefaultConnection` environment variable rather than using the normal development database.

The browser-test helper starts the application on `http://127.0.0.1:5080`, waits for it to respond, and stops the process when the test finishes. It then attempts to remove the temporary database and SQLite sidecar files.

The browser helper currently uses a fixed port (`5080`), so avoid running multiple instances of the browser suite concurrently on the same machine.

## Failure diagnostics

Browser tests use a shared helper that records a Playwright trace while a test runs. When a test fails, the helper attempts to save:

- A full-page screenshot (`.png`)
- A Playwright trace (`.zip`)

Artifacts are written under the test output directory:

```text
TestForge.Tests/bin/Debug/net10.0/TestResults/Playwright/
```

Successful tests stop tracing without saving a trace file. Artifact capture is best-effort so a screenshot or trace failure does not replace the original test failure.

To inspect a saved trace, use Playwright's trace viewer:

```powershell
pwsh TestForge.Tests/bin/Debug/net10.0/playwright.ps1 show-trace "path\to\trace.zip"
```

## Continuous integration

The GitHub Actions workflow is located at `.github/workflows/dotnet.yml`. It runs when code is pushed, a pull request is updated, or the workflow is manually dispatched.

The workflow:

1. Checks out the repository.
2. Installs the .NET 10 SDK.
3. Restores and builds `TestForge.slnx`.
4. Installs Chromium and its system dependencies.
5. Runs the test suite using `playwright.runsettings`.
6. Uploads Playwright diagnostic files when a workflow step fails and artifacts are available.

See the **Actions** tab in the GitHub repository for workflow runs and logs.

## Development notes

- The local development database is `testforge.db`; local database files should not be committed.
- Browser tests use their own temporary database.
- The project currently uses `EnsureCreated()` for the prototype database setup. If the database schema evolves, consider moving to Entity Framework Core migrations.
- Keep tests independent of local state so they can run consistently on developer machines and CI.
