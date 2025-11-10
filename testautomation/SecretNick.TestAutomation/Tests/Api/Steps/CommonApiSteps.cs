using Reqnroll;
using Shouldly;
using Tests.Api.Clients;
using Tests.Helpers;

namespace Tests.Api.Steps
{
    [Binding]
    public class CommonApiSteps(ScenarioContext scenarioContext, SystemApiClient systemApiClient)
    {
        private readonly ScenarioContext _scenarioContext = scenarioContext;
        private readonly SystemApiClient _systemApiClient = systemApiClient;

        [Given("the API is available")]
        public async Task GivenTheAPIIsAvailable()
        {
            try
            {
                var response = await _systemApiClient.GetSystemInfoAsync();
                response.ShouldNotBeNull();
                response.Environment.ShouldNotBeNullOrEmpty();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"API is not available. Please ensure the backend service is running at {_systemApiClient.GetType().Name}. " +
                    $"Original error: {ex.Message}", ex);
            }
        }

        [Then("the request should return status {int}")]
        public void ThenTheRequestShouldPassOrFailWithStatus(int expectedStatus)
        {
            var actualStatus = _scenarioContext.Get<int>("LastStatusCode");
            actualStatus.ShouldBe(expectedStatus);
        }

        [Then("the error should mention {string}")]
        public void ThenTheErrorShouldMention(string expectedError)
        {
            var errorBody = _scenarioContext.Get<string>("LastErrorBody");

            var (hasValidationErrors, errors) = ErrorResponseParser.ParseValidationErrors(errorBody);

            if (hasValidationErrors && errors != null)
            {
                // Шукаємо в ключах errors
                var found = errors.Keys.Any(k =>
                    k.Contains(expectedError, StringComparison.OrdinalIgnoreCase)) ||
                    errors.Values.Any(messages =>
                        messages.Any(m => m.Contains(expectedError, StringComparison.OrdinalIgnoreCase)));

                found.ShouldBeTrue(
                    $"Expected '{expectedError}' in validation errors. " +
                    $"Actual errors: {string.Join(", ", errors.Keys)}. " +
                    $"Full response: {errorBody}");
            }
            else
            {
                errorBody.ShouldContain(expectedError, Case.Insensitive,
                    $"Expected '{expectedError}' in error response: {errorBody}");
            }
        }

        [Then("the error should mention field {string}")]
        public void ThenTheErrorShouldMentionField(string fieldName)
        {
            var errorBody = _scenarioContext.Get<string>("LastErrorBody");

            ErrorResponseParser.ContainsFieldError(errorBody, fieldName)
                .ShouldBeTrue(
                    $"Expected validation error for field '{fieldName}'. " +
                    $"Response: {errorBody}");
        }

        [Then("Creation date within last hour")]
        public void ThenCreationDateWithinLastHour()
        {
            var entity = _scenarioContext.Get<dynamic>("LastEntity");
            var now = DateTime.UtcNow;
            ((DateTime)entity.CreatedOn).ShouldBeInRange(now.AddHours(-1), now.AddMinutes(1));
        }

        [Then("Modification date within last hour")]
        public void ThenModificationDateWithinLastHour()
        {
            var entity = _scenarioContext.Get<dynamic>("LastEntity");
            var now = DateTime.UtcNow;
            ((DateTime)entity.ModifiedOn).ShouldBeInRange(now.AddHours(-1), now.AddMinutes(1));
        }
    }
}
