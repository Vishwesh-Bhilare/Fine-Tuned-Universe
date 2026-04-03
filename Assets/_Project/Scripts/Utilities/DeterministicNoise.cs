using UnityEngine;

public static class DeterministicNoise
{
    public static float Sample(float x, float y, int seed)
    {
        float nx = x + seed * 0.01f;
        float ny = y + seed * 0.01f;

        return Mathf.PerlinNoise(nx, ny);
    }

    public static float Sample3D(float x, float y, float z, int seed)
    {
        float xy = Sample(x, y, seed);
        float yz = Sample(y, z, seed);
        float zx = Sample(z, x, seed);

        return (xy + yz + zx) / 3f;
    }
}
