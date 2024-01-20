using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TerraVoice.Misc;

namespace TerraVoice.Content;

public class VolumeInfoDisplay : InfoDisplay
{
    public override bool Active() => true;

    public override string DisplayValue(ref Color displayColor, ref Color displayShadowColor) {
        float volume = DrawingSystem.CurrentDisplayedVolume;
        float db = MathHelper.Clamp(Helper.GetDecibel(volume), 0f, 120f);
        string value = $"{db:N0} dB";
        if (VoiceConfig.Instance.DamageAffectedBySound) {
            float factor = Helper.DamageMultiplierCurve(db);
            float damageMultiplier = MathHelper.Lerp(
                VoiceConfig.Instance.DmgMultiMinimum,
                VoiceConfig.Instance.DmgMultiMaximum,
                factor);
            value += $"（{damageMultiplier:f2}x 伤害）";
        }

        return value;
    }
}