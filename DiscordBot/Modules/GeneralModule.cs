using Discord.Interactions;
using Discord.WebSocket;

namespace SWTMDCBot.Modules;

public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Bot çalışıyor mu test eder")]
    public async Task Ping()
    {
        if ((Context.User as SocketGuildUser)!.GuildPermissions.Administrator)
            await RespondAsync("🏓 Pong!");
    }
}
