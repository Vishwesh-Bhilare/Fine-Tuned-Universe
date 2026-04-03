using UnityEngine;

public class GalaxySampler : MonoBehaviour
{
    private UniverseFieldSampler field;

    private void Awake()
    {
        field = GetComponent<UniverseFieldSampler>();
    }

    public bool IsGalaxyNode(Vector3 position)
    {
        UniverseSample sample = field.Sample(position, SimulationManager.Instance.SimulationTime);
        return sample.density > 0.75f;
    }
}
