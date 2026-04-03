using UnityEngine;

public class GalaxySampler : MonoBehaviour
{
    private UniverseFieldSampler field;

    void Awake()
    {
        field = GetComponent<UniverseFieldSampler>();
    }

    public bool IsGalaxyNode(Vector3 position)
    {
        float density = field.GetDensity(position);

        // Higher threshold = only dense regions survive
        return density > 0.85f;
    }
}