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
        var existingSim = Object.FindAnyObjectByType<SimulationManager>();
        GameObject root;

        if (existingSim == null)
        {
            root = new GameObject("UniverseRuntime_[AUTO]");
            Object.DontDestroyOnLoad(root);
            Debug.Log("[Bootstrap] Auto-created UniverseRuntime root.");
        }
        else
        {
            root = existingSim.gameObject;
            Object.DontDestroyOnLoad(root);
            Debug.Log("[Bootstrap] Using existing SimulationManager root.");
        }

        EnsureComponent<SeedManager>(root);
        EnsureComponent<ConstantsManager>(root);
        EnsureComponent<UniverseFieldSampler>(root);
        EnsureComponent<GalaxySampler>(root);
        EnsureComponent<SimulationManager>(root);
        EnsureComponent<GalaxyVisualizer>(root);
        EnsureComponent<CodexManager>(root);
        EnsureComponent<PersistenceManager>(root);
        EnsureComponent<WhisperSystem>(root);
        EnsureComponent<UniverseGameController>(root);
    }

    private static T EnsureComponent<T>(GameObject root) where T : Component
    {
        var c = root.GetComponent<T>();
        if (c != null) return c;
        return root.AddComponent<T>();
    }
}
