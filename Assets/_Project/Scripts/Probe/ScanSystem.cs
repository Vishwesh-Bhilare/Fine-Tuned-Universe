using System;
using UnityEngine;

public class ScanSystem : MonoBehaviour
{
    [SerializeField] private Transform probe;
    [SerializeField] private float scanDistance = 400f;

    public event Action<ScanResult> OnScanCompleted;

    private void Update()
    {
        if (!GameInput.GetKeyDown(KeyCode.Space) || probe == null)
        {
            return;
        }

        ScanForward();
    }

    public bool ScanForward()
    {
        if (probe == null || SimulationManager.Instance == null)
        {
            return false;
        }

        Vector3 samplePos = probe.position + probe.forward * scanDistance;
        UniverseSample sample = SimulationManager.Instance.Sample(samplePos);

        ScanResult result = new ScanResult
        {
            sample = sample,
            signalStrength = Mathf.Clamp01(sample.lifeProbability * 0.7f + sample.density * 0.3f),
            signature = $"{sample.planetArchetype}|D{sample.density:0.00}|R{sample.radiation:0.00}|E{sample.entropy:0.00}"
        };

        OnScanCompleted?.Invoke(result);
        return true;
    }

    public void SetProbe(Transform probeTransform)
    {
        probe = probeTransform;
    }
}
