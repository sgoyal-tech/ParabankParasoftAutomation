Test Strategy: ParaBank Authentication Flows

Scope
- Functional flows: Login and Registration (Flow 1 and Flow 2).
- Core scenarios include positive and negative paths, validation checks, and basic UI state assertions.
- This strategy covers test levels: UI functional tests (end-to-end) executed with Selenium WebDriver.

Test Levels and Types
- Smoke: Basic positive login and registration (happy paths).
- Functional: Negative authentication scenarios (invalid credentials, empty inputs), registration validation (password mismatch), and UI element presence.
- Regression: Re-run full suite to ensure no breakage after changes.

Test Design
- Tests implemented using Page Object Model to separate locators/actions from test logic.
- Tests are independent and repeatable: registration uses timestamped usernames to avoid conflicts.
- Assertions are explicit using `FluentAssertions` for better failure messages.

Test Data
- Minimal test data embedded in tests for demo purposes (e.g., known demo credential `john`/`demo`).
- Positive registration uses generated usernames to avoid collisions.
- Sensitive data: No credentials or secrets stored in repo. For production use, move credentials to secure storage (e.g., GitHub Secrets, Azure Key Vault).

Environment
- Tests run against the publicly accessible demo site `https://parabank.parasoft.com/`.
- Local runs require Chrome installed. `WebDriverManager` ensures an appropriate ChromeDriver is downloaded automatically.
- CI: GitHub Actions workflow provided to run tests on `Windows-latest`.

Risks & Assumptions
- UI locators are stable; if site markup changes frequently, locator maintenance will be required.
- Tests assume reasonable page load times; explicit waits are used when locating elements.

Reporting and Artifacts
- Test artifacts (lightweight logs and screenshots) saved under `TestResults/`.
- CI uploads `TestResults` artifacts for review.

Exit Criteria
- Automated tests run cleanly in CI.

Maintenance
- Keep Page Objects small and focused; add new pages for additional flows.
- Add data-driven tests for broader coverage.
- Integrate stronger reporting (e.g., ExtentReports).

