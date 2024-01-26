using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using TerraVoice.Misc.ConfigElements;

namespace TerraVoice.Misc;

public class VoiceConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [CustomModConfigItem(typeof(DecibelTest))]
    public object DecibelTest;

    [DefaultValue(false)]
    public bool VoiceAttenuation;

    [Range(30, 150)]
    [DefaultValue(90)]
    [Slider]
    [Increment(5)]
    public int VoiceAttenuationDistance;
    
    [Header("SoundDamage")]
    [DefaultValue(false)]
    public bool DamageAffectedBySound;

    [Range(50, 100)]
    [DefaultValue(80)]
    [Slider]
    [Increment(5)]
    public int EnvironmentDecibel;

    [Range(0f, 1f)]
    [DefaultValue(0f)]
    [Slider]
    [Increment(0.1f)]
    public float DmgMultiMinimum;

    [Range(5, 100)]
    [DefaultValue(100)]
    [Slider]
    [Increment(5)]
    public int DmgMultiMaximum;
    
    [Header("QuietOrExplode")]
    [DefaultValue(false)]
    public bool ExplodeMode;

    [Range(80, 115)]
    [DefaultValue(96)]
    [Slider]
    [Increment(1)]
    public int ExplodeDecibel;
    
    public static VoiceConfig Instance => ModContent.GetInstance<VoiceConfig>();
}