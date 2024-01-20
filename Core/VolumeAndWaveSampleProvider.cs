using NAudio.Wave;
using TerraVoice.Misc;

namespace TerraVoice.Core;

public class VolumeAndWaveSampleProvider : ISampleProvider
{
    private readonly ISampleProvider source;

    public VolumeAndWaveSampleProvider(ISampleProvider source) {
        this.source = source;
        Volume = 1.0f;
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int sampleCount) {
        int samplesRead = source.Read(buffer, offset, sampleCount);
        bool showWave = PersonalConfig.Instance.ShowWave;
        for (int n = 0; n < sampleCount; n++) {
            if (showWave && n % 80 == 0) {
                while (DrawingSystem.WaveDatas.Count > 600) {
                    DrawingSystem.WaveDatas.Dequeue();
                }

                DrawingSystem.WaveDatas.Enqueue(buffer[offset + n]);
            }

            buffer[offset + n] *= Volume;
        }

        return samplesRead;
    }

    public float Volume { get; set; }
}