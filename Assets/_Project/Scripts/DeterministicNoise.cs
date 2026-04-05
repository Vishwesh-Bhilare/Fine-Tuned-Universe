using UnityEngine;

public static class DeterministicNoise
{
    public static float Sample(float x, float y, int seed)
    {
        float nx = x + seed * 0.0001f;
        float ny = y + seed * 0.0001f;
        return Mathf.PerlinNoise(nx, ny);
    }

    public static float Sample3D(float x, float y, float z, int seed)
    {
        float xy = Sample(x, y, seed);
        float yz = Sample(y, z, seed + 17);
        float zx = Sample(z, x, seed + 31);
        return (xy + yz + zx) / 3f;
    }

    public static int HashToInt(string value)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in value)
                hash = hash * 31 + c;
            return Mathf.Abs(hash);
        }
    }

    public static float Hash01(int a, int b, int c, int d)
    {
        unchecked
        {
            uint h = 2166136261u;
            h = (h ^ (uint)a) * 16777619u;
            h = (h ^ (uint)b) * 16777619u;
            h = (h ^ (uint)c) * 16777619u;
            h = (h ^ (uint)d) * 16777619u;
            return (h & 0x00FFFFFFu) / (float)0x01000000;
        }
    }

    public static Vector3 HashDirection(int a, int b)
    {
        float x = Hash01(a, b, 11, 97)  * 2f - 1f;
        float y = Hash01(a, b, 23, 193) * 2f - 1f;
        float z = Hash01(a, b, 47, 389) * 2f - 1f;
        Vector3 dir = new Vector3(x, y, z);
        return dir.sqrMagnitude < 0.0001f ? Vector3.forward : dir.normalized;
    }
}
