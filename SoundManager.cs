using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NAudio.Wave;

namespace MazeQuest;

public static class SoundManager
{
    private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string AudioDir = Path.Combine(BaseDir, "audio");
    private static readonly string CustomMusicDir = Path.Combine(BaseDir, "music");

    private static bool _initialized = false;
    private static bool _musicEnabled = true;
    private static bool _sfxEnabled = true;

    
    private static WaveOutEvent? _bgmOut;
    private static LoopStream? _bgmStream;
    private static string? _currentMusicPath;

    
    private const double C3 = 130.81, D3 = 146.83, E3 = 164.81, F3 = 174.61, G3 = 196.00, A3 = 220.00, B3 = 246.94;
    private const double C4 = 261.63, D4 = 293.66, E4 = 329.63, F4 = 349.23, G4 = 392.00, A4 = 440.00, B4 = 493.88;
    private const double C5 = 523.25, D5 = 587.33, E5 = 659.25, F5 = 698.46, G5 = 783.99, A5 = 880.00;
    private const double REST = 0;

    public static void Initialize()
    {
        if (_initialized) return;

        try
        {
            Directory.CreateDirectory(AudioDir);
            Directory.CreateDirectory(CustomMusicDir);

            GenerateMenuMusic();
            GenerateGameMusic();
            GenerateSoundEffects();
            _initialized = true;
        }
        catch
        {
            _initialized = false;
        }
    }

    public static void ToggleMusic()
    {
        _musicEnabled = !_musicEnabled;
        if (!_musicEnabled) StopMusic();
    }

    public static void ToggleSFX()
    {
        _sfxEnabled = !_sfxEnabled;
    }

    public static void PlayMenuMusic()
    {
        if (!_initialized || !_musicEnabled) return;
        string? path = FindCustomFile("menu_music") ?? FindAudioFile("menu_music");
        if (path != null) StartMusicLoop(path);
    }

    public static void PlayGameMusic()
    {
        if (!_initialized || !_musicEnabled) return;
        string? path = FindCustomFile("game_music") ?? FindAudioFile("game_music");
        if (path != null) StartMusicLoop(path);
    }

    public static void StopMusic()
    {
        try
        {
            _bgmOut?.Stop();
            _bgmOut?.Dispose();
            _bgmStream?.Dispose();
            _bgmOut = null;
            _bgmStream = null;
            _currentMusicPath = null;
        }
        catch { }
    }

    private static void StartMusicLoop(string path)
    {
        if (_currentMusicPath == path && _bgmOut != null && _bgmOut.PlaybackState == PlaybackState.Playing)
            return; 

        StopMusic();

        try
        {
            _currentMusicPath = path;
            var reader = new AudioFileReader(path);
            _bgmStream = new LoopStream(reader);
            _bgmOut = new WaveOutEvent();
            _bgmOut.Init(_bgmStream);
            _bgmOut.Play();
        }
        catch
        {
            
        }
    }

    
    
    
    private class LoopStream : WaveStream
    {
        private WaveStream sourceStream;
        public LoopStream(WaveStream sourceStream) { this.sourceStream = sourceStream; }
        public override WaveFormat WaveFormat => sourceStream.WaveFormat;
        public override long Length => sourceStream.Length;
        public override long Position { get => sourceStream.Position; set => sourceStream.Position = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (sourceStream.Position == 0) break; 
                    sourceStream.Position = 0; 
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            sourceStream.Dispose();
            base.Dispose(disposing);
        }
    }

    public static void PlayMove() => PlaySFX("sfx_move.wav");
    public static void PlayItemPickup() => PlaySFX("sfx_item.wav");
    public static void PlayAttack() => PlaySFX("sfx_attack.wav");
    public static void PlayHurt() => PlaySFX("sfx_hurt.wav");
    public static void PlayEnemyKill() => PlaySFX("sfx_kill.wav");
    public static void PlayLevelComplete() => PlaySFX("sfx_level.wav");
    public static void PlayGameOver() => PlaySFX("sfx_gameover.wav");
    public static void PlayUndo() => PlaySFX("sfx_undo.wav");
    public static void PlayHeal() => PlaySFX("sfx_heal.wav");
    public static void PlayVictory() => PlaySFX("sfx_victory.wav");
    public static void PlayTrap() => PlaySFX("sfx_trap.wav");
    public static void PlayMenuSelect() => PlaySFX("sfx_select.wav");

    private static void PlaySFX(string filename)
    {
        if (!_initialized || !_sfxEnabled) return;
        string path = Path.Combine(AudioDir, filename);
        if (!File.Exists(path)) return;

        
        Task.Run(() =>
        {
            try
            {
                using var reader = new AudioFileReader(path);
                using var output = new WaveOutEvent();
                output.Init(reader);
                output.Play();
                while (output.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }
            }
            catch { }
        });
    }

    private static string? FindCustomFile(string baseName)
    {
        string[] extensions = { ".mp3", ".wav" };
        foreach (var ext in extensions)
        {
            string path = Path.Combine(CustomMusicDir, baseName + ext);
            if (File.Exists(path)) return path;
        }
        return null;
    }

    private static string? FindAudioFile(string baseName)
    {
        string path = Path.Combine(AudioDir, baseName + ".wav");
        return File.Exists(path) ? path : null;
    }

    private static void GenerateMenuMusic()
    {
        string path = Path.Combine(AudioDir, "menu_music.wav");
        if (File.Exists(path)) return;

        int sampleRate = 22050;
        var samples = new List<double>();
        (double[] chord, double duration)[] progression =
        {
            (new[] { A3, C4, E4 }, 0.3), (new[] { C4, E4, A4 }, 0.3), (new[] { E4, A4, C5 }, 0.3), (new[] { C4, E4, A4 }, 0.3),
            (new[] { F3, A3, C4 }, 0.3), (new[] { A3, C4, F4 }, 0.3), (new[] { C4, F4, A4 }, 0.3), (new[] { A3, C4, F4 }, 0.3),
            (new[] { C4, E4, G4 }, 0.3), (new[] { E4, G4, C5 }, 0.3), (new[] { G4, C5, E5 }, 0.3), (new[] { E4, G4, C5 }, 0.3),
            (new[] { G3, B3, D4 }, 0.3), (new[] { B3, D4, G4 }, 0.3), (new[] { D4, G4, B4 }, 0.3), (new[] { B3, D4, G4 }, 0.3)
        };
        for (int repeat = 0; repeat < 4; repeat++)
        {
            foreach (var (chord, dur) in progression)
            {
                int numSamples = (int)(dur * sampleRate);
                for (int i = 0; i < numSamples; i++)
                {
                    double t = (double)i / sampleRate;
                    double env = Envelope(i, numSamples, 0.05, 0.3);
                    double sample = 0;
                    foreach (double freq in chord) sample += SineWave(t, freq) * 0.2;
                    sample += SineWave(t, chord[0] * 0.5) * 0.15;
                    samples.Add(sample * env * 0.5);
                }
            }
        }
        WriteWav(path, samples.ToArray(), sampleRate);
    }

    private static void GenerateGameMusic()
    {
        string path = Path.Combine(AudioDir, "game_music.wav");
        if (File.Exists(path)) return;

        int sampleRate = 22050;
        var samples = new List<double>();
        (double freq, double bassDur)[] bassLine =
        {
            (D3, 0.15), (REST, 0.05), (D3, 0.10), (REST, 0.05), (D3, 0.15), (A3, 0.10), (REST, 0.05), (F3, 0.15),
            (REST, 0.05), (D3, 0.15),
            (B3 * 0.944, 0.15), (REST, 0.05), (B3 * 0.944, 0.10), (REST, 0.05), (B3 * 0.944, 0.15), (F3, 0.10), (REST, 0.05), (D3, 0.15),
            (REST, 0.05), (B3 * 0.944, 0.15),
            (G3, 0.15), (REST, 0.05), (G3, 0.10), (REST, 0.05), (G3, 0.15), (D4, 0.10), (REST, 0.05), (B3 * 0.944, 0.15),
            (REST, 0.05), (G3, 0.15),
            (A3, 0.15), (REST, 0.05), (A3, 0.10), (REST, 0.05), (A3, 0.15), (E4, 0.10), (REST, 0.05), (C4 * 1.06, 0.15),
            (REST, 0.05), (A3, 0.15),
        };
        for (int repeat = 0; repeat < 5; repeat++)
        {
            foreach (var (freq, dur) in bassLine)
            {
                int numSamples = (int)(dur * sampleRate);
                for (int i = 0; i < numSamples; i++)
                {
                    double t = (double)i / sampleRate;
                    double env = Envelope(i, numSamples, 0.01, 0.6);
                    double sample = 0;
                    if (freq > 0)
                    {
                        sample += SquareWave(t, freq) * 0.2;
                        sample += SineWave(t, freq * 0.5) * 0.25;
                        sample += TriangleWave(t, freq * 2) * 0.08;
                    }
                    samples.Add(sample * env * 0.5);
                }
            }
        }
        WriteWav(path, samples.ToArray(), sampleRate);
    }

    private static void GenerateSoundEffects()
    {
        int sr = 22050;
        GenerateBlip(Path.Combine(AudioDir, "sfx_move.wav"), sr, 600, 0.06, 0.15);
        GenerateArpeggio(Path.Combine(AudioDir, "sfx_item.wav"), sr, new[] { C5, E5, G5 }, 0.08, 0.4);
        GenerateAttackSFX(Path.Combine(AudioDir, "sfx_attack.wav"), sr);
        GeneratePitchSweep(Path.Combine(AudioDir, "sfx_hurt.wav"), sr, 500, 150, 0.2, 0.4);
        GenerateArpeggio(Path.Combine(AudioDir, "sfx_kill.wav"), sr, new[] { E4, G4, B4, E5 }, 0.1, 0.5);
        GenerateArpeggio(Path.Combine(AudioDir, "sfx_level.wav"), sr, new[] { C4, E4, G4, C5, E5, G5, C5 * 2 }, 0.12, 0.5);
        GenerateArpeggio(Path.Combine(AudioDir, "sfx_gameover.wav"), sr, new[] { E4, D4, C4, B3, A3 }, 0.2, 0.35);
        GeneratePitchSweep(Path.Combine(AudioDir, "sfx_undo.wav"), sr, 300, 500, 0.08, 0.25);
        GenerateArpeggio(Path.Combine(AudioDir, "sfx_heal.wav"), sr, new[] { C4, E4, G4, C5 }, 0.1, 0.35);
        GenerateArpeggio(Path.Combine(AudioDir, "sfx_victory.wav"), sr, new[] { C4, C4, G4, G4, A4, A4, G4, REST, F4, F4, E4, E4, D4, D4, C4 }, 0.15, 0.5);
        GenerateTrapSFX(Path.Combine(AudioDir, "sfx_trap.wav"), sr);
        GenerateBlip(Path.Combine(AudioDir, "sfx_select.wav"), sr, 800, 0.04, 0.25);
    }

    private static double SineWave(double t, double freq) => Math.Sin(2 * Math.PI * freq * t);
    private static double SquareWave(double t, double freq) => Math.Sign(Math.Sin(2 * Math.PI * freq * t)) * 0.5;
    private static double TriangleWave(double t, double freq) => (2 / Math.PI) * Math.Asin(Math.Sin(2 * Math.PI * freq * t));
    private static double NoiseWave() => (Random.Shared.NextDouble() * 2 - 1);
    private static double Envelope(int sample, int total, double attack, double release)
    {
        double pos = (double)sample / total;
        if (pos < attack) return pos / attack;
        if (pos > 1.0 - release) return (1.0 - pos) / release;
        return 1.0;
    }

    private static void GenerateBlip(string path, int sr, double freq, double dur, double vol)
    {
        if (File.Exists(path)) return;
        int n = (int)(dur * sr);
        var samples = new double[n];
        for (int i = 0; i < n; i++) samples[i] = SineWave((double)i / sr, freq) * Envelope(i, n, 0.05, 0.5) * vol;
        WriteWav(path, samples, sr);
    }

    private static void GenerateArpeggio(string path, int sr, double[] notes, double dur, double vol)
    {
        if (File.Exists(path)) return;
        var samples = new List<double>();
        foreach (double freq in notes)
        {
            int n = (int)(dur * sr);
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / sr;
                samples.Add((freq > 0 ? SineWave(t, freq) * 0.6 + TriangleWave(t, freq) * 0.3 : 0) * Envelope(i, n, 0.02, 0.4) * vol);
            }
        }
        WriteWav(path, samples.ToArray(), sr);
    }

    private static void GeneratePitchSweep(string path, int sr, double f1, double f2, double dur, double vol)
    {
        if (File.Exists(path)) return;
        int n = (int)(dur * sr);
        var samples = new double[n];
        for (int i = 0; i < n; i++)
            samples[i] = SineWave((double)i / sr, f1 + (f2 - f1) * ((double)i / n)) * Envelope(i, n, 0.02, 0.4) * vol;
        WriteWav(path, samples, sr);
    }

    private static void GenerateAttackSFX(string path, int sr)
    {
        if (File.Exists(path)) return;
        int n = (int)(0.15 * sr);
        var samples = new double[n];
        for (int i = 0; i < n; i++)
            samples[i] = (NoiseWave() * 0.3 + SquareWave((double)i / sr, 120) * 0.3) * Envelope(i, n, 0.01, 0.6) * 0.45;
        WriteWav(path, samples, sr);
    }

    private static void GenerateTrapSFX(string path, int sr)
    {
        if (File.Exists(path)) return;
        int n = (int)(0.25 * sr);
        var samples = new double[n];
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / sr;
            samples[i] = (SquareWave(t, 200 + Math.Sin(t * 30) * 80) * 0.3 + NoiseWave() * 0.15) * Envelope(i, n, 0.01, 0.3) * 0.4;
        }
        WriteWav(path, samples, sr);
    }

    private static void WriteWav(string path, double[] samples, int sampleRate)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Create);
            using var bw = new BinaryWriter(fs);
            int dataSize = samples.Length * 2;

            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + dataSize);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16); bw.Write((short)1); bw.Write((short)1);
            bw.Write(sampleRate); bw.Write(sampleRate * 2); bw.Write((short)2); bw.Write((short)16);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(dataSize);

            foreach (double s in samples) bw.Write((short)(Math.Clamp(s, -1.0, 1.0) * 32767));
        }
        catch { }
    }

    public static void Shutdown() => StopMusic();
}
