using System.Media;
using ZenWin.Models;

namespace ZenWin.Services;

public sealed class AudioManager : IDisposable
{
    private SoundPlayer? _player;
    private MemoryStream? _stream;

    public void Play(AmbientSound sound)
    {
        Stop();
        var samples = Generate(sound, TimeSpan.FromSeconds(4), 44100);
        _stream = new MemoryStream(BuildWav(samples, 44100));
        _player = new SoundPlayer(_stream);
        _player.PlayLooping();
    }

    public void Stop()
    {
        _player?.Stop();
        _player?.Dispose();
        _stream?.Dispose();
        _player = null;
        _stream = null;
    }

    private static short[] Generate(AmbientSound sound, TimeSpan length, int rate)
    {
        var count = (int)(length.TotalSeconds * rate);
        var data = new short[count];
        var random = new Random(42);
        double brown = 0;
        for (var i = 0; i < count; i++)
        {
            var white = random.NextDouble() * 2 - 1;
            brown = (brown + 0.02 * white) / 1.02;
            var wave = sound switch
            {
                AmbientSound.WhiteNoise => white * 0.18,
                AmbientSound.Rain => white * 0.08 + Math.Sin(i * 0.006) * 0.025,
                AmbientSound.Forest => brown * 0.18 + Math.Sin(i * 0.0017) * 0.05,
                AmbientSound.Cafe => white * 0.055 + Math.Sin(i * 0.004) * 0.035,
                AmbientSound.Ocean => brown * 0.22 + Math.Sin(i * 0.0009) * 0.1,
                _ => brown * 0.25
            };
            data[i] = (short)Math.Clamp(wave * short.MaxValue, short.MinValue, short.MaxValue);
        }
        return data;
    }

    private static byte[] BuildWav(short[] samples, int sampleRate)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var byteRate = sampleRate * 2;
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + samples.Length * 2);
        writer.Write("WAVEfmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8.ToArray());
        writer.Write(samples.Length * 2);
        foreach (var sample in samples)
            writer.Write(sample);
        return stream.ToArray();
    }

    public void Dispose() => Stop();
}
