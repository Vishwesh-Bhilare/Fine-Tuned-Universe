using System.Collections.Generic;
using UnityEngine;

public class SignalSystem : MonoBehaviour
{
    [SerializeField] private Transform probe;
    [SerializeField] private float     detectionRange = 700f;
    [SerializeField] private float     tuneFrequency  = 0.5f;

    private readonly List<(Vector3 pos, float freq, string type)> signals = new();
    private int lastTimeSlice = int.MinValue;

    // Progression: detection range expands with codex entries
    private float effectiveRange => detectionRange + (CodexManager.Instance != null
        ? CodexManager.Instance.EntryCount * 15f
        : 0f);

    private void Start()  => RebuildSignalField();
    private void Update()
    {
        if (SimulationManager.Instance == null) return;
        int ts = Mathf.FloorToInt(SimulationManager.Instance.SimulationTime * 2f);
        if (ts != lastTimeSlice) RebuildSignalField();
    }

    private void RebuildSignalField()
    {
        signals.Clear();
        if (SimulationManager.Instance == null) return;

        int  seed      = SimulationManager.Instance.SeedManager.SeedValue;
        int  timeSlice = Mathf.FloorToInt(SimulationManager.Instance.SimulationTime * 2f);
        lastTimeSlice  = timeSlice;

        for (int i = 0; i < 96; i++)
        {
            float fx = DeterministicNoise.Hash01(seed, timeSlice, i, 11) * 2f - 1f;
            float fy = DeterministicNoise.Hash01(seed, timeSlice, i, 13) * 2f - 1f;
            float fz = DeterministicNoise.Hash01(seed, timeSlice, i, 17) * 2f - 1f;
            Vector3 pos = new Vector3(fx * 1600f, fy * 300f, fz * 1600f);

            UniverseSample s = SimulationManager.Instance.Sample(pos);
            if (s.lifeProbability < 0.42f && s.gravityPotential < 0.7f) continue;

            float freq = DeterministicNoise.Hash01(seed, i, 53, 91);
            string type = s.lifeProbability > 0.5f ? "biosig"
                        : s.gravityPotential > 0.7f ? "gravitational"
                        : "anomaly";

            signals.Add((pos, freq, type));
        }
    }

    public bool TryGetSignalDirection(out Vector3 dir, out string signalType)
    {
        dir        = Vector3.zero;
        signalType = string.Empty;
        if (probe == null || signals.Count == 0) return false;

        float   best    = 0f;
        Vector3 bestDir = Vector3.zero;
        string  bestType = string.Empty;

        foreach (var (pos, freq, type) in signals)
        {
            Vector3 toSig  = pos - probe.position;
            float   dist   = toSig.magnitude;
            if (dist > effectiveRange) continue;

            float tuneMatch = 1f - Mathf.Abs(freq - tuneFrequency);
            float score     = tuneMatch / Mathf.Max(dist, 1f);

            if (score > best) { best = score; bestDir = toSig.normalized; bestType = type; }
        }

        if (best <= 0f) return false;
        dir        = bestDir;
        signalType = bestType;
        return true;
    }

    public void SetTuneFrequency(float v) => tuneFrequency = Mathf.Clamp01(v);
    public void SetProbe(Transform t)     => probe         = t;
    public float TuneFrequency            => tuneFrequency;
}
