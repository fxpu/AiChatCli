using FxPu.LlmClient.Pp.IntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace FxPu.LlmClient.Pp.IntegrationTests
{
    [Collection("Sequential")]
    public class PpClientTests : IClassFixture<TestApp>, IAsyncLifetime
    {
        private readonly TestApp _app;

        public PpClientTests(TestApp app, ITestOutputHelper outputHelper)
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
            var apiKey = _app.Configuration["ApiKey"];
            var modelName = "sonar-pro";
            var loggerFactory = _app.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var ppClient = new PpClient(loggerFactory.CreateLogger<PpClient>(), apiKey);

            var messages = new List<LlmChatMessage>();
            messages.Add(new LlmChatMessage { Role = LlmChatRole.User, Content = "Wieviele Einwohner hat Paris"  });

            var request = new LlmChatCompletionRequest
            {
                ModelName = modelName,
                Messages = messages
            };
            var response = await ppClient.GetChatCompletionAsync(request);

            messages.Add(response.Message);

            messages.Add(new LlmChatMessage { Role = LlmChatRole.User, Content = "Und wieviele Studenten gibt es" });

            request = new LlmChatCompletionRequest
            {
                ModelName = modelName,
                Messages = messages
            };
            response = await ppClient.GetChatCompletionAsync(request);

            var i = 0;

        }
    }
}