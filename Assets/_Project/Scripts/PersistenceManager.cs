using System.IO;
using UnityEngine;

public class PersistenceManager : MonoBehaviour
{
    [System.Serializable]
    private struct SaveData
    {
        public string          seed;
        public float           simulationTime;
        public UniverseConstants constants;
        public int             codexCount;
    }

    [SerializeField] private string fileName = "universe_save.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    public void Save()
    {
        if (SimulationManager.Instance == null) return;

        var data = new SaveData
        {
            seed           = SimulationManager.Instance.SeedManager.Seed,
            simulationTime = SimulationManager.Instance.SimulationTime,
            constants      = SimulationManager.Instance.ConstantsManager.Constants,
            codexCount     = CodexManager.Instance?.EntryCount ?? 0
        };

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log($"[Persistence] Saved to {SavePath}");
    }

    public bool TryLoad(out string loadedSeed)
    {
        loadedSeed = string.Empty;
        if (!File.Exists(SavePath) || SimulationManager.Instance == null) return false;

        var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        SimulationManager.Instance.SeedManager.SetSeed(data.seed);
        SimulationManager.Instance.ConstantsManager.ApplyConstants(data.constants);
        loadedSeed = data.seed;
        Debug.Log($"[Persistence] Loaded seed={data.seed}  codex={data.codexCount}");
        return true;
    }

    // Auto-save on quit
    private void OnApplicationQuit() => Save();
}
