using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Events;
namespace DiscordBot.Core
{
    public class BotService
    {
        private readonly IConfiguration _config;

        private DiscordSocketClient _client = null!;
        private InteractionService _interactions = null!;
        private IServiceProvider _services = null!;

        public BotService(IConfiguration config)
        {
            _config = config;
        }
        public async Task StartAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers
            });

            _interactions = new InteractionService(_client.Rest);

            _services = new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(_client)
                .AddSingleton(_interactions)
                .BuildServiceProvider();

            _client.Log += LogAsync;
            _interactions.Log += LogAsync;

            _client.Ready += OnReadyAsync;
            _client.InteractionCreated += OnInteractionAsync;

            _client.UserJoined += (user) => MemberJoinEvent.OnUserJoinedAsync(user, _config);

            string token = _config["Token"]!;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        private async Task OnReadyAsync()
        {
            Console.WriteLine("✅ Ready!");

            await _interactions.AddModulesAsync(typeof(BotService).Assembly, _services);

            ulong guildId = ulong.Parse(_config["GuildId"]!);
            await _interactions.RegisterCommandsToGuildAsync(guildId);

            Console.WriteLine("✅ Slash komutlar yüklendi.");
        }

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, interaction);
                await _interactions.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Interaction Error: " + ex.Message);

                try
                {
                    if (interaction.Type == InteractionType.ApplicationCommand)
                        await interaction.RespondAsync("❌ Komut çalıştırılamadı.", ephemeral: true);
                }
                catch { }
            }
        }

        private Task LogAsync(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }

}
