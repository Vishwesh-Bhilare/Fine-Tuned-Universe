using UnityEngine;

public class LocalInfluenceSystem : MonoBehaviour
{
    public enum InfluenceType
    {
        None,
        MicroCompress,
        StabilizeTurbulence,
        SeedStarFormation
    }

    [SerializeField] private Transform probe;
    [SerializeField] private float pulseRadius = 30f;
    [SerializeField] private float pulseDuration = 8f;

    private float pulseUntilTime;
    private Vector3 pulseCenter;
    private InfluenceType activeType;
    public InfluenceType ActiveType => activeType;

    private void Update()
    {
        if (probe == null)
        {
            return;
        }

        if (GameInput.GetKeyDown(KeyCode.Alpha1))
        {
            TriggerPulse(InfluenceType.MicroCompress);
        }

        if (GameInput.GetKeyDown(KeyCode.Alpha2))
        {
            TriggerPulse(InfluenceType.StabilizeTurbulence);
        }

        if (GameInput.GetKeyDown(KeyCode.Alpha3))
        {
            TriggerPulse(InfluenceType.SeedStarFormation);
        }
    }

    public void TriggerPulse(InfluenceType type = InfluenceType.MicroCompress)
    {
        activeType = type;
        pulseCenter = probe.position + probe.forward * 15f;
        pulseUntilTime = SimulationManager.Instance.SimulationTime + pulseDuration;
    }

    public float GetInfluenceAt(Vector3 worldPosition)
    {
        if (SimulationManager.Instance == null || SimulationManager.Instance.SimulationTime > pulseUntilTime)
        {
            activeType = InfluenceType.None;
            return 0f;
        }

        float dist = Vector3.Distance(worldPosition, pulseCenter);
        if (dist > pulseRadius)
        {
            return 0f;
        }

        return 1f - dist / pulseRadius;
    }

    public float TimeRemaining()
    {
        if (SimulationManager.Instance == null)
        {
            return 0f;
        }

        return Mathf.Max(0f, pulseUntilTime - SimulationManager.Instance.SimulationTime);
    }

    public void SetProbe(Transform probeTransform)
    {
        probe = probeTransform;
    }
}
