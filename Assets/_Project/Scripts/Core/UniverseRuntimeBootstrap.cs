using UnityEngine;

public static class UniverseRuntimeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureRuntime()
    {
        if (Object.FindAnyObjectByType<SimulationManager>() != null)
        {
            return;
        }

        GameObject root = new GameObject("UniverseRuntime");
        root.AddComponent<SeedManager>();
        root.AddComponent<ConstantsManager>();
        root.AddComponent<UniverseFieldSampler>();
        root.AddComponent<CodexManager>();
        root.AddComponent<SimulationManager>();
        root.AddComponent<GalaxyVisualizer>();
        root.AddComponent<UniverseGameController>();
    }
}
