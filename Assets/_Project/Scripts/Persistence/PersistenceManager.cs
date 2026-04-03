using System.IO;
using UnityEngine;

public class PersistenceManager : MonoBehaviour
{
    [System.Serializable]
    private struct SaveData
    {
        public string seed;
        public float simulationTime;
        public UniverseConstants constants;
        public int codexCount;
    }

    [SerializeField] private string saveFileName = "universe_save.json";
    [SerializeField] private CodexManager codexManager;

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public void Save()
    {
        if (SimulationManager.Instance == null) return;

        SaveData data = new SaveData
        {
            seed = SimulationManager.Instance.SeedManager.Seed,
            simulationTime = SimulationManager.Instance.SimulationTime,
            constants = SimulationManager.Instance.ConstantsManager.Constants,
            codexCount = codexManager != null ? codexManager.EntryCount : 0
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public bool TryLoad(out string loadedSeed)
    {
        loadedSeed = string.Empty;
        if (!File.Exists(SavePath) || SimulationManager.Instance == null)
        {
            return false;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        SimulationManager.Instance.SeedManager.SetSeed(data.seed);
        SimulationManager.Instance.ConstantsManager.ApplyConstants(data.constants);
        loadedSeed = data.seed;
        return true;
    }
}
