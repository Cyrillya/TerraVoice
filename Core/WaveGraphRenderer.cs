using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Terraria;
using TerraVoice.Misc;

namespace TerraVoice.Core;

public class WaveGraphRenderer
{
    private WaveOutEvent _waveOut;

    private readonly BufferedWaveProvider _bufferedWaveProvider;

    public WaveGraphRenderer() {
        _waveOut = new WaveOutEvent();
        _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat((int) PlayVoiceSystem.SampleRate, 1)) {
            DiscardOnBufferOverflow = true
        };
        var meteringSampleProvider = new MeteringSampleProvider(_bufferedWaveProvider.ToSampleProvider());
        meteringSampleProvider.StreamVolume += (_, args) => {
            DrawingSystem.RealVolume = args.MaxSampleValues.Max();
        };
        var waveProvider = new VolumeSampleProvider(meteringSampleProvider) {
            Volume = 0f
        };
        _waveOut.Init(waveProvider);
        _waveOut.Play();
    }

    public void AddSamples(byte[] data, int position, int len) {
        _bufferedWaveProvider.AddSamples(data, position, len);
    }

    public void Dispose() {
        _waveOut.Dispose();
        _waveOut = null;
    }
}