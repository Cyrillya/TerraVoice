using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerraVoice.Misc;

public static class Helper
{
    /// <summary>
    /// 获取<b>大概</b>的分贝值
    /// </summary>
    /// <param name="volume">0-1的音量</param>
    /// <returns>分贝值</returns>
    public static float GetDecibel(float volume) {
        // P0的系数是乱调的，看着乐吧，差不多就得了。。。
        return 20 * (float)Math.Log10(volume / 1.786e-6);
    }

    public static float DamageMultiplierCurve(float db) {
        float factor = Math.Clamp(Utils.GetLerpValue(VoiceConfig.Instance.EnvironmentDecibel, 110, db), 0, 1);
        // 圆右下的1/4圆，即 x^2 + (y-1)^2 = 1 的下半部分
        float value = 2 - (MathF.Sqrt(1 - MathF.Pow(factor, 2)) + 1);
        // 更加明显的激增
        value = MathF.Pow(value, 1.3f);
        return value;
    }
}