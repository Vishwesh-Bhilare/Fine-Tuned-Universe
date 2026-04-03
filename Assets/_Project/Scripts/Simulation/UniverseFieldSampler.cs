using UnityEngine;

public class UniverseFieldSampler : MonoBehaviour
{
    private SeedManager seedManager;
    private ConstantsManager constantsManager;

    private void Awake()
    {
        seedManager = GetComponent<SeedManager>();
        constantsManager = GetComponent<ConstantsManager>();
    }

    public UniverseSample Sample(Vector3 position, float simulationTime)
    {
        int seed = seedManager.SeedValue;
        UniverseConstants constants = constantsManager.Constants;

        float cosmicDrift = simulationTime * (0.02f + constants.darkEnergy * 0.01f);
        Vector3 drifted = position + new Vector3(cosmicDrift * 43f, cosmicDrift * 9f, cosmicDrift * 31f);

        float web = Mathf.Pow(DeterministicNoise.Sample3D(drifted.x * 0.00015f, drifted.y * 0.00015f, drifted.z * 0.00015f, seed + 100), 2.3f);
        float filament = DeterministicNoise.Sample3D(drifted.x * 0.0014f, drifted.y * 0.0014f, drifted.z * 0.0014f, seed + 330);
        float cluster = Mathf.Pow(DeterministicNoise.Sample3D(drifted.x * 0.004f, drifted.y * 0.004f, drifted.z * 0.004f, seed + 777), 3.5f);

        float density = Mathf.Clamp01((web * 0.55f + filament * 0.3f + cluster * 0.15f) * constants.gravity / (0.65f + constants.darkEnergy * 0.2f));

        float radiationCore = DeterministicNoise.Sample3D(position.x * 0.0025f, position.y * 0.0025f, position.z * 0.0025f, seed + 19);
        float radiation = Mathf.Clamp01(radiationCore * constants.electromagnetism * (0.6f + density));

        float entropyWave = DeterministicNoise.Sample3D(
            position.x * 0.001f + simulationTime * 0.01f,
            position.y * 0.001f + simulationTime * 0.008f,
            position.z * 0.001f + simulationTime * 0.012f,
            seed + 404);
        float entropy = Mathf.Clamp01(entropyWave * constants.entropy);

        float gravityPotential = Mathf.Clamp01(density * constants.gravity);

        float chemistryBalance = 1f - Mathf.Abs(constants.strongForce - constants.electromagnetism) / 10f;
        float thermalWindow = 1f - Mathf.Abs(radiation - 0.45f) * 1.8f;
        float turbulencePenalty = 1f - entropy;

        float lifeProbability = Mathf.Clamp01(
            density * 0.35f +
            Mathf.Clamp01(chemistryBalance) * 0.25f +
            Mathf.Clamp01(thermalWindow) * 0.25f +
            Mathf.Clamp01(turbulencePenalty) * 0.15f);

        return new UniverseSample
        {
            position = position,
            time = simulationTime,
            density = density,
            radiation = radiation,
            entropy = entropy,
            gravityPotential = gravityPotential,
            lifeProbability = lifeProbability,
            planetArchetype = ClassifyPlanet(constants, density, radiation, entropy, lifeProbability)
        };
    }

    public PlanetArchetype ClassifyPlanet(UniverseConstants constants, float density, float radiation, float entropy, float lifeProbability)
    {
        if (constants.gravity > 6f && density > 0.7f)
        {
            return PlanetArchetype.Collapsed;
        }

        if (constants.darkEnergy > 3.5f && density < 0.3f)
        {
            return PlanetArchetype.ThinExiled;
        }

        if (entropy > 0.72f)
        {
            return PlanetArchetype.Volatile;
        }

        if (lifeProbability > 0.58f && radiation > 0.25f && radiation < 0.65f)
        {
            return PlanetArchetype.Habitable;
        }

        return PlanetArchetype.Sterile;
    }
}
