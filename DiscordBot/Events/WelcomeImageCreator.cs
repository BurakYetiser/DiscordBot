using System.Net.Http;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DiscordBot.Events;

public static class WelcomeImageGenerator
{
    private static readonly HttpClient http = new HttpClient();

    public static async Task<string> CreateWelcomeImageAsync(string username, string avatarUrl, int memberCount)
    {
        string bgPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Dual monitor wallpaper red - Imgur.png");
        string outPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"welcome_{Guid.NewGuid()}.png");

        using Image<Rgba32> background = Image.Load<Rgba32>(bgPath);

        // Avatar indir
        byte[] avatarBytes = await http.GetByteArrayAsync(avatarUrl);
        using Image<Rgba32> avatar = Image.Load<Rgba32>(avatarBytes);

        // Avatar
        int avatarSize = 460;
        avatar.Mutate(x => x.Resize(avatarSize, avatarSize));
        using Image<Rgba32> circleAvatar = MakeCircular(avatar);

        // Font ailesi
        var family = SystemFonts.Collection.Families.FirstOrDefault(f => f.Name == "Arial");
        if (family == default)
            family = SystemFonts.Collection.Families.First();

        // fontlar
        float nameFontSize = 160;
        var subFont = family.CreateFont(80, FontStyle.Regular);
        var memberFont = family.CreateFont(68, FontStyle.Bold);

        string nameText = username;
        string subText = "SWTM Topluluğuna Hoşgeldin";
        string memberText = $"Member {memberCount}";

        // Satır boşlukları
        int gapAfterAvatar = 55; // avatar alt boşluk
        int gapNameToSub = 85;   // username -> welcome arası
        int gapSubToMember = 60; // welcome -> member arası

        background.Mutate(ctx =>
        {
            // koyu overlay
            ctx.Fill(new Color(new Rgba32(0, 0, 0, 130)));

            float centerX = background.Width / 2f;

            // Avatar ortada (üstte ama dev)
            float avatarX = centerX - (avatarSize / 2f);
            float avatarY = background.Height * 0.06f;

            ctx.DrawImage(circleAvatar, new Point((int)avatarX, (int)avatarY), 1f);

            // Username fontunu otomatik küçült (çok uzun isimlerde taşma olmasın)
            Font nameFont = family.CreateFont(nameFontSize, FontStyle.Bold);
            var nameSize = TextMeasurer.MeasureSize(nameText, new TextOptions(nameFont));

            float maxTextWidth = background.Width * 0.90f; // ekranın %90'ını geçmesin

            while (nameSize.Width > maxTextWidth && nameFontSize > 80)
            {
                nameFontSize -= 10;
                nameFont = family.CreateFont(nameFontSize, FontStyle.Bold);
                nameSize = TextMeasurer.MeasureSize(nameText, new TextOptions(nameFont));
            }

            // Yazıların başlayacağı Y
            float textStartY = avatarY + avatarSize + gapAfterAvatar;

            // Username ortala
            float nameX = centerX - (nameSize.Width / 2f);
            ctx.DrawText(nameText, nameFont, Color.White, new PointF(nameX, textStartY));

            // Welcome text
            float subY = textStartY + nameSize.Height + gapNameToSub;
            var subSize = TextMeasurer.MeasureSize(subText, new TextOptions(subFont));
            float subX = centerX - (subSize.Width / 2f);
            ctx.DrawText(subText, subFont, Color.White, new PointF(subX, subY));

            // Member text
            float memberY = subY + subSize.Height + gapSubToMember;
            var memberSize = TextMeasurer.MeasureSize(memberText, new TextOptions(memberFont));
            float memberX = centerX - (memberSize.Width / 2f);
            ctx.DrawText(memberText, memberFont, Color.White, new PointF(memberX, memberY));
        });

        await background.SaveAsPngAsync(outPath);
        return outPath;
    }

    private static Image<Rgba32> MakeCircular(Image<Rgba32> input)
    {
        int size = Math.Min(input.Width, input.Height);
        var output = new Image<Rgba32>(size, size);

        int cx = size / 2;
        int cy = size / 2;
        int r = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = x - cx;
                int dy = y - cy;

                output[x, y] = (dx * dx + dy * dy <= r * r)
                    ? input[x, y]
                    : new Rgba32(0, 0, 0, 0);
            }
        }

        return output;
    }
}
