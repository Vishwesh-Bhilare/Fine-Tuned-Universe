using UnityEngine;

[System.Serializable]
public struct UniverseConstants
{
    [Range(0.1f, 10f)] public float gravity;
    [Range(0.1f, 10f)] public float strongForce;
    [Range(0.1f, 10f)] public float electromagnetism;
    [Range(0f, 5f)]    public float entropy;
    [Range(0f, 5f)]    public float darkEnergy;

    public static UniverseConstants Default => new UniverseConstants
    {
        gravity          = 1f,
        strongForce      = 1f,
        electromagnetism = 1f,
        entropy          = 1f,
        darkEnergy       = 1f
    };

    public readonly float StabilityBalance
    {
        get
        {
            float gravVsDark  = 1f - Mathf.Clamp01(Mathf.Abs(gravity - darkEnergy) / 10f);
            float forceVsEm   = 1f - Mathf.Clamp01(Mathf.Abs(strongForce - electromagnetism) / 10f);
            float entropyPenalty = 1f - Mathf.Clamp01(entropy / 5f);
            return gravVsDark * 0.4f + forceVsEm * 0.4f + entropyPenalty * 0.2f;
        }
    }
}
