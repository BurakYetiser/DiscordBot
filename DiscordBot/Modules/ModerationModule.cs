using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace SWTMDCBot.Modules;

public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IConfiguration _config;

    public ModerationModule(IConfiguration config)
    {
        _config = config;
    }

    [SlashCommand("yasakla", "Bir kullanıcıyı sunucudan yasaklar.")]
    public async Task Yasakla(
        [Summary("kullanici", "Yasaklanacak kişi")] SocketGuildUser kullanici,
        [Summary("sebep", "Sebep (opsiyonel)")] string? sebep = null)
    {
        sebep ??= "Sebep belirtilmedi.";

        if (!(Context.User as SocketGuildUser)!.GuildPermissions.BanMembers)
        {
            await RespondAsync("❌ Bu komutu kullanmak için **Üyeleri Yasakla** yetkin olmalı.", ephemeral: true);
            return;
        }

        if (!Context.Guild.CurrentUser.GuildPermissions.BanMembers)
        {
            await RespondAsync("❌ Benim **Üyeleri Yasakla** yetkim yok.", ephemeral: true);
            return;
        }

        if (kullanici.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
        {
            await RespondAsync("❌ Bu kullanıcıyı banlayamam (rol hiyerarşisi).", ephemeral: true);
            return;
        }

        await kullanici.BanAsync(reason: sebep);

        var publicEmbed = new EmbedBuilder()
            .WithTitle("✅ Kullanıcı Yasaklandı")
            .WithDescription($"**{kullanici.Mention}** sunucudan yasaklandı.")
            .WithColor(Color.Red)
            .AddField("Sebep", sebep, inline: false)
            .Build();

        await RespondAsync(embed: publicEmbed);

        string logChannelIdStr = _config["LogChannelId"] ?? "";
        if (ulong.TryParse(logChannelIdStr, out ulong logChannelId))
        {
            var logChannel = Context.Guild.GetTextChannel(logChannelId);
            if (logChannel != null)
            {
                var logEmbed = new EmbedBuilder()
                    .WithTitle("🚫 BAN LOG")
                    .WithColor(Color.DarkRed)
                    .AddField("Yasaklanan", $"{kullanici.Username} ({kullanici.Id})", false)
                    .AddField("Yasaklayan", $"{Context.User.Username} ({Context.User.Id})", false)
                    .AddField("Kanal", $"{Context.Channel.Name} ({Context.Channel.Id})", false)
                    .AddField("Sebep", sebep, false)
                    .WithCurrentTimestamp()
                    .Build();

                await logChannel.SendMessageAsync(embed: logEmbed);
            }
        }
    }
}
