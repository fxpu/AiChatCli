using FxPu.LlmClient.OpenAi.IntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace FxPu.LlmClient.OpenAi.IntegrationTests
{
    [Collection("Sequential")]
    public class OpenAiClientTests : IClassFixture<TestApp>, IAsyncLifetime
    {
        private readonly TestApp _app;

        public OpenAiClientTests(TestApp app, ITestOutputHelper outputHelper)
        {
            _app = app;
            _app.OutputHelper = outputHelper;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact(DisplayName = "Test PpClient")]
        public async Task TestPpClientAsync()
        {
            // create pp client
            var optionsFactory = new OptionsWrapper<LlmClientOptions>(new LlmClientOptions
            {
                ModelName = "gpt-4o-mini",
                ApiKey = _app.Configuration["ApiKey"]
            });
            var loggerFactory = _app.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var openAiClient = new OpenAiClient(loggerFactory.CreateLogger<OpenAiClient>(), optionsFactory);

            var messages = new List<LlmChatMessage>();
            messages.Add(new LlmChatMessage { Role = LlmChatRole.User, Content = "Wieviele Einwohner hat Paris" });

            var request = new LlmChatCompletionRequest
            {
                Messages = messages
            };
            var response = await openAiClient.GetChatCompletionAsync(request);

            messages.Add(response.Message);

            messages.Add(new LlmChatMessage { Role = LlmChatRole.User, Content = "Und wieviele Studenten gibt es" });

            request = new LlmChatCompletionRequest
            {
                Messages = messages
            };
            response = await openAiClient.GetChatCompletionAsync(request);

            var i = 0;

        }
    }
}