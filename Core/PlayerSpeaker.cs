using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Terraria;

namespace TerraVoice.Core;

/// <summary>
/// 某个玩家的语音播放器
/// </summary>
public class PlayerSpeaker
{
    public float RealVolume { get; set; }
    public float CurrentDisplayedVolume { get; set; }

    /// <summary>
    /// The WaveOut instance to play the sound
    /// </summary>
    public WaveOutEvent WaveOut;

    /// <summary>
    /// A dummy provider to help convert buffer into SampleProvider for PanningSampleProvider
    /// </summary>
    private readonly BufferedWaveProvider _bufferedWaveProvider;

    /// <summary>
    /// Ability to change the pan (where the sound plays, aka 3D sound effect)
    /// </summary>
    public PanningSampleProvider WaveProvider;

    public PlayerSpeaker() {
        WaveOut = new WaveOutEvent();
        _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat((int) PlayVoiceSystem.SampleRate, 1)) {
            DiscardOnBufferOverflow = true
        };
        var meteringSampleProvider = new MeteringSampleProvider(_bufferedWaveProvider.ToSampleProvider());
        meteringSampleProvider.StreamVolume += (_, args) => {
            RealVolume = args.MaxSampleValues.Max();
        };
        WaveProvider = new PanningSampleProvider(meteringSampleProvider) {
            PanStrategy = new SinPanStrategyWithVolume()
        };
        WaveOut.Init(WaveProvider);
        WaveOut.Play();
    }

    public void AddSamples(byte[] data, int position, int len) {
        _bufferedWaveProvider.AddSamples(data, position, len);
    }

    public void ClearBuffer() {
        _bufferedWaveProvider.ClearBuffer();
    }

    public void Dispose() {
        WaveOut.Dispose();
        WaveOut = null;
    }
}