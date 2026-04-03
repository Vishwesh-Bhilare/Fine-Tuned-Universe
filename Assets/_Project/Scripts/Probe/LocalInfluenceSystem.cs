using UnityEngine;

public class LocalInfluenceSystem : MonoBehaviour
{
    [SerializeField] private Transform probe;
    [SerializeField] private float pulseRadius = 30f;
    [SerializeField] private float pulseDuration = 8f;

    private float pulseUntilTime;
    private Vector3 pulseCenter;

    private void Update()
    {
        if (probe == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TriggerPulse();
        }
    }

    public void TriggerPulse()
    {
        pulseCenter = probe.position + probe.forward * 15f;
        pulseUntilTime = SimulationManager.Instance.SimulationTime + pulseDuration;
    }

    public float GetInfluenceAt(Vector3 worldPosition)
    {
        if (SimulationManager.Instance == null || SimulationManager.Instance.SimulationTime > pulseUntilTime)
        {
            return 0f;
        }

        float dist = Vector3.Distance(worldPosition, pulseCenter);
        if (dist > pulseRadius)
        {
            return 0f;
        }

        return 1f - dist / pulseRadius;
    }
}
