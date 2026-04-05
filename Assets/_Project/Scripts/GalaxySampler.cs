using UnityEngine;

public class GalaxySampler : MonoBehaviour
{
    private UniverseFieldSampler field;

    private void Awake() => field = GetComponent<UniverseFieldSampler>();

    public bool IsGalaxyNode(Vector3 position)
    {
        if (SimulationManager.Instance == null) return false;
        UniverseSample s = field.Sample(position, SimulationManager.Instance.SimulationTime);
        return s.density > 0.75f;
    }
}
