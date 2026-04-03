using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance;

    public float simulationTime { get; private set; }

    private SeedManager seedManager;
    private ConstantsManager constantsManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        seedManager = GetComponent<SeedManager>();
        constantsManager = GetComponent<ConstantsManager>();
    }

    void Update()
    {
        float entropyFactor = constantsManager.constants.entropy;

        simulationTime += Time.deltaTime * entropyFactor;
    }
}