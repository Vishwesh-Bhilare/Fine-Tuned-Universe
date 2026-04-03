using UnityEngine;

public class FieldDebugVisualizer : MonoBehaviour
{
    private UniverseFieldSampler sampler;

    void Start()
    {
        sampler = GetComponent<UniverseFieldSampler>();

        for (int x = -10; x <= 10; x++)
        {
            for (int z = -10; z <= 10; z++)
            {
                Vector3 pos = new Vector3(x * 2, 0, z * 2);

                float density = sampler.GetDensity(pos);

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = pos;
                cube.transform.localScale = Vector3.one * (0.2f + density);

                cube.GetComponent<Renderer>().material.color =
                    Color.Lerp(Color.black, Color.white, density);
            }
        }
    }
}
