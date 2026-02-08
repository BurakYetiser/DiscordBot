using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DiscordBot.Core;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<BotService>();
    })
    .Build();

await host.Services.GetRequiredService<BotService>().StartAsync();
await Task.Delay(-1);
