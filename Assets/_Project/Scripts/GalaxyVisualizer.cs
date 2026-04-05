using System.Collections.Generic;
using UnityEngine;

public class GalaxyVisualizer : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private int   halfExtent = 48;
    [SerializeField] private int   spacing    = 6;

    [Header("LOD")]
    [SerializeField] private float fullDetailRange   = 80f;
    [SerializeField] private float medDetailRange    = 200f;
    [SerializeField] private Camera observatoryCamera;

    private SeedManager         seed;
    private UniverseFieldSampler field;

    private readonly List<StarEntry> stars = new(4096);

    private struct StarEntry
    {
        public Transform  xform;
        public Renderer   rend;
        public Vector3    worldPos;
    }

    private void Start()
    {
        seed  = GetComponent<SeedManager>();
        field = GetComponent<UniverseFieldSampler>();
        GenerateStars();
    }

    private void Update()
    {
        if (SimulationManager.Instance == null) return;

        float simTime = SimulationManager.Instance.SimulationTime;

        // Determine reference point for LOD
        Vector3 refPoint = (observatoryCamera != null)
            ? observatoryCamera.transform.position
            : Vector3.zero;

        for (int i = 0; i < stars.Count; i++)
        {
            var entry = stars[i];
            float dist = Vector3.Distance(refPoint, entry.worldPos);

            // LOD: skip update for far stars most frames
            if (dist > medDetailRange && Time.frameCount % 8 != i % 8) continue;

            UniverseSample s = field.Sample(entry.worldPos, simTime);

            float visibility = Mathf.SmoothStep(0.55f, 0.9f, s.density);
            float twinkle    = 0.75f + 0.25f * Mathf.Sin(simTime * 0.1f + i * 0.13f);

            entry.rend.enabled = visibility > 0.05f;
            if (!entry.rend.enabled) continue;

            float scale = dist < fullDetailRange
                ? 0.08f + s.density * 0.25f
                : dist < medDetailRange
                    ? 0.12f + s.density * 0.2f
                    : 0.2f + s.density * 0.15f;

            entry.xform.localScale = Vector3.one * scale;

            Color col = Color.Lerp(new Color(0.05f, 0.08f, 0.2f), Color.white, visibility * twinkle);
            entry.rend.material.color = col;
        }
    }

    private void GenerateStars()
    {
        int det = seed.SeedValue;

        for (int x = -halfExtent; x <= halfExtent; x += spacing)
        for (int y = -halfExtent / 3; y <= halfExtent / 3; y += spacing)
        for (int z = -halfExtent; z <= halfExtent; z += spacing)
        {
            int cellHash = det + (x * 73856093) ^ (y * 19349663) ^ (z * 83492791);
            float jx = (DeterministicNoise.Hash01(cellHash, 1, 17, 5)  * 2f - 1f) * (spacing * 0.4f);
            float jy = (DeterministicNoise.Hash01(cellHash, 2, 19, 7)  * 2f - 1f) * (spacing * 0.4f);
            float jz = (DeterministicNoise.Hash01(cellHash, 3, 23, 11) * 2f - 1f) * (spacing * 0.4f);

            Vector3 pos = new Vector3(x + jx, y + jy, z + jz);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Star";
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            Destroy(go.GetComponent<Collider>());

            // Use single shared material per star to reduce draw calls
            var rend = go.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Standard"))
            {
                color           = Color.white,
                enableInstancing = true
            };

            stars.Add(new StarEntry
            {
                xform    = go.transform,
                rend     = rend,
                worldPos = go.transform.position
            });
        }
    }

    public void SetObservatoryCamera(Camera cam) => observatoryCamera = cam;
}
