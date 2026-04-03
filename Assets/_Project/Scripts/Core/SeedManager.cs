using UnityEngine;

public class SeedManager : MonoBehaviour
{
    [SerializeField] private string seed = "A1B2C3";

    public int SeedValue { get; private set; }

    void Awake()
    {
        SeedValue = ConvertSeedToInt(seed);
        Debug.Log("Seed Initialized: " + seed + " -> " + SeedValue);
    }

    private int ConvertSeedToInt(string seed)
    {
        int hash = 17;

        foreach (char c in seed)
        {
            hash = hash * 31 + c;
        }

        return Mathf.Abs(hash);
    }
}
