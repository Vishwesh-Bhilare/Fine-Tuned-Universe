using System;
using UnityEngine;

public class ScanSystem : MonoBehaviour
{
    [SerializeField] private Transform probe;
    [SerializeField] private float     scanDistance = 400f;

    // Progression: scan types unlock via codex count
    private int unlockedScanTypes = 1; // 1 = basic, 2 = + biosig, 3 = + anomaly

    public event Action<ScanResult> OnScanCompleted;

    private void Update()
    {
        if (GameInput.GetKeyDown(KeyCode.Space) && probe != null)
            ScanForward();

        // Refresh unlock level from codex
        if (CodexManager.Instance != null)
        {
            int entries = CodexManager.Instance.EntryCount;
            unlockedScanTypes = entries >= 10 ? 3 : entries >= 4 ? 2 : 1;
        }
    }

    public bool ScanForward()
    {
        if (probe == null || SimulationManager.Instance == null) return false;

        for (int t = 0; t < unlockedScanTypes; t++)
        {
            Vector3      samplePos = probe.position + probe.forward * (scanDistance * (0.5f + t * 0.5f));
            UniverseSample sample   = SimulationManager.Instance.Sample(samplePos);

            string scanType = t switch { 1 => "biosig", 2 => "anomaly", _ => "basic" };

            float signalStrength = scanType == "biosig"
                ? sample.lifeProbability
                : scanType == "anomaly"
                    ? Mathf.Abs(sample.entropy - 0.5f) * 2f
                    : sample.density * 0.7f + sample.radiation * 0.3f;

            var result = new ScanResult
            {
                sample         = sample,
                signalStrength = Mathf.Clamp01(signalStrength),
                signature      = BuildSignature(sample),
                scanType       = scanType
            };

            OnScanCompleted?.Invoke(result);
        }
        return true;
    }

    private static string BuildSignature(UniverseSample s) =>
        $"{s.planetArchetype}|D{s.density:0.00}|R{s.radiation:0.00}|E{s.entropy:0.00}|L{s.lifeProbability:0.00}";

    public void SetProbe(Transform t) => probe = t;
    public float ScanDistance         => scanDistance;
}
