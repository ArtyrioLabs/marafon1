using Microsoft.Playwright;
using Reqnroll;
using Tests.Api.Models.Responses;

namespace Tests.Ui.Steps
{
    [Binding]
    public class NavigationSteps(
            ScenarioContext scenarioContext,
            IPage page)
    {
        [Given("I am on the home page")]
        public async Task GivenIAmOnTheHomePage()
        {
            var baseUrl = scenarioContext.Get<string>("baseUrl");

            await page.GotoAsync(baseUrl);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        [When("I navigate to room page with admin code")]
        public async Task WhenINavigateToRoomPageWithAdminCode()
        {
            var response = scenarioContext.Get<RoomCreationResponse>("RoomCreationResponse");
            var baseUrl = scenarioContext.Get<string>("baseUrl");

            await page.GotoAsync($"{baseUrl}/room/{response.UserCode}", new()
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            // Wait for Angular app to initialize
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Wait for room name to appear (indicates room data loaded from API)
            var roomNameLocator = page.Locator("xpath=.//h1[contains(@class,'room-info__title')] | .//h1[contains(@class,'title')]");
            await roomNameLocator.WaitForAsync(new() 
            { 
                State = Microsoft.Playwright.WaitForSelectorState.Visible, 
                Timeout = 15000 
            });

            // Wait for participant list to appear (indicates users data loaded from API)
            var participantListLocator = page.Locator("xpath=.//*[contains(@class,'pl-counter')] | .//*[contains(@class,'participant-list')]");
            await participantListLocator.WaitForAsync(new() 
            { 
                State = Microsoft.Playwright.WaitForSelectorState.Visible, 
                Timeout = 15000 
            });

            // Additional wait for data to fully render
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        [When("I navigate to join page with invitation code")]
        public async Task WhenINavigateToJoinPageWithInvitationCode()
        {
            var response = scenarioContext.Get<RoomCreationResponse>("RoomCreationResponse");
            var baseUrl = scenarioContext.Get<string>("baseUrl");
            var expectedUrl = $"{baseUrl}/join/{response.Room.InvitationCode}";

            // Navigate to join page and wait for page to load
            await page.GotoAsync(expectedUrl, new() 
            { 
                WaitUntil = WaitUntilState.Load,
                Timeout = 30000
            });
            
            // Wait for navigation to complete and verify we're on the correct URL
            // The welcomeGuard may redirect to /home if room is not available
            try
            {
                await page.WaitForURLAsync($"**/join/{response.Room.InvitationCode}", new() { Timeout = 15000 });
            }
            catch
            {
                // Check current URL - if redirected, guard may have failed
                var currentUrl = page.Url;
                if (currentUrl.Contains("/home") || currentUrl.Contains("/not-found"))
                {
                    throw new InvalidOperationException(
                        $"Navigation redirected to {currentUrl}. The room '{response.Room.InvitationCode}' might not be available. " +
                        $"This could happen if:\n" +
                        $"1. The room was not found in the database\n" +
                        $"2. The room is closed or full\n" +
                        $"3. There was an API error when loading the room");
                }
                
                // If URL doesn't match but also doesn't contain /home, continue
                // Maybe we're on a different path or URL format is different
            }
            
            // Wait for Angular app to initialize and API calls to complete
            // The welcomeGuard makes an API call, so we need to wait for it
            try
            {
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 20000 });
            }
            catch
            {
                // NetworkIdle might timeout if there are long-running requests
                // Try DOMContentLoaded as fallback
                try
                {
                    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new() { Timeout = 5000 });
                }
                catch
                {
                    // Continue anyway - we'll wait for specific elements below
                }
            }
            
            // Small delay to allow Angular to bootstrap and start rendering
            await page.WaitForTimeoutAsync(1500);
            
            // Wait for page-layout component to appear (indicates Angular component loaded)
            var pageLayoutLocator = page.Locator("css=app-page-layout, .page-layout");
            try
            {
                await pageLayoutLocator.WaitForAsync(new() 
                { 
                    State = Microsoft.Playwright.WaitForSelectorState.Visible, 
                    Timeout = 20000 
                });
            }
            catch
            {
                // If page-layout doesn't appear, check if we're on an error page or home page
                var currentUrl = page.Url;
                var bodyText = await page.Locator("body").TextContentAsync() ?? "";
                
                if (currentUrl.Contains("/home") || currentUrl.Contains("/not-found"))
                {
                    throw new InvalidOperationException(
                        $"Navigation redirected to {currentUrl}. The room '{response.Room.InvitationCode}' might not be available. " +
                        $"Room was just created, but may not be accessible yet.");
                }
                
                // Check for error messages
                var errorIndicators = page.Locator("xpath=.//*[contains(text(), 'error')] | .//*[contains(text(), 'Error')] | .//*[contains(text(), 'not available')] | .//*[contains(text(), 'unavailable')]");
                if (await errorIndicators.CountAsync() > 0)
                {
                    var errorText = await errorIndicators.First.TextContentAsync();
                    throw new InvalidOperationException($"Error detected on page: {errorText}");
                }
                
                throw new InvalidOperationException(
                    $"Page layout not found. Current URL: {currentUrl}. Page may not have loaded correctly.");
            }
            
            // Wait for room data cards to appear FIRST (these indicate API data is loaded)
            // This is more reliable than waiting for title, as cards appear after data is loaded
            var roomDataCard = page.Locator("xpath=.//*[contains(text(), 'Exchange Date')] | .//*[contains(text(), 'Gift Budget')] | .//*[contains(@class,'room-data-card')]");
            try
            {
                await roomDataCard.First.WaitForAsync(new() 
                { 
                    State = Microsoft.Playwright.WaitForSelectorState.Visible, 
                    Timeout = 20000 
                });
            }
            catch
            {
                // If data cards don't appear, the API call in guard might have failed
                throw new InvalidOperationException(
                    $"Room data cards not found. The room data may not have loaded from API. " +
                    $"Room code: {response.Room.InvitationCode}");
            }
            
            // Now wait for page title (h1.page-layout__title) to appear with content
            // Title format: "Welcome to the {roomName}!"
            var titleLocator = page.Locator("css=h1.page-layout__title");
            
            // Wait for title to be visible AND have text content
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    await titleLocator.WaitForAsync(new() 
                    { 
                        State = Microsoft.Playwright.WaitForSelectorState.Visible, 
                        Timeout = 3000 
                    });
                    
                    var titleText = await titleLocator.TextContentAsync();
                    if (!string.IsNullOrWhiteSpace(titleText) && 
                        (titleText.Contains("Welcome", StringComparison.OrdinalIgnoreCase) || 
                         titleText.Contains(response.Room.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        break; // Title found with content
                    }
                }
                catch
                {
                    if (attempt == 4)
                        throw; // Last attempt failed
                }
                
                await page.WaitForTimeoutAsync(500);
            }
            
            // Finally, wait for "Join the Room" button to be visible
            // Button text is inside <span class="btn__text">, so we need to find button containing this text
            var joinButton = page.Locator("xpath=.//button[.//span[contains(text(), 'Join the Room')]] | .//button[contains(., 'Join the Room')]");
            await joinButton.WaitForAsync(new() 
            { 
                State = Microsoft.Playwright.WaitForSelectorState.Visible, 
                Timeout = 20000 
            });
            
            // Final wait to ensure all data is fully rendered
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        [When("I refresh the page")]
        public async Task WhenIRefreshThePage()
        {
            await page.ReloadAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }
}
