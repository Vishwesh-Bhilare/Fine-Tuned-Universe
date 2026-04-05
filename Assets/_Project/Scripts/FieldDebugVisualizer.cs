using UnityEngine;

public class FieldDebugVisualizer : MonoBehaviour
{
    [SerializeField] private int   radius  = 10;
    [SerializeField] private float spacing = 2f;

    private UniverseFieldSampler sampler;

    private void Start()
    {
        sampler = GetComponent<UniverseFieldSampler>();
        if (sampler == null) return;

        for (int x = -radius; x <= radius; x++)
        for (int z = -radius; z <= radius; z++)
        {
            Vector3        pos    = new Vector3(x * spacing, 0f, z * spacing);
            UniverseSample sample = sampler.Sample(pos, 0f);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position   = pos;
            cube.transform.localScale = Vector3.one * (0.1f + sample.density * 1.5f);
            cube.GetComponent<Renderer>().material.color =
                Color.Lerp(Color.black, Color.cyan, sample.lifeProbability);
            Destroy(cube.GetComponent<Collider>());
        }
    }
}
