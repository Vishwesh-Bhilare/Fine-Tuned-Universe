using UnityEngine;

public enum PlanetArchetype
{
    Habitable,
    Sterile,
    Volatile,
    Collapsed,
    ThinExiled
}

public struct UniverseSample
{
    public Vector3 position;
    public float time;
    public float density;
    public float radiation;
    public float entropy;
    public float lifeProbability;
    public float gravityPotential;
    public PlanetArchetype planetArchetype;
}

public struct ScanResult
{
    public UniverseSample sample;
    public float signalStrength;
    public string signature;
    public string scanType;
}
