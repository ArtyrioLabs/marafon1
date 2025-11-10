using Microsoft.Playwright;
using Tests.Helpers;

namespace Tests.Ui.Pages
{
    public class CreateRoomSuccessPage(IPage page) : BaseSuccessPage(page)
    {
        public override async Task<bool> IsOnSuccessPageAsync()
        {
            // Wait for URL to contain success path
            try
            {
                var url = Page.Url;
                if (!url.Contains("/create-room/success"))
                {
                    // Wait for navigation to success page
                    await Page.WaitForURLAsync("**/create-room/success", new() { Timeout = 10000 });
                }
            }
            catch
            {
                // URL check failed, continue with element checks
            }

            // Wait for page to load
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Check if ANY success indicator is present
            var titleVisible = await IsSuccessTitleVisibleAsync();
            if (titleVisible)
                return true;

            // Check for "Visit Your Room" button
            var buttonVisible = await base.IsOnSuccessPageAsync();
            if (buttonVisible)
                return true;

            // Check for "Your Room Link" section (specific to create room success)
            var roomLinkSection = Page.Locator("xpath=.//*[contains(text(), 'Your Room Link')]");
            return await roomLinkSection.IsVisibleSafeAsync(5000);
        }

        public async Task<bool> IsSuccessTitleVisibleAsync(string roomName)
        {
            // Title is rendered in h1 with class page-layout__title inside app-page-layout
            // Text can be: "Your {roomName} Room is Ready!"
            // Try multiple selectors with increased timeout
            var selectors = new[]
            {
                $"xpath=.//h1[contains(@class,'title')][contains(.,'{roomName}')][contains(.,'Room is Ready')]",
                $"xpath=.//h1[contains(@class,'_title')][contains(.,'{roomName}')]",
                $"xpath=.//*[contains(@class,'title')][contains(.,'{roomName}')][contains(.,'Room is Ready')]",
                $"xpath=.//h1[normalize-space()='Your {roomName} Room is Ready!']",
                $"xpath=.//h1[contains(text(), 'Your')][contains(text(), '{roomName}')][contains(text(), 'Room is Ready')]"
            };

            foreach (var selector in selectors)
            {
                var locator = Page.Locator(selector);
                if (await locator.IsVisibleSafeAsync(5000))
                    return true;
            }
            return false;
        }

        public async Task<bool> IsSuccessTitleVisibleAsync()
        {
            // Title contains "Room is Ready!"
            var selectors = new[]
            {
                "xpath=.//h1[contains(@class,'title')][contains(.,'Room is Ready')]",
                "xpath=.//h1[contains(@class,'_title')][contains(.,'Room is Ready')]",
                "xpath=.//*[contains(@class,'title')][contains(.,'Room is Ready')]",
                "xpath=.//h1[contains(text(), 'Room is Ready')]"
            };

            foreach (var selector in selectors)
            {
                var locator = Page.Locator(selector);
                if (await locator.IsVisibleSafeAsync(5000))
                    return true;
            }
            return false;
        }

        public async Task<string> GetRoomLinkAsync()
        {
            return await ClickOnCopyAndGetClipboardText("Your Room Link");
        }

        public async Task<string> GetInvitationNoteAsync()
        {
            return await ClickOnCopyAndGetClipboardText("Invitation Note");
        }
    }
}
