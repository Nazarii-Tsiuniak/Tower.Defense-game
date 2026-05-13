using UnityEngine;

// Generates a looping procedural medieval-style background track at runtime.
// No audio files needed — everything is synthesized from sine waves.
public class BackgroundMusic : MonoBehaviour
{
    private const int SampleRate = 44100;
    private const float MasterVolume = 0.28f;

    void Start()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.clip = BuildTrack();
        src.loop = true;
        src.volume = MasterVolume;
        src.spatialBlend = 0f;  // 2D
        src.playOnAwake = false;
        src.Play();
    }

    // ── note frequencies (Hz) ──────────────────────────────────────────────
    // D minor pentatonic: D4, F4, G4, A4, C5, D5
    static float D4  = 293.66f;
    static float E4  = 329.63f;
    static float F4  = 349.23f;
    static float G4  = 392.00f;
    static float A4  = 440.00f;
    static float Bb4 = 466.16f;
    static float C5  = 523.25f;
    static float D5  = 587.33f;
    static float F5  = 698.46f;

    // Each entry: (freq, beats)
    // Melody: 16-beat loop that repeats twice (32 beats total)
    static readonly (float freq, float beats)[] MelodyNotes = new[]
    {
        (D4,  1f), (F4, 0.5f), (G4, 0.5f), (A4,  1f), (G4, 0.5f), (F4, 0.5f),
        (E4,  1f), (F4, 0.5f), (G4, 0.5f), (F4,  1f), (D4, 1f),
        (A4,  1f), (C5, 0.5f), (Bb4,0.5f), (A4,  1f), (G4, 0.5f), (F4, 0.5f),
        (G4,  1f), (A4, 0.5f), (G4, 0.5f), (F4,  1f), (D4, 1f),
    };

    // Drone / pad: whole-note chords (D, F, A) for a warm backdrop
    static readonly (float[] freqs, float beats)[] DroneNotes = new[]
    {
        (new[]{ D4, F4, A4 },  4f),
        (new[]{ E4, G4, Bb4},  4f),
        (new[]{ F4, A4, C5 },  4f),
        (new[]{ G4, Bb4, D5},  4f),
        (new[]{ D4, F4, A4 },  4f),
        (new[]{ E4, G4, Bb4},  4f),
        (new[]{ F4, A4, C5 },  4f),
        (new[]{ A4, C5, F5 },  4f),
    };

    AudioClip BuildTrack()
    {
        float bpm   = 88f;
        float beat  = 60f / bpm;

        // Calculate total duration from melody (it loops twice)
        float melodyDur = 0f;
        foreach (var n in MelodyNotes) melodyDur += n.beats * beat;
        float totalDur = melodyDur * 2f;   // two melody passes = one full loop

        int totalSamples = Mathf.CeilToInt(totalDur * SampleRate);
        float[] data = new float[totalSamples];

        // ── Layer 1: Melody (lute-like timbre: fundamental + 2nd + 3rd harmonic) ──
        AddMelodyLayer(data, beat, melodyDur, 0.55f);
        AddMelodyLayer(data, beat, melodyDur, 0.55f, (int)(melodyDur * SampleRate));  // 2nd pass

        // ── Layer 2: Drone pad (soft, low volume) ──
        AddDroneLayer(data, beat, 0.18f);

        // ── Layer 3: Simple bass line (root notes, slow) ──
        AddBassLayer(data, beat, totalDur, 0.28f);

        // Normalize to avoid clipping
        float peak = 0f;
        foreach (float s in data) if (Mathf.Abs(s) > peak) peak = Mathf.Abs(s);
        if (peak > 0.9f)
        {
            float scale = 0.9f / peak;
            for (int i = 0; i < data.Length; i++) data[i] *= scale;
        }

        var clip = AudioClip.Create("BGMusic", totalSamples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    void AddMelodyLayer(float[] data, float beat, float loopDur, float vol, int startSample = 0)
    {
        int pos = startSample;
        foreach (var note in MelodyNotes)
        {
            int noteSamples = Mathf.RoundToInt(note.beats * beat * SampleRate);
            int attack  = Mathf.RoundToInt(0.015f * SampleRate);
            int release = Mathf.RoundToInt(Mathf.Min(0.12f, note.beats * beat * 0.4f) * SampleRate);

            for (int s = 0; s < noteSamples && pos + s < data.Length; s++)
            {
                float t = (float)s / SampleRate;
                // Lute: fundamental + softer harmonics
                float wave  = Mathf.Sin(2f * Mathf.PI * note.freq * t);
                wave += 0.45f * Mathf.Sin(2f * Mathf.PI * note.freq * 2f * t);
                wave += 0.20f * Mathf.Sin(2f * Mathf.PI * note.freq * 3f * t);
                wave /= 1.65f;

                float env = 1f;
                if (s < attack)
                    env = (float)s / attack;
                else if (s > noteSamples - release)
                    env = (float)(noteSamples - s) / release;

                data[pos + s] += wave * env * vol;
            }
            pos += noteSamples;
        }
    }

    void AddDroneLayer(float[] data, float beat, float vol)
    {
        int pos = 0;
        foreach (var chord in DroneNotes)
        {
            int chordSamples = Mathf.RoundToInt(chord.beats * beat * SampleRate);
            int release = Mathf.RoundToInt(0.25f * SampleRate);

            for (int s = 0; s < chordSamples && pos + s < data.Length; s++)
            {
                float t = (float)s / SampleRate;
                float wave = 0f;
                foreach (float f in chord.freqs)
                    wave += Mathf.Sin(2f * Mathf.PI * f * t);
                wave /= chord.freqs.Length;

                float env = 1f;
                if (s > chordSamples - release)
                    env = (float)(chordSamples - s) / release;

                data[pos + s] += wave * env * vol;
            }
            pos += chordSamples;
        }
    }

    // Simple bass: root note D3/F3/A3/G3 alternating every 2 beats
    void AddBassLayer(float[] data, float beat, float totalDur, float vol)
    {
        float[] bassFreqs = { 146.83f, 174.61f, 220.00f, 196.00f };  // D3,F3,A3,G3
        int bIdx = 0;
        int pos  = 0;
        int step = Mathf.RoundToInt(2f * beat * SampleRate);

        while (pos < data.Length)
        {
            float freq = bassFreqs[bIdx % bassFreqs.Length];
            int noteSamples = Mathf.Min(step, data.Length - pos);
            int attack  = Mathf.RoundToInt(0.02f  * SampleRate);
            int release = Mathf.RoundToInt(0.18f  * SampleRate);

            for (int s = 0; s < noteSamples; s++)
            {
                float t = (float)s / SampleRate;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t)
                           + 0.3f * Mathf.Sin(2f * Mathf.PI * freq * 2f * t);
                wave /= 1.3f;

                float env = 1f;
                if (s < attack)
                    env = (float)s / attack;
                else if (s > noteSamples - release)
                    env = (float)(noteSamples - s) / release;

                data[pos + s] += wave * env * vol;
            }
            pos += step;
            bIdx++;
        }
    }
}
