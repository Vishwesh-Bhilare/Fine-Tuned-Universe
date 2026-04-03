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
        for (int x = -100; x <= 100; x += 2)
        {
            for (int z = -100; z <= 100; z += 2)
            {
                Vector3 basePos = new Vector3(x, 0, z);

                float jitterX = DeterministicNoise.Sample(x, z, seed.SeedValue) - 0.5f;
                float jitterZ = DeterministicNoise.Sample(z, x, seed.SeedValue + 77) - 0.5f;

                Vector3 pos = basePos + new Vector3(jitterX * 2f, 0, jitterZ * 2f);

                GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                star.transform.parent = transform;
                star.transform.position = pos;

                star.name = "Star";
            }
        }
    }

    void UpdateStars()
    {
        foreach (Transform star in transform)
        {
            Vector3 pos = star.position;

            float density = field.GetDensity(pos);

            // smoother transition band
            float visibility = Mathf.SmoothStep(0.65f, 0.9f, density);

            // stronger visual response
            float pulse = Mathf.Sin(Time.time * 2f + pos.x * 0.1f + pos.z * 0.1f) * 0.1f;
            float scale = 0.1f + visibility * 3.5f + pulse;

            var renderer = star.GetComponent<Renderer>();

            // higher contrast
            float contrast = Mathf.Pow(visibility, 3f);
            renderer.material.color = Color.Lerp(Color.black, Color.white, contrast);
        }
    }
}