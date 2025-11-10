using Microsoft.Playwright;
using Reqnroll;
using Shouldly;
using Tests.Api.Models.Responses;

namespace Tests.Ui.Steps
{
    [Binding]
    public class VerificationSteps(ScenarioContext scenarioContext,
            IPage page) : UiStepsBase(page)
    {
        [Then("I should see heading {string}")]
        public async Task ThenIShouldSeeHeading(string expectedHeading)
        {
            var isVisible = await GetBasePage().IsHeadingVisibleAsync(expectedHeading);
            isVisible.ShouldBeTrue($"Heading '{expectedHeading}' should be visible");
        }

        [Then("I should see {string} text")]
        public async Task ThenIShouldSeeText(string text)
        {
            var isVisible = await GetBasePage().IsTextVisibleAsync(text);
            isVisible.ShouldBeTrue($"Text '{text}' should be visible");
        }

        [Then("I should see {string} button")]
        public async Task ThenIShouldSeeButton(string buttonText)
        {
            var isVisible = await GetBasePage().IsButtonVisibleAsync(buttonText);
            isVisible.ShouldBeTrue($"Button '{buttonText}' should be visible");
        }

        [Then("I should see {string} button disabled")]
        public async Task ThenIShouldSeeButtonDisabled(string buttonText)
        {
            var isDisabled = await GetBasePage().IsButtonDisabledAsync(buttonText);
            isDisabled.ShouldBeTrue($"Button '{buttonText}' should be disabled");
        }

        [Then("I should see {string} button enabled")]
        public async Task ThenIShouldSeeButtonEnabled(string buttonText)
        {
            var isEnabled = await GetBasePage().IsButtonEnabledAsync(buttonText);
            isEnabled.ShouldBeTrue($"Button '{buttonText}' should be enabled");
        }

        [Then("I should see room success with name {string}")]
        public async Task ThenIShouldSeeRoomSuccessPageWithName(string roomName)
        {
            // Wait a bit for page to fully render after navigation
            await page.WaitForTimeoutAsync(1000);
            
            var isVisible = await GetRoomSuccessPage().IsSuccessTitleVisibleAsync(roomName);
            isVisible.ShouldBeTrue($"Success title with room name '{roomName}' should be visible (timeout: 5s)");

            if (scenarioContext.ContainsKey("RoomCreationResponse"))
            {
                var roomResponse = scenarioContext.Get<RoomCreationResponse>("RoomCreationResponse");
                scenarioContext.Set(roomResponse.Room.InvitationCode, "RoomLink");
                scenarioContext.Set(roomResponse.UserCode, "AdminLink");
            }
        }

        [Then("I should see room success page")]
        public async Task ThenIShouldSeeRoomSuccessPage()
        {
            // Wait a bit for page to fully render after navigation
            await page.WaitForTimeoutAsync(1000);
            
            var isOnSuccessPage = await GetRoomSuccessPage().IsOnSuccessPageAsync();
            isOnSuccessPage.ShouldBeTrue("Should be on room success page (timeout: 10s)");
        }

        [Then("I should see room link")]
        public async Task ThenIShouldSeeRoomLink()
        {
            var roomLink = await GetRoomSuccessPage().GetRoomLinkAsync();
            roomLink.ShouldNotBeNullOrWhiteSpace("Room link should be present");

            scenarioContext.Set(roomLink, "RoomLink");
        }

        [Then("I should see personal link")]
        public async Task ThenIShouldSeePersonalLink()
        {
            string personalLink = await GetSuccessPage().GetPersonalLinkAsync();

            personalLink.ShouldNotBeNullOrWhiteSpace("Personal link should be present");
        }

        [Then("I should see invitation text:")]
        public async Task ThenIShouldSeeInvitationText(string expectedText)
        {
            var actualText = await GetSuccessPage().ClickOnCopyAndGetClipboardText("Invitation Note");

            var normalizedExpected = expectedText.Trim();
            var roomLink = "";

            if (scenarioContext.ContainsKey("RoomLink"))
            {
                roomLink = scenarioContext.Get<string>("RoomLink");
                normalizedExpected = normalizedExpected.Replace("{room_link}", roomLink);
            }

            // Normalize both texts: remove all line breaks and extra spaces
            var normalizedActual = NormalizeWhitespace(actualText);
            normalizedExpected = NormalizeWhitespace(normalizedExpected);

            // Verify the normalized texts match
            normalizedActual.ShouldBe(normalizedExpected);

            // Additionally verify the room link is present if we have one
            if (!string.IsNullOrEmpty(roomLink))
            {
                actualText.Contains(roomLink).ShouldBeTrue("Invitation should contain the room link");
            }
        }

        private static string NormalizeWhitespace(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Replace all types of line breaks with a single space
            text = text.Replace("\r\n", " ")
                       .Replace("\r", " ")
                       .Replace("\n", " ");

            // Replace multiple spaces with single space
            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            return text.Trim();
        }

        [Then("I should see room name {string}")]
        public async Task ThenIShouldSeeRoomName(string expectedName)
        {
            var actualName = await GetRoomPage().GetRoomNameAsync();
            actualName.ShouldBe(expectedName);
        }

        [Then("I should see room name from API")]
        public async Task ThenIShouldSeeRoomNameFromAPI()
        {
            var roomResponse = scenarioContext.Get<RoomCreationResponse>("RoomResponse");
            var actualName = await GetRoomPage().GetRoomNameAsync();
            actualName.ShouldBe(roomResponse.Room.Name);
        }

        [Then("I should see exchange date as today")]
        public async Task ThenIShouldSeeExchangeDateAsToday()
        {
            var (isTrue, message) = await GetRoomPage().IsExchangeDateTodayAsync();
            isTrue.ShouldBeTrue(message);
        }

        [Then("I should see exchange date as in request")]
        public async Task ThenIShouldSeeExchangeDateAsInRequest()
        {
            var roomResponse = scenarioContext.Get<RoomCreationResponse>("RoomResponse");
            var (isTrue, message) = await GetRoomPage().IsExchangeDateAsync(roomResponse.Room.GiftExchangeDate);
            isTrue.ShouldBeTrue(message);
        }

        [Then("I should see gift budget {string}")]
        public async Task ThenIShouldSeeGiftBudget(string expectedBudget)
        {
            var actualBudget = await GetRoomPage().GetGiftBudgetAsync();
            actualBudget.ShouldBe(expectedBudget);
        }

        [Then("I should see participants count {int}")]
        public async Task ThenIShouldSeeParticipantsCount(int expectedCount)
        {
            var actualCount = await GetRoomPage().GetParticipantsCountAsync();
            actualCount.ShouldBe(expectedCount);
        }

        [Then("I should see minimum users warning")]
        public async Task ThenIShouldSeeMinimumUsersWarning()
        {
            var isVisible = await GetRoomPage().IsMinimumPeopleWarningVisibleAsync();
            isVisible.ShouldBeTrue("Minimum people warning should be visible");
        }

        [Then("I should not see minimum users warning")]
        public async Task ThenIShouldNotSeeMinimumUsersWarning()
        {
            var isVisible = await GetRoomPage().IsMinimumPeopleWarningVisibleAsync();
            isVisible.ShouldBeFalse("Minimum people warning should not be visible");
        }

        [Then("I should see draw reminder")]
        public async Task ThenIShouldSeeDrawReminder()
        {
            var isVisible = await GetRoomPage().IsDrawReminderVisibleAsync();
            isVisible.ShouldBeTrue("Draw reminder should be visible");
        }

        [Then("I should see recipient name")]
        public async Task ThenIShouldSeeRecipientName()
        {
            var recipientName = await GetRoomPage().GetGifteeNameAsync();
            recipientName.ShouldNotBeNullOrWhiteSpace("Recipient name should be visible");
            scenarioContext.Set(recipientName, "RecipientName");
        }

        [Then("I should see recipient details in modal")]
        [Then("I should see recipient personal info")]
        public async Task ThenIShouldSeeRecipientPersonalInfo()
        {
            var personalInfo = await GetRoomPage().GetPersonalInfoAsync();
            personalInfo.ShouldNotBeNullOrWhiteSpace("Personal info should be visible");
        }

        [Then("I should see recipient wishes or interests")]
        public async Task ThenIShouldSeeRecipientWishesOrInterests()
        {
            var wishesOrInterests = await GetRoomPage().GetWishlistOrInterestsAsync();
            wishesOrInterests.ShouldNotBeNullOrWhiteSpace("Wishes or interests should be visible");
        }

        [Then("I should see welcome heading with room name")]
        public async Task ThenIShouldSeeWelcomeHeadingWithRoomName()
        {
            var roomResponse = scenarioContext.Get<RoomCreationResponse>("RoomResponse");
            var isVisible = await GetJoinPage().IsWelcomeTitleVisibleAsync(roomResponse.Room.Name);
            isVisible.ShouldBeTrue($"Welcome title with room name '{roomResponse.Room.Name}' should be visible");
        }

        [Then("I should see room exchange date")]
        public async Task ThenIShouldSeeRoomExchangeDate()
        {
            var dateText = await GetJoinPage().GetExchangeDateAsync();
            dateText.ShouldNotBeNullOrWhiteSpace("Exchange date should be visible");
        }

        [Then("I should see room gift budget {string}")]
        public async Task ThenIShouldSeeRoomGiftBudget(string expectedBudget)
        {
            var actualBudget = await GetJoinPage().GetGiftBudgetAsync();
            actualBudget.ShouldBe(expectedBudget);
        }

        [Then("I should see room gift budget as in request")]
        public async Task ThenIShouldSeeRoomGiftBudgetAsInRequest()
        {
            var roomResponse = scenarioContext.Get<RoomCreationResponse>("RoomResponse");
            var expectedBudget = roomResponse.Room.GiftMaximumBudget == 0 ? "Unlimited" : $"{roomResponse.Room.GiftMaximumBudget} UAH";
            await ThenIShouldSeeRoomGiftBudget(expectedBudget);
        }

        [Then("I should see copy links for all participants")]
        public async Task ThenIShouldSeeCopyLinksForAllParticipants()
        {
            var participants = scenarioContext.Get<List<string>>("ParticipantNames");

            foreach (var participant in participants)
            {
                var isVisible = await GetRoomPage().IsCopyLinkVisibleForParticipantAsync(participant);
                isVisible.ShouldBeTrue($"Copy link should be visible for participant {participant}");
            }
        }

        [Then("I should see budget text {string}")]
        public async Task ThenIShouldSeeBudgetText(string expectedText)
        {
            var actualText = await GetCreateRoomPage().GetBudgetTextAsync();
            actualText.ShouldBe(expectedText);
        }

        [Then("I should see {string} in header")]
        public async Task ThenIShouldSeeInHeader(string expectedText)
        {
            var headerText = await GetBasePage().GetHeaderTextAsync();
            headerText.ShouldContain(expectedText);
        }

        [Then("I should see error page")]
        public async Task ThenIShouldSeeErrorPage()
        {
            var isErrorVisible = await GetInvalidPage().IsErrorPageVisibleAsync();
            isErrorVisible.ShouldBeTrue("Error page should be visible");
        }

        [Then("I should see {string} button {string}")]
        public async Task ThenIShouldSeeButtonWithState(string buttonText, string state)
        {
            if (state == "enabled")
            {
                var isEnabled = await GetBasePage().IsButtonEnabledAsync(buttonText);
                isEnabled.ShouldBeTrue($"Button '{buttonText}' should be enabled");
            }
            else if (state == "disabled")
            {
                var isDisabled = await GetBasePage().IsButtonDisabledAsync(buttonText);
                isDisabled.ShouldBeTrue($"Button '{buttonText}' should be disabled");
            }
        }
    }
}
