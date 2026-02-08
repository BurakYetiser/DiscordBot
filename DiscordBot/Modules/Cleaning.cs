using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace SWTMDCBot.Modules;

public class Cleaning : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("temizle", "Bulunduğun kanalda istediğin kadar mesajı siler (1-100).")]
    public async Task Temizle(
        [Summary("adet", "Kaç mesaj silinsin? (1-100)")] int adet)
    {

        if (adet < 1 || adet > 100)
        {
            await RespondAsync("❌ Adet 1 ile 100 arasında olmalı.", ephemeral: true);
            return;
        }

        var user = Context.User as SocketGuildUser;
        if (user == null || !user.GuildPermissions.ManageMessages)
        {
            await RespondAsync("❌ Bu komutu kullanmak için **Mesajları Yönet** yetkisine sahip olmalısın.", ephemeral: true);
            return;
        }

        var bot = Context.Guild.CurrentUser;
        if (!bot.GuildPermissions.ManageMessages)
        {
            await RespondAsync("❌ Benim **Mesajları Yönet** yetkim yok. Bana yetki ver.", ephemeral: true);
            return;
        }

        if (Context.Channel is not SocketTextChannel textChannel)
        {
            await RespondAsync("❌ Bu komut sadece yazı kanallarında çalışır.", ephemeral: true);
            return;
        }

        await RespondAsync($"🧹 {adet} mesaj siliniyor...", ephemeral: true);

        
        var messages = await textChannel.GetMessagesAsync(limit: adet + 1).FlattenAsync();

        var deletable = messages
            .Where(m => (DateTimeOffset.UtcNow - m.Timestamp).TotalDays < 14)
            .ToList();

        if (deletable.Count == 0)
        {
            await FollowupAsync("⚠️ Silinecek uygun mesaj yok (14 günden eski olabilir).", ephemeral: true);
            return;
        }

        await textChannel.DeleteMessagesAsync(deletable);

        await FollowupAsync($"✅ {deletable.Count} mesaj silindi. (14 günden eski olanlar silinmez)", ephemeral: true);
    }
}
