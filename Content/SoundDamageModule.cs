using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerraVoice.Core;
using TerraVoice.Misc;

namespace TerraVoice.Content;

public class SoundDamageModule
{
    private class TheModPlayer : ModPlayer
    {
        public override void PostUpdateEquips() {
            var speaker = PlayVoiceSystem.PlayerSpeakers[Player.whoAmI];
            if (speaker is null) return;

            float volume = Player.whoAmI == Main.myPlayer
                ? DrawingSystem.CurrentDisplayedVolume
                : speaker.CurrentDisplayedVolume;
            float db = Helper.GetDecibel(volume);
            float factor = Helper.DamageMultiplierCurve(db);
            Player.GetDamage(DamageClass.Generic) *= MathHelper.Lerp(VoiceConfig.Instance.DmgMultiMinimum, VoiceConfig.Instance.DmgMultiMaximum,
                factor);
        }
    }
}