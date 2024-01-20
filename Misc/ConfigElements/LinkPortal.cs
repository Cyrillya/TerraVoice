using Terraria;
using Terraria.Localization;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace TerraVoice.Misc.ConfigElements;

public class DecibelTest : LinkPortal
{
    public override string GetUrl() => "https://youlean.co/online-loudness-meter/";
}

public abstract class LinkPortal : ConfigElement
{
    public abstract string GetUrl();

    public override void LeftClick(UIMouseEvent evt) {
        base.LeftClick(evt);

        Utils.OpenToURL(GetUrl());
    }
}