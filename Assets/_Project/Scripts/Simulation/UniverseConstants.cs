using UnityEngine;

[System.Serializable]
public class UniverseConstants
{
    [Range(0.1f, 10f)] public float gravity = 1.0f;
    [Range(0.1f, 10f)] public float strongForce = 1.0f;
    [Range(0.1f, 10f)] public float electromagnetism = 1.0f;
    [Range(0.0f, 5.0f)] public float entropy = 1.0f;
    [Range(0.0f, 5.0f)] public float darkEnergy = 1.0f;
}
