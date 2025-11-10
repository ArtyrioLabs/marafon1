using System.Text.RegularExpressions;
using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using Reqnroll.BoDi;
using Serilog;
using Tests.Api.Clients;
using Tests.Core.Configuration;
using Tests.Core.Drivers;

namespace Tests.Hooks
{
    [Binding]
    public class TestHooks(ScenarioContext scenarioContext, IObjectContainer container)
    {
        private readonly ScenarioContext _scenarioContext = scenarioContext;
        private readonly IObjectContainer _container = container;

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Initialize ConfigManager (reads from appsettings.json, ENV, and TestContext.Parameters)
            ConfigManager.Initialize();
            var logPath = ConfigManager.Settings.Logging.FilePath.Replace("{Date}", DateTime.Now.ToString("yyyyMMdd"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Test run started at {StartTime}", DateTime.Now);
            Log.Information("Configuration loaded:");
            Log.Information("  BaseUrl: {Ui}", ConfigManager.Settings.BaseUrls.Ui);
            Log.Information("  API: {Api}", ConfigManager.Settings.BaseUrls.Api);
            Log.Information("  Browser: {Browser} (Headless: {Headless})",
                ConfigManager.Settings.Browser.Type,
                ConfigManager.Settings.Browser.Headless);

            // Log test parameters if provided
            try
            {
                if (TestContext.Parameters.Count > 0)
                {
                    Log.Information("Test parameters provided:");
                    foreach (var paramName in TestContext.Parameters.Names)
                    {
                        Log.Information("  {ParamName} = {ParamValue}",
                            paramName, TestContext.Parameters[paramName]);
                    }
                }
            }
            catch { /* TestContext not available during initialization */ }
        }

        [BeforeScenario(Order = 1)]
        public void RegisterDependencies()
        {
            // Register configuration
            var config = new ConfigurationManager();
            _container.RegisterInstanceAs<IConfigurationManager>(config);

            // Register logger
            _container.RegisterInstanceAs(Log.Logger);

            // Register drivers
            _container.RegisterTypeAs<BrowserDriver, IBrowserDriver>();
            _container.RegisterTypeAs<ApiDriver, IApiDriver>();

            Log.Information("Starting scenario: {ScenarioTitle}", _scenarioContext.ScenarioInfo.Title);
        }

        [BeforeScenario(Order = 2)]
        public async Task InitializeApiDriver()
        {
            var apiDriver = _container.Resolve<IApiDriver>();
            await apiDriver.InitializeAsync();

            // Register API context
            _container.RegisterInstanceAs<IAPIRequestContext>(apiDriver.Context);

            // Register API clients
            _container.RegisterTypeAs<RoomApiClient, RoomApiClient>();
            _container.RegisterTypeAs<UserApiClient, UserApiClient>();
            _container.RegisterTypeAs<SystemApiClient, SystemApiClient>();
        }

        [BeforeScenario("ui", Order = 2)]
        public async Task InitializeBrowserDriver()
        {
            var browserDriver = _container.Resolve<IBrowserDriver>();
            await browserDriver.InitializeAsync();

            // Register page for steps
            _container.RegisterInstanceAs(browserDriver.Page);

            var config = _container.Resolve<IConfigurationManager>();
            var baseUrl = config.Settings.BaseUrls.Ui;

            _scenarioContext.Set(baseUrl, "baseUrl");
        }

        [AfterScenario("ui", Order = 100)]
        public async Task TakeScreenshotOnFailure()
        {
            if (_scenarioContext.TestError != null && _container.IsRegistered<IBrowserDriver>())
            {
                var browserDriver = _container.Resolve<IBrowserDriver>();
                if (browserDriver.Page != null)
                {
                    var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                    Directory.CreateDirectory(screenshotDir);

                    var arguments = _scenarioContext.ScenarioInfo.Arguments;
                    // Truncate long argument values to prevent Windows path length issues (max 260 chars)
                    string argumentsString = string.Join("_", arguments.Values.Cast<object>().Select(v =>
                    {
                        var str = v?.ToString()?.Replace(" ", "_") ?? "";
                        // Limit each argument to 50 characters to keep filename reasonable
                        return str.Length > 50 ? str.Substring(0, 50) + "..." : str;
                    }));

                    var rawFileName = $"{_scenarioContext.ScenarioInfo.Title}_{argumentsString}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    var fileName = Regex.Replace(rawFileName, @"[<>:""/\\|?*]", "_");
                    
                    // Ensure total path length doesn't exceed Windows limit (260 chars)
                    // Screenshot dir + filename should be < 260
                    var maxFileNameLength = 260 - screenshotDir.Length - 10; // 10 for safety margin
                    if (fileName.Length > maxFileNameLength)
                    {
                        var extension = Path.GetExtension(fileName);
                        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        fileName = nameWithoutExt.Substring(0, maxFileNameLength - extension.Length) + extension;
                    }
                    
                    var screenshotPath = Path.Combine(screenshotDir, fileName);

                    await browserDriver.Page.ScreenshotAsync(new PageScreenshotOptions
                    {
                        Path = screenshotPath,
                        FullPage = true
                    });

                    Log.Error(_scenarioContext.TestError,
                        "Test failed. Screenshot saved to {Path}", screenshotPath);
                }
            }
        }

        [AfterScenario(Order = 200)]
        public async Task Cleanup()
        {
            try
            {
                // Cleanup API driver
                if (_container.IsRegistered<IApiDriver>())
                {
                    var apiDriver = _container.Resolve<IApiDriver>();
                    await apiDriver.DisposeAsync();
                }

                // Cleanup browser driver
                if (_container.IsRegistered<IBrowserDriver>())
                {
                    var browserDriver = _container.Resolve<IBrowserDriver>();
                    await browserDriver.DisposeAsync();
                }

                Log.Information("Scenario completed: {ScenarioTitle}", _scenarioContext.ScenarioInfo.Title);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during cleanup");
            }
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            Log.Information("Test run completed at {EndTime}", DateTime.Now);
            Log.CloseAndFlush();
        }
    }
}
