using System.Collections.Generic;
using UnityEngine;

public class GalaxyVisualizer : MonoBehaviour
{
    [SerializeField] private int halfExtent = 48;
    [SerializeField] private int spacing = 6;

    private SeedManager seed;
    private UniverseFieldSampler field;
    private readonly List<Transform> stars = new List<Transform>(4096);

    private void Start()
    {
        seed = GetComponent<SeedManager>();
        field = GetComponent<UniverseFieldSampler>();
        GenerateInitial();
    }

    private void Update()
    {
        float simTime = SimulationManager.Instance != null ? SimulationManager.Instance.SimulationTime : 0f;

        for (int i = 0; i < stars.Count; i++)
        {
            Transform star = stars[i];
            UniverseSample sample = field.Sample(star.position, simTime);

            float visibility = Mathf.SmoothStep(0.55f, 0.9f, sample.density);
            float twinkle = 0.75f + 0.25f * Mathf.Sin(simTime * 0.1f + i * 0.13f);

            float scale = 0.08f + sample.density * 0.25f;
            star.localScale = Vector3.one * scale;

            Renderer renderer = star.GetComponent<Renderer>();
            Color color = Color.Lerp(new Color(0.05f, 0.08f, 0.2f), Color.white, visibility * twinkle);
            renderer.material.color = color;
            renderer.enabled = visibility > 0.05f;
        }
    }

    private void GenerateInitial()
    {
        int deterministic = seed.SeedValue;

        for (int x = -halfExtent; x <= halfExtent; x += spacing)
        {
            for (int y = -halfExtent / 3; y <= halfExtent / 3; y += spacing)
            {
                for (int z = -halfExtent; z <= halfExtent; z += spacing)
                {
                    int cellHash = deterministic + (x * 73856093) ^ (y * 19349663) ^ (z * 83492791);
                    float jitterX = (DeterministicNoise.Hash01(cellHash, 1, 17, 5) * 2f - 1f) * (spacing * 0.4f);
                    float jitterY = (DeterministicNoise.Hash01(cellHash, 2, 19, 7) * 2f - 1f) * (spacing * 0.4f);
                    float jitterZ = (DeterministicNoise.Hash01(cellHash, 3, 23, 11) * 2f - 1f) * (spacing * 0.4f);

                    Vector3 pos = new Vector3(x + jitterX, y + jitterY, z + jitterZ);

                    GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    star.name = "Star";
                    star.transform.SetParent(transform, false);
                    star.transform.localPosition = pos;
                    Destroy(star.GetComponent<Collider>());

                    stars.Add(star.transform);
                }
            }
        }
    }
}
