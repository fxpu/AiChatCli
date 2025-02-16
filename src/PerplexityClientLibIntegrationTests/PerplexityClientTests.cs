using FxPu.Perplexity.Client.IntegrationTests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace FxPu.Perplexity.Client.IntegrationTests
{
    [Collection("Sequential")]
    public class PerplexityClientTests : IClassFixture<TestApp>, IAsyncLifetime
    {
        private readonly TestApp _app;

        public PerplexityClientTests(TestApp app, ITestOutputHelper outputHelper)
        {
            _app = app;
            _app.OutputHelper = outputHelper;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;



    }
}