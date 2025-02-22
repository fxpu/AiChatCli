using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FxPu.LlmClient.Pp.IntegrationTests.Utils
{
    public class TestApp : IAsyncDisposable, ITestOutputHelperAccessor
    {
        private bool _isDisposed;
        private readonly ServiceProvider _serviceProvider;

        public TestApp()
        {
            // setup configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets(typeof(TestApp).Assembly, true)
                .Build();

            // service provider
            var services = new ServiceCollection();

            services.AddLogging(cfg =>
           {
               cfg.AddConfiguration(Configuration.GetSection("Logging"));
               cfg.AddXUnit(this);
           });
            services.AddSingleton(Configuration);

            _serviceProvider = services.BuildServiceProvider();
        }


        public IServiceProvider ServiceProvider { get => _serviceProvider; }
        public IConfiguration Configuration { get; }
        public ITestOutputHelper? OutputHelper { get; set; }

        public async ValueTask DisposeAsync()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                await _serviceProvider.DisposeAsync();
            }
        }

    }
}
