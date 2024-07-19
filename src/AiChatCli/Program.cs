using System.Diagnostics;
using FxPu.AiChatCli.Utils;
using FxPu.AiChatLib.Services;
using FxPu.AiChatLib.Utils;
using FxPu.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FxPu.AiChatCli
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "AiChatCli";
            try
            {
                // locate the .AiChat folder in the home directory and create AiChat.json config file if not exists
                var aiChatCliUserDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".AiChat");
                if (!Directory.Exists(aiChatCliUserDirectory))
                {
                    Directory.CreateDirectory(aiChatCliUserDirectory);
                }
                var settingsFile = Path.Combine(aiChatCliUserDirectory, "AiChat.json");
                if (!File.Exists(settingsFile))
                {
                    await File.WriteAllTextAsync(settingsFile, JsonHelper.ToJson(ChatOptions.SAMPLE_OPTIONS));
                    Console.WriteLine($"Created sample settings file {settingsFile}, please enter api keys and restart.");
                    Environment.ExitCode = 1;
                    return;
                }

                //  normal program starting ...
                var configuration = new ConfigurationBuilder()
                        .SetBasePath(aiChatCliUserDirectory)
                        .AddJsonFile("AiChat.json", true)
                        .Build();

                // service provider
                var services = new ServiceCollection();
                services.AddLogging(cfg =>
                {
                    cfg.ClearProviders();
                    cfg.AddConfiguration(configuration.GetSection("Logging"));
                    cfg.AddDebug();
                });

                // register services
                services.Configure<ChatOptions>(configuration);
                services.AddScoped<IChatService, ChatService>();
                services.AddScoped<ICommandProcessor, CommandProcessor>();
                services.AddScoped<Commands>();

                await using var serviceProvider = services.BuildServiceProvider();
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                var commandProcessor = serviceProvider.GetRequiredService<ICommandProcessor>();
                try
                {
                    logger.LogTrace("Start");
                        await commandProcessor.RunAsync();
                    logger.LogDebug("End");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                    logger.LogError(e, "Error in program");
                    Environment.ExitCode = 1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Debug.WriteLine(e.ToString());
                Environment.ExitCode = 1;
            }
        }

    }
}