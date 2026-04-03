using UnityEngine;

public enum GameMode
{
    Observatory,
    Probe
}

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    [SerializeField] private float fixedStep = 0.02f;

    public float SimulationTime { get; private set; }
    public GameMode CurrentMode { get; private set; } = GameMode.Observatory;

    public SeedManager SeedManager { get; private set; }
    public ConstantsManager ConstantsManager { get; private set; }
    public UniverseFieldSampler FieldSampler { get; private set; }

    private float accumulatedRealTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SeedManager = GetComponent<SeedManager>();
        ConstantsManager = GetComponent<ConstantsManager>();
        FieldSampler = GetComponent<UniverseFieldSampler>();
    }

    private void Update()
    {
        float entropyScale = Mathf.Max(0.05f, ConstantsManager.Constants.entropy);
        accumulatedRealTime += Time.unscaledDeltaTime * entropyScale;

        while (accumulatedRealTime >= fixedStep)
        {
            accumulatedRealTime -= fixedStep;
            SimulationTime += fixedStep;
        }
    }

    public UniverseSample Sample(Vector3 position)
    {
        return FieldSampler.Sample(position, SimulationTime);
    }

    public Vector3 GetDeterministicProbeSpawn()
    {
        int baseSeed = SeedManager.SeedValue;
        int timeSlice = Mathf.FloorToInt(SimulationTime * 10f);

        float x = (DeterministicNoise.Hash01(baseSeed, timeSlice, 1, 101) * 2f - 1f) * 250f;
        float y = (DeterministicNoise.Hash01(baseSeed, timeSlice, 2, 103) * 2f - 1f) * 80f;
        float z = (DeterministicNoise.Hash01(baseSeed, timeSlice, 3, 107) * 2f - 1f) * 250f;

        return new Vector3(x, y, z);
    }

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;
    }
}
