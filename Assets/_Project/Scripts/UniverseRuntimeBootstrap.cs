using UnityEngine;

/// <summary>
/// Ensures the full simulation stack exists at runtime, even if the Bootstrap scene
/// is not loaded first. Safe to call multiple times — guards against duplicates.
/// </summary>
public static class UniverseRuntimeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureRuntime()
    {
        if (Object.FindAnyObjectByType<SimulationManager>() != null) return;

        var root = new GameObject("UniverseRuntime_[AUTO]");
        Object.DontDestroyOnLoad(root);

        root.AddComponent<SeedManager>();
        root.AddComponent<ConstantsManager>();
        root.AddComponent<UniverseFieldSampler>();
        root.AddComponent<GalaxySampler>();
        root.AddComponent<SimulationManager>();
        root.AddComponent<GalaxyVisualizer>();
        root.AddComponent<CodexManager>();
        root.AddComponent<PersistenceManager>();
        root.AddComponent<WhisperSystem>();
        root.AddComponent<UniverseGameController>();

        Debug.Log("[Bootstrap] Auto-created UniverseRuntime root.");
    }
}
