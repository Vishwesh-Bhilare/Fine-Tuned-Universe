using System.Collections.Generic;
using UnityEngine;

public class SignalSystem : MonoBehaviour
{
    [SerializeField] private Transform probe;
    [SerializeField] private float detectionRange = 700f;
    [SerializeField] private float tuneFrequency;

    private readonly List<Vector3> deterministicSignals = new List<Vector3>();
    private int lastTimeSlice = int.MinValue;

    private void Start()
    {
        BuildSignalField();
    }

    private void Update()
    {
        if (SimulationManager.Instance == null)
        {
            return;
        }

        int timeSlice = Mathf.FloorToInt(SimulationManager.Instance.SimulationTime * 2f);
        if (timeSlice != lastTimeSlice)
        {
            BuildSignalField();
        }
    }

    private void BuildSignalField()
    {
        deterministicSignals.Clear();

        int seed = SimulationManager.Instance.SeedManager.SeedValue;
        int timeSlice = Mathf.FloorToInt(SimulationManager.Instance.SimulationTime * 2f);
        lastTimeSlice = timeSlice;

        for (int i = 0; i < 96; i++)
        {
            float fx = DeterministicNoise.Hash01(seed, timeSlice, i, 11) * 2f - 1f;
            float fy = DeterministicNoise.Hash01(seed, timeSlice, i, 13) * 2f - 1f;
            float fz = DeterministicNoise.Hash01(seed, timeSlice, i, 17) * 2f - 1f;

            Vector3 pos = new Vector3(fx * 1600f, fy * 300f, fz * 1600f);
            UniverseSample sample = SimulationManager.Instance.Sample(pos);

            if (sample.lifeProbability > 0.42f || sample.gravityPotential > 0.7f)
            {
                deterministicSignals.Add(pos);
            }
        }
    }

    public bool TryGetSignalDirection(out Vector3 worldDirection)
    {
        worldDirection = Vector3.zero;

        if (probe == null || deterministicSignals.Count == 0)
        {
            return false;
        }

        float best = 0f;
        Vector3 bestDir = Vector3.zero;

        for (int i = 0; i < deterministicSignals.Count; i++)
        {
            Vector3 toSignal = deterministicSignals[i] - probe.position;
            float distance = toSignal.magnitude;
            if (distance > detectionRange) continue;

            float frequency = DeterministicNoise.Hash01(i, SimulationManager.Instance.SeedManager.SeedValue, 53, 91);
            float tuneMatch = 1f - Mathf.Abs(frequency - tuneFrequency);
            float score = tuneMatch / Mathf.Max(distance, 1f);

            if (score > best)
            {
                best = score;
                bestDir = toSignal.normalized;
            }
        }

        if (best <= 0f)
        {
            return false;
        }

        worldDirection = bestDir;
        return true;
    }

    public void SetTuneFrequency(float normalizedFrequency)
    {
        tuneFrequency = Mathf.Clamp01(normalizedFrequency);
    }

    public void SetProbe(Transform probeTransform)
    {
        probe = probeTransform;
    }
}
