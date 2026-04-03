using UnityEngine;

public class UniverseFieldSampler : MonoBehaviour
{
    private SeedManager seedManager;
    private ConstantsManager constantsManager;

    void Awake()
    {
        seedManager = GetComponent<SeedManager>();
        constantsManager = GetComponent<ConstantsManager>();
    }

    public float GetDensity(Vector3 position)
    {
        float time = SimulationManager.Instance.simulationTime;

        // --- strong directional drift (visible movement)
        Vector3 drift = new Vector3(
            time * 8f,
            0,
            time * 5f
        );

        // --- turbulence (adds organic motion)
        float turbulence = DeterministicNoise.Sample3D(
            position.x * 0.001f + time * 0.2f,
            position.y * 0.001f,
            position.z * 0.001f + time * 0.2f,
            seedManager.SeedValue + 999
        );

        Vector3 warpedTime = drift + new Vector3(
            turbulence * 20f,
            0,
            turbulence * 20f
        );

        Vector3 p = position + warpedTime;

        // --- spatial warp (break uniformity)
        float warp = DeterministicNoise.Sample3D(
            p.x * 0.0008f,
            p.y * 0.0008f,
            p.z * 0.0008f,
            seedManager.SeedValue + 500
        );

        Vector3 warped = p + new Vector3(warp * 50f, 0, warp * 50f);

        // --- multi-scale structure
        float large = DeterministicNoise.Sample3D(
            warped.x * 0.0003f,
            warped.y * 0.0003f,
            warped.z * 0.0003f,
            seedManager.SeedValue
        );

        float medium = DeterministicNoise.Sample3D(
            warped.x * 0.002f,
            warped.y * 0.002f,
            warped.z * 0.002f,
            seedManager.SeedValue + 17
        );

        float small = DeterministicNoise.Sample3D(
            warped.x * 0.01f,
            warped.y * 0.01f,
            warped.z * 0.01f,
            seedManager.SeedValue + 99
        );

        float density =
            (large * 0.6f) +
            (medium * 0.3f) +
            (small * 0.1f);

        // sharper cluster definition
        density = Mathf.Pow(density, 4f);

        return Mathf.Clamp01(density * 1.5f * constantsManager.constants.gravity);
    }

    public float GetRadiation(Vector3 position)
    {
        float noise = DeterministicNoise.Sample3D(
            position.x * 0.002f,
            position.y * 0.002f,
            position.z * 0.002f,
            seedManager.SeedValue + 42
        );

        return noise * constantsManager.constants.electromagnetism;
    }

    public float GetEntropy(Vector3 position)
    {
        float noise = DeterministicNoise.Sample3D(
            position.x * 0.0005f,
            position.y * 0.0005f,
            position.z * 0.0005f,
            seedManager.SeedValue + 99
        );

        return noise * constantsManager.constants.entropy;
    }

    public float GetLifeProbability(Vector3 position)
    {
        float density = GetDensity(position);
        float radiation = GetRadiation(position);
        float entropy = GetEntropy(position);

        float balance =
            Mathf.Clamp01(1f - Mathf.Abs(radiation - 0.5f)) *
            Mathf.Clamp01(1f - Mathf.Abs(entropy - 0.5f));

        return density * balance;
    }
}