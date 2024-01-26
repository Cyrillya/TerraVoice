using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraVoice.Misc;

namespace TerraVoice.Content;

public class ExplodeModule
{
    private class ExplodeModPlayer : ModPlayer
    {
        private int _explodeDelay = 0;

        // 确保仅在客户端运行，在一端生成射弹然后同步到其他端
        public override void PostUpdate() {
            _explodeDelay--;
            if (Player.whoAmI != Main.myPlayer) return;
            if (!VoiceConfig.Instance.ExplodeMode) return;

            float volume = DrawingSystem.RealVolume;
            int decibel = (int) Helper.GetDecibel(volume);
            if (decibel > VoiceConfig.Instance.ExplodeDecibel && _explodeDelay < 0) {
                ExplodeProj.Spawn(Main.LocalPlayer.Center, decibel);
                _explodeDelay = 20;
            }
        }
    }
}