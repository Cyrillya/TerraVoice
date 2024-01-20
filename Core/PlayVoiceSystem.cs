using System;
using Microsoft.Xna.Framework;
using NAudio.Wave.SampleProviders;
using Steamworks;
using Terraria;
using Terraria.ModLoader;
using TerraVoice.Misc;

namespace TerraVoice.Core;

[Autoload(Side = ModSide.Client)]
public class PlayVoiceSystem : ModSystem
{
    [Autoload(Side = ModSide.Client)]
    private class ModPlayerForEnterWorldHook : ModPlayer
    {
        public override void OnEnterWorld() {
            foreach (var playerSpeaker in PlayerSpeakers) {
                playerSpeaker?.ClearBuffer();
            }
        }
    }

    private static readonly byte[] VoiceDataBuffer = new byte[10000];
    private static readonly byte[] DataDecompressedBuffer = new byte[50 * 1024];
    public static PlayerSpeaker[] PlayerSpeakers { get; private set; }
    internal const uint SampleRate = 22050; // 44100;
    private static WaveGraphRenderer _waveGraphRenderer;

    /// <summary>
    /// 向音频缓存区中添加数据，不要将缓存区填满
    /// </summary>
    /// <param name="player">玩家编号</param>
    /// <param name="data">要填入的数据</param>
    /// <param name="position">数据的起始位置</param>
    /// <param name="len">数据的长度</param>
    public static void AddDataToBufferedWaveProvider(int player, byte[] data, int position, int len) {
        PlayerSpeakers[player] ??= new PlayerSpeaker();
        PlayerSpeakers[player].AddSamples(data, position, len);
        SetPanAndVolume(PlayerSpeakers[player].WaveProvider, player);
    }

    private static void SetPanAndVolume(PanningSampleProvider provider, int whoAmI) {
        if (!VoiceConfig.Instance.VoiceAttenuation) {
            ((SinPanStrategyWithVolume) provider.PanStrategy).Volume = PersonalConfig.Instance.VoiceVolume / 100f;
            provider.Pan = 0f;
            return;
        }

        var player = Main.player[whoAmI];
        var screenCenter = Main.screenPosition + Main.ScreenSize.ToVector2() / 2f;
        float playerToCenterX = player.Center.X - screenCenter.X;
        int attenuationDistance = VoiceConfig.Instance.VoiceAttenuationDistance * 16;
        // 声音衰减和声音定位(3D音效)
        float volume = Math.Clamp(1f - Math.Abs(playerToCenterX) / attenuationDistance, 0f, 1f);
        volume *= PersonalConfig.Instance.VoiceVolume / 100f;
        float pan = Math.Clamp(playerToCenterX / attenuationDistance, -1f, 1f);
        ((SinPanStrategyWithVolume) provider.PanStrategy).Volume = volume;
        provider.Pan = pan;
    }

    public override void Load() {
        PlayerSpeakers = new PlayerSpeaker[Main.maxPlayers];
        _waveGraphRenderer = new WaveGraphRenderer();
    }

    public override void Unload() {
        foreach (var playerSpeaker in PlayerSpeakers) {
            playerSpeaker?.Dispose();
        }

        PlayerSpeakers = null;
        _waveGraphRenderer?.Dispose();
        _waveGraphRenderer = null;
    }

    public static void ReceiveVoiceBuffer(int sender, byte[] buffer, uint dataSize) {
        SteamUser.DecompressVoice(buffer, dataSize, DataDecompressedBuffer,
            (uint) DataDecompressedBuffer.Length, out uint bytesWritten, SampleRate);
        AddDataToBufferedWaveProvider(sender, DataDecompressedBuffer, 0, (int) bytesWritten);
    }

    // 找不到合适的地方放语音获取代码（因为要求无论是否暂停都每帧读取）就放这里了
    public override void UpdateUI(GameTime gameTime) {
        if (SteamUser.GetAvailableVoice(out var dataSize) is not EVoiceResult.k_EVoiceResultOK || dataSize <= 0) return;

        SteamUser.GetVoice(true, VoiceDataBuffer, dataSize, out var bytesWrittden);
        TerraVoice.PushVoiceBuffer(VoiceDataBuffer, dataSize);
        // if (data.Any(d => d is not 0)) {
        //     Main.NewText("Yes");
        // }

        SteamUser.DecompressVoice(VoiceDataBuffer, dataSize, DataDecompressedBuffer,
            (uint) DataDecompressedBuffer.Length, out uint bytesWritten, SampleRate);
        _waveGraphRenderer.AddSamples(DataDecompressedBuffer, 0, (int) bytesWritten);
    }
}