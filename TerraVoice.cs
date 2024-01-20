using Terraria.ModLoader;
using TerraVoice.Misc;

namespace TerraVoice;

public partial class TerraVoice : Mod
{
    public static TerraVoice Instance;

    public override void Load() {
        Instance = this;
    }

    public override void Unload() {
        PersonalConfig.Instance = null;
    }
}