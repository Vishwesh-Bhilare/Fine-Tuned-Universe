using System.Text;
using UnityEngine;

public class SeedManager : MonoBehaviour
{
    [SerializeField] private string seed = "A1B2C3";

    public string Seed => seed;
    public int SeedValue { get; private set; }

    private void Awake()
    {
        seed = NormalizeSeed(seed);
        SeedValue = DeterministicNoise.HashToInt(seed);
        Debug.Log($"Seed Initialized: {seed} -> {SeedValue}");
    }

    public void SetSeed(string value)
    {
        seed = NormalizeSeed(value);
        SeedValue = DeterministicNoise.HashToInt(seed);
    }

    private static string NormalizeSeed(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "A1B2C3";
        }

        StringBuilder filtered = new StringBuilder(6);
        foreach (char c in input.ToUpperInvariant())
        {
            if (char.IsLetterOrDigit(c))
            {
                filtered.Append(c);
            }

            if (filtered.Length == 6)
            {
                break;
            }
        }

        while (filtered.Length < 6)
        {
            filtered.Append('0');
        }

        return filtered.ToString();
    }
}
