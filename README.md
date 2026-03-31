# ParaBank Parasoft Automation

Automated UI tests for ParaBank (https://parabank.parasoft.com/) using Selenium WebDriver, NUnit and FluentAssertions.

This repository contains automated tests for the Login and Registration flows implemented using the Page Object Model.

## Prerequisites

- .NET SDK (6.0 or later). Preferably .NET 8 for CI; if your project targets .NET 10 ensure the runner supports it.
- Google Chrome installed (local runs).
- Git

Note: The project uses `WebDriverManager` to download a matching ChromeDriver at runtime, so you do not need to manage ChromeDriver manually.

## Running tests locally

1. Clone the repository

   ```bash
   git clone https://github.com/sgoyal-tech/ParabankParasoftAutomation.git
   cd ParabankParasoftAutomation
   ```

2. Restore packages

   ```bash
   dotnet restore
   ```

3. Run tests (visible Chrome by default)

   ```bash
   dotnet test
   ```

4. Headless (CI) mode

   To run tests in headless mode (recommended for CI), set the environment variable `HEADLESS=true`:

   - Linux / macOS
     ```bash
     export HEADLESS=true
     dotnet test
     ```

   - Windows (PowerShell)
     ```powershell
     $env:HEADLESS = "true"
     dotnet test
     ```

## Reports and artifacts

- Lightweight per-test logs and a basic report are written to `TestResults/reports/`.
- Screenshots captured on failure are saved to `TestResults/screenshots/`.

## Project structure (high level)

- `Pages/` - Page Objects (Login, Register, Home, Base)
- `Tests/` - NUnit test classes and `TestBase` (WebDriver setup/teardown)
- `Reports/` - lightweight report writer

## CI

A GitHub Actions workflow is included at `.github/workflows/ci.yml`. The workflow will run the tests on `ubuntu-latest` and upload `TestResults` as artifacts.

## Notes

- Tests use `FluentAssertions` for assertions.
- If you want a richer HTML report, ExtentReports integration can be added — currently a lightweight file-based reporter is used to avoid package complexity in the demo.

If you need help running tests on a specific environment, tell me your OS and .NET SDK version and I can provide tailored instructions.
