using UnityEngine;

public class GalaxyVisualizer : MonoBehaviour
{
    private SeedManager seed;
    private UniverseFieldSampler field;

    void Start()
    {
        seed = GetComponent<SeedManager>();
        field = GetComponent<UniverseFieldSampler>();

        GenerateInitial();
    }

    void Update()
    {
        UpdateStars();
    }

    void GenerateInitial()
    {
        for (int x = -60; x <= 60; x += 4)
        {
            for (int y = -20; y <= 20; y += 4)
            {
                for (int z = -60; z <= 60; z += 4)
                {
                    Vector3 basePos = new Vector3(x, y, z);

                    float jitterX = DeterministicNoise.Sample(x, z, seed.SeedValue) - 0.5f;
                    float jitterY = DeterministicNoise.Sample(y, x, seed.SeedValue + 33) - 0.5f;
                    float jitterZ = DeterministicNoise.Sample(z, y, seed.SeedValue + 77) - 0.5f;

                    Vector3 pos = basePos + new Vector3(
                        jitterX * 2f,
                        jitterY * 2f,
                        jitterZ * 2f
                    );

                    GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    star.transform.parent = transform;
                    star.transform.position = pos;

                    star.name = "Star";
                }
            }
        }
    }

    void UpdateStars()
    {
        foreach (Transform star in transform)
        {
            Vector3 pos = star.position;

            float density = field.GetDensity(pos);

            // smooth density mapping
            float visibility = Mathf.SmoothStep(0.65f, 0.9f, density);

            // FIX: constant scale (no breathing blobs)
            float baseScale = 0.2f + Mathf.Abs(Mathf.Sin(pos.x * 0.01f + pos.z * 0.01f)) * 0.1f;
            star.localScale = Vector3.one * baseScale;

            var renderer = star.GetComponent<Renderer>();

            // depth-based fade
            float depthFactor = Mathf.InverseLerp(-20f, 20f, pos.y);

            // stronger contrast
            float contrast = Mathf.Pow(visibility, 3f);

            renderer.material.color =
                Color.Lerp(Color.black, Color.white, contrast * depthFactor);
        }
    }
}