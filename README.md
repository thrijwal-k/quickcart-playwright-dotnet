# QuickCart: .NET 8 Playwright + Reqnroll BDD test automation framework

UI, API and data tests for **QuickCart**, a small ReactJS shop with a REST API, written in **C# / .NET 8** with
**Microsoft.Playwright**, **Reqnroll** (the SpecFlow successor) for BDD, **NUnit**, the **Page Object Model**,
**RestSharp** API tests, **EPPlus** Excel test data, a **SQLite** reference database and **Serilog** logging.
Pipelines are included for **Azure DevOps** and **GitHub Actions**.

| | |
|---|---|
| BDD scenarios (Gherkin, Reqnroll) | 27 across 4 features: sign-in, products, cart, checkout |
| API tests (NUnit + RestSharp) | 19: contract, status codes, auth, order totals, 7 validation rules |
| Data-driven | Scenario Outlines, NUnit `TestCase` / `TestCaseSource`, Excel rows read with EPPlus |
| Database checks | UI prices and API data asserted against a SQL reference database |
| Failure evidence | Playwright trace, full-page screenshot and Serilog log attached to every failed scenario |

## Framework design

```
QuickCart.Tests/
  Features/*.feature        Gherkin: Background, Scenario, Scenario Outline; tags become NUnit categories
  StepDefinitions/          Thin step bindings - no selectors, only page-object calls and Expect assertions
  Pages/                    Page Objects - every locator and page action lives here (BasePage for the header)
  Hooks/PlaywrightHooks.cs  One browser per run, a fresh context per scenario, tracing, screenshots on failure
  Api/                      RestSharp API tests (NUnit) with shared client in ApiTestBase
  Support/
    TestSettings.cs         Environment switching: config/{TEST_ENV}.json, overridable by BASE_URL / API_URL
    ExcelTestData.cs        Reads parameterised test data from TestData/quickcart-test-data.xlsx (EPPlus)
    ReferenceDatabase.cs    Isolated in-memory SQLite database seeded from Excel, queried with SQL
    TestLog.cs              Serilog structured logging to console and file
  config/                   local.json, sit.json, uat.json
azure-pipelines.yml         Azure DevOps: environment and category parameters, VSTest results, artefacts
.github/workflows/          The same run on GitHub Actions in the official Playwright .NET container
app/, server/               The application under test (React + Node REST API)
```

Design choices:

- **No hard sleeps.** Every check uses Playwright's auto-retrying `Expect(...)` assertions and auto-wait, which is
  what removes most flakiness.
- **Readable steps, reusable pages.** Steps call page-object methods only, so a UI change is fixed in one place.
- **Isolated scenarios.** Each scenario gets its own browser context and its own test data; the reference database
  is a fresh in-memory copy per run.
- **Environment switching.** `TEST_ENV=sit dotnet test` points the same tests at SIT; pipeline variables override URLs.
- **Fast diagnosis.** Failed scenarios keep a trace (`pwsh bin/Release/net8.0/playwright.ps1 show-trace <file>`),
  a screenshot and a log, all attached to the NUnit result and published by the pipeline.

## Run it locally

```bash
npm ci                                   # app under test
npm run api & npm run app &              # start the API (3001) and the shop (5173)
dotnet build QuickCart.Tests -c Release
pwsh QuickCart.Tests/bin/Release/net8.0/playwright.ps1 install chromium
dotnet test QuickCart.Tests -c Release                         # everything
dotnet test QuickCart.Tests -c Release --filter Category=smoke # @smoke scenarios only
dotnet test QuickCart.Tests -c Release --filter Category=api   # API tests only
BROWSER=firefox dotnet test QuickCart.Tests -c Release         # another browser
```

## Azure DevOps

Create a pipeline from `azure-pipelines.yml`. It takes an **environment** parameter (local, sit, uat) and an optional
**category** filter, publishes `.trx` results with `PublishTestResults@2`, and uploads screenshots, traces and logs.
To use it as a PR gate, add it as a Build Validation policy on `main`. For SIT and UAT, link a variable group holding
the URLs and service-account secrets.

## Use of AI

I used AI assistants (Claude, GitHub Copilot) to speed up scaffolding, then reviewed, ran and corrected the output
myself. The design mirrors my TypeScript version of the same framework:
[quickcart-playwright-bdd](https://github.com/thrijwal-k/quickcart-playwright-bdd).
