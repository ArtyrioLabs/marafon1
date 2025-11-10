using Microsoft.Playwright;
using Tests.Helpers;

namespace Tests.Ui.Pages
{
    public class BasePage(IPage page)
    {
        protected internal readonly IPage Page = page;

        internal readonly Dictionary<string, string> fieldPlaceholders = new()
        {
            { "Room Name", "Enter your room name" },
            { "Message", "Enter your message here..." },
            { "Room Description", "Enter your message here..." },
            { "Budget", "Type in your budget" },
            { "Gift Budget", "Type in your budget" },
            { "First Name", "e.g. Nickolas" },
            { "Last Name", "e.g. Secret" },
            { "Phone", "777777777" },
            { "Email", "nickolas@example.com" },
            { "Address", "Where should St. Nick deliver your gift?" },
            { "Wish Name", "Enter your wish name" },
            { "Wish Link", "E.g. https://example.com/item" },
            { "Interests", "e.g. reading, coffee, cozy socks" }
        };

        internal readonly List<string> multipleFields =
        [
            "Wish Name",
            "Wish Link"
        ];

        protected internal async Task ClickButtonAsync(string buttonText)
        {
            // Button text can be directly in button or inside span.btn__text (Angular component)
            var locator = Page.Locator($"xpath=.//button[.='{buttonText}'] | .//button[.//span[.='{buttonText}']] | .//button[contains(., '{buttonText}')]");
            // Increased timeout for buttons that may take time to appear (e.g., after page load)
            await locator.ClickSafeAsync(15000);
        }

        public async Task ClickOutsideAsync()
        {
            await Page.Locator("body").ClickAsync(new LocatorClickOptions
            {
                Position = new Position { X = 10, Y = 10 }
            });
        }

        protected async Task FillByPlaceholderAsync(string placeholder, string value)
        {
            var locator = Page.Locator($"xpath=.//*[@placeholder='{placeholder}']");
            await locator.FillSafeAsync(value);
        }

        protected async Task FillLastFieldByPlaceholderAsync(string placeholder, string value)
        {
            var locator = Page.Locator($"xpath=(.//*[@placeholder='{placeholder}'])[last()]");
            await locator.FillSafeAsync(value);
        }

        protected async Task FillFieldByPlaceholderAndIndexAsync(string placeholder, string value, int index)
        {
            var locator = Page.Locator($"xpath=(.//*[@placeholder='{placeholder}'])[{index}]");
            await locator.FillSafeAsync(value);
        }

        protected async Task ClickByTextAsync(string text)
        {
            var locator = Page.Locator($"xpath=.//*[normalize-space()='{text}']");
            await locator.ClickSafeAsync();
        }

        protected async Task<string> GetTextAsync(string xpath)
        {
            var locator = Page.Locator($"xpath={xpath}");
            return await locator.GetTextSafeAsync();
        }

        protected async Task<bool> IsVisibleAsync(string xpath)
        {
            var locator = Page.Locator($"xpath={xpath}");
            return await locator.IsVisibleSafeAsync();
        }

        protected internal async Task SelectRadioButtonAsync(string labelText)
        {
            var locator = Page.Locator($"xpath=.//label[contains(@class,'radio')][normalize-space()='{labelText}']");
            await locator.ClickSafeAsync();
        }

        public async Task<string> ClickOnCopyAndGetClipboardText(string labelText)
        {
            await Page.Context.GrantPermissionsAsync(["clipboard-read", "clipboard-write"]);

            var copyButtonLocator = Page.Locator(
                $"xpath=(.//*[normalize-space()='{labelText}']/following::*[@class='copy-button'] | " +
                $".//*[normalize-space()='{labelText}']/following::*[@aria-label='Copy to clipboard'] | " +
                $".//*[normalize-space()='{labelText}']/following-sibling::*//*[@aria-label='Copy to clipboard'])[1]"
            );

            await copyButtonLocator.ClickSafeAsync();

            var clipboardText = await Page.EvaluateAsync<string>(@"
                    async () => {
                        try {
                            return await navigator.clipboard.readText();
                        } catch (e) {
                            console.error('Clipboard read failed:', e);
                            return '';
                        }
                    }
                ");

            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                throw new Exception($"Clipboard is empty after clicking copy button for '{labelText}'");
            }

            return clipboardText.Trim();
        }

        public async Task<bool> IsHeadingVisibleAsync(string expectedHeading)
        {
            var selectors = new[]
            {
                $"xpath=.//*[contains(@class,'_title')][normalize-space()='{expectedHeading}']",
                $"xpath=.//h1[normalize-space()='{expectedHeading}']",
                $"xpath=.//h2[normalize-space()='{expectedHeading}']",
                $"xpath=.//h3[normalize-space()='{expectedHeading}']"
            };

            foreach (var selector in selectors)
            {
                var locator = Page.Locator(selector);
                if (await locator.IsVisibleSafeAsync())
                    return true;
            }

            return false;
        }

        public async Task<bool> IsTextVisibleAsync(string text)
        {
            var locator = Page.Locator($"xpath=.//*[contains(text(),'{text}')]");
            return await locator.IsVisibleSafeAsync();
        }

        public async Task<bool> IsButtonVisibleAsync(string buttonText)
        {
            var locator = Page.Locator($"xpath=.//button[.='{buttonText}']");
            return await locator.IsVisibleSafeAsync();
        }

        public async Task<bool> IsButtonDisabledAsync(string buttonText)
        {
            return await Page.Locator($"xpath=.//button[.='{buttonText}']").IsDisabledAsync();
        }

        public async Task<bool> IsButtonEnabledAsync(string buttonText)
        {
            return await Page.Locator($"xpath=.//button[.='{buttonText}']").IsEnabledAsync();
        }

        public async Task<string> GetHeaderTextAsync()
        {
            return await Page.Locator("xpath=.//header").TextContentAsync() ?? String.Empty;
        }

        public virtual async Task FillFieldAsync(string fieldName, string value)
        {
            var placeholder = fieldPlaceholders.TryGetValue(fieldName, out string? newValue)
                ? newValue : fieldName;

            if (multipleFields.Contains(fieldName))
            {
                await FillLastFieldByPlaceholderAsync(placeholder, value);
            }
            else
            {
                await FillByPlaceholderAsync(placeholder, value);
            }
        }
    }
}
