using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Events
{
    public static class MemberJoinEvent
    {
        private const ulong WelcomeChannelId = 0; // Hoş geldin mesajlarının gönderileceği kanal ID'si

        public static async Task OnUserJoinedAsync(SocketGuildUser user, IConfiguration config)
        {
            Console.WriteLine("✅ MemberJoinEvent tetiklendi: " + user.Username);

            try
            {

                string roleIdStr = config["UnregisteredRoleId"] ?? "";

                if (!string.IsNullOrWhiteSpace(roleIdStr))
                {
                    ulong roleId = ulong.Parse(roleIdStr);
                    var role = user.Guild.GetRole(roleId);

                    if (role != null)
                    {
                        await user.AddRoleAsync(role);
                        Console.WriteLine($"✅ {user.Username} kullanıcısına Unregistered rolü verildi.");
                    }
                    else
                    {
                        Console.WriteLine("❌ Rol bulunamadı! UnregisteredRoleId yanlış olabilir.");
                    }
                }
                else
                {
                    Console.WriteLine("❌ UnregisteredRoleId config boş!");
                }

                var channel = user.Guild.GetTextChannel(WelcomeChannelId);

                if (channel == null)
                {
                    Console.WriteLine("❌ Welcome channel bulunamadı! Kanal ID yanlış olabilir.");
                    return;
                }

                var loadingMsg = await channel.SendMessageAsync($"👋 Hoş geldin {user.Mention}! Kart hazırlanıyor...");

                try
                {

                    string avatarUrl = user.GetAvatarUrl(size: 256) ?? user.GetDefaultAvatarUrl();
                    int memberCount = user.Guild.MemberCount;

                    string imagePath = await WelcomeImageGenerator.CreateWelcomeImageAsync(
                        user.Username,
                        avatarUrl,
                        memberCount
                    );

                    await channel.SendFileAsync(imagePath, $"🎧 Hoş geldin {user.Mention}!");

                    await loadingMsg.DeleteAsync();

                    System.IO.File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Welcome card hatası: " + ex.Message);

                    await loadingMsg.ModifyAsync(msg =>
                    {
                        msg.Content = $"❌ Hoş geldin {user.Mention}! Kart hazırlanamadı.";
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ MemberJoinEvent hatası: " + ex.Message);
            }
        }
    }
}
