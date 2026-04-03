using System.Text;
using UnityEngine;

public class UniverseGameController : MonoBehaviour
{
    [Header("Runtime References")]
    [SerializeField] private ProbeController probeController;
    [SerializeField] private ProbeCamera probeCamera;
    [SerializeField] private ObservatoryController observatoryController;

    [Header("Probe Runtime Systems")]
    [SerializeField] private ScanSystem scanSystem;
    [SerializeField] private SignalSystem signalSystem;
    [SerializeField] private LocalInfluenceSystem localInfluenceSystem;
    [SerializeField] private FeedbackSystem feedbackSystem;
    [SerializeField] private WhisperSystem whisperSystem;

    private bool probeActive;
    private float tuneFrequency = 0.5f;
    private ScanResult? lastScan;
    private readonly StringBuilder telemetryBuilder = new StringBuilder(256);

    private void Start()
    {
        EnsureCameras();
        EnsureProbe();
        EnsureRuntimeSystems();

        if (scanSystem != null)
        {
            scanSystem.OnScanCompleted += HandleScanCompleted;
        }

        SetProbeMode(false);
    }

    private void OnDestroy()
    {
        if (scanSystem != null)
        {
            scanSystem.OnScanCompleted -= HandleScanCompleted;
        }
    }

    private void Update()
    {
        if (SimulationManager.Instance == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            SetProbeMode(!probeActive);
        }

        if (probeActive && signalSystem != null)
        {
            float input = 0f;
            if (Input.GetKey(KeyCode.Q)) input -= 0.35f;
            if (Input.GetKey(KeyCode.E)) input += 0.35f;

            if (Mathf.Abs(input) > 0f)
            {
                tuneFrequency = Mathf.Clamp01(tuneFrequency + input * Time.deltaTime);
                signalSystem.SetTuneFrequency(tuneFrequency);
            }
        }
    }

    private void OnGUI()
    {
        if (SimulationManager.Instance == null)
        {
            return;
        }

        const int panelWidth = 360;
        GUI.Box(new Rect(16f, 16f, panelWidth, probeActive ? 360f : 460f), string.Empty);

        GUILayout.BeginArea(new Rect(28f, 28f, panelWidth - 24f, probeActive ? 340f : 440f));
        GUILayout.Label("FINE-TUNED UNIVERSE", HeaderStyle());
        GUILayout.Label($"Seed: {SimulationManager.Instance.SeedManager.Seed} | t={SimulationManager.Instance.SimulationTime:0.0}");

        if (!probeActive)
        {
            DrawObservatoryUi();
        }
        else
        {
            DrawProbeUi();
        }

        GUILayout.EndArea();
    }

    private void DrawObservatoryUi()
    {
        ConstantsManager constantsManager = SimulationManager.Instance.ConstantsManager;
        UniverseConstants c = constantsManager.Constants;

        GUILayout.Space(10f);
        GUILayout.Label("Observatory Mode");
        GUILayout.Label("[P] Launch Probe | Mouse Wheel Zoom");

        DrawConstantSlider("Gravity", c.gravity, constantsManager.SetGravity);
        DrawConstantSlider("Strong Nuclear Force", c.strongForce, constantsManager.SetStrongForce);
        DrawConstantSlider("Electromagnetism", c.electromagnetism, constantsManager.SetElectromagnetism);
        DrawConstantSlider("Entropy", c.entropy, constantsManager.SetEntropy);
        DrawConstantSlider("Dark Energy", c.darkEnergy, constantsManager.SetDarkEnergy);

        UniverseSample origin = SimulationManager.Instance.Sample(Vector3.zero);
        GUILayout.Space(8f);
        GUILayout.Label($"Local Archetype: {origin.planetArchetype}");
        GUILayout.Label($"Life Probability: {origin.lifeProbability:0.00}  |  Density: {origin.density:0.00}");
        GUILayout.Label($"Whisper: \"{(whisperSystem != null ? whisperSystem.LastLine : "...")}\"");
    }

    private void DrawProbeUi()
    {
        GUILayout.Space(10f);
        GUILayout.Label("Probe Active");
        GUILayout.Label("[P] Return | [SPACE] Scan | [TAB] Movement Mode | [1/2/3] Influence | [Q/E] Tune");

        if (probeController != null)
        {
            telemetryBuilder.Clear();
            telemetryBuilder.Append("Movement Mode: ").Append(probeController.Mode);
            telemetryBuilder.Append("\nVelocity: ").Append(probeController.GetComponent<Rigidbody>().velocity.magnitude.ToString("0.0"));
            GUILayout.Label(telemetryBuilder.ToString());
        }

        if (signalSystem != null && signalSystem.TryGetSignalDirection(out Vector3 direction))
        {
            GUILayout.Label($"Signal Lock: {direction} | Freq: {tuneFrequency:0.00}");
        }
        else
        {
            GUILayout.Label("Signal Lock: none");
        }

        if (localInfluenceSystem != null)
        {
            GUILayout.Label($"Influence: {localInfluenceSystem.ActiveType} ({localInfluenceSystem.TimeRemaining():0.0}s)");
        }

        if (lastScan.HasValue)
        {
            ScanResult scan = lastScan.Value;
            GUILayout.Space(8f);
            GUILayout.Label("Last Scan");
            GUILayout.Label(scan.signature);
            GUILayout.Label($"Entropy {scan.sample.entropy:0.00} | Radiation {scan.sample.radiation:0.00} | Life {scan.sample.lifeProbability:0.00}");
        }

        GUILayout.Space(8f);
        GUILayout.Label($"Whisper: \"{(whisperSystem != null ? whisperSystem.LastLine : "...")}\"");
    }

    private static GUIStyle HeaderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16
        };
        return style;
    }

    private static void DrawConstantSlider(string label, float value, System.Action<float> setter)
    {
        GUILayout.Label($"{label}: {value:0.00}");
        float next = GUILayout.HorizontalSlider(value, 0.1f, 10f);
        if (!Mathf.Approximately(next, value))
        {
            setter(next);
        }
    }

    private void HandleScanCompleted(ScanResult result)
    {
        lastScan = result;
        CodexManager codex = SimulationManager.Instance.GetComponent<CodexManager>();
        codex?.RegisterScan(result);
    }

    private void SetProbeMode(bool active)
    {
        probeActive = active;

        if (observatoryController != null)
        {
            observatoryController.enabled = !active;
        }

        if (probeController != null)
        {
            if (active)
            {
                probeController.transform.position = SimulationManager.Instance.GetDeterministicProbeSpawn();
            }

            probeController.gameObject.SetActive(active);
        }

        if (probeCamera != null)
        {
            probeCamera.gameObject.SetActive(active);
        }

        if (active)
        {
            SimulationManager.Instance.SetMode(GameMode.Probe);
        }
        else
        {
            SimulationManager.Instance.SetMode(GameMode.Observatory);
        }
    }

    private void EnsureCameras()
    {
        if (observatoryController == null)
        {
            Camera observatoryCam = Camera.main;
            if (observatoryCam == null)
            {
                GameObject camGo = new GameObject("ObservatoryCamera");
                observatoryCam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }

            observatoryController = observatoryCam.gameObject.GetComponent<ObservatoryController>();
            if (observatoryController == null)
            {
                observatoryController = observatoryCam.gameObject.AddComponent<ObservatoryController>();
            }
        }

        if (probeCamera == null)
        {
            GameObject probeCamGo = new GameObject("ProbeCamera");
            Camera probeCam = probeCamGo.AddComponent<Camera>();
            probeCam.enabled = true;
            probeCamera = probeCamGo.AddComponent<ProbeCamera>();
        }

        if (probeCamera != null)
        {
            probeCamera.gameObject.SetActive(false);
        }
    }

    private void EnsureProbe()
    {
        if (probeController != null)
        {
            return;
        }

        GameObject probe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        probe.name = "Probe";
        probe.transform.localScale = Vector3.one * 0.8f;

        Rigidbody rb = probe.AddComponent<Rigidbody>();
        rb.mass = 1f;

        probeController = probe.AddComponent<ProbeController>();

        if (probeCamera != null)
        {
            probeCamera.SetTarget(probe.transform);
        }
    }

    private void EnsureRuntimeSystems()
    {
        if (scanSystem == null)
        {
            scanSystem = gameObject.GetComponent<ScanSystem>() ?? gameObject.AddComponent<ScanSystem>();
        }

        if (signalSystem == null)
        {
            signalSystem = gameObject.GetComponent<SignalSystem>() ?? gameObject.AddComponent<SignalSystem>();
        }

        if (localInfluenceSystem == null)
        {
            localInfluenceSystem = gameObject.GetComponent<LocalInfluenceSystem>() ?? gameObject.AddComponent<LocalInfluenceSystem>();
        }

        if (feedbackSystem == null)
        {
            feedbackSystem = gameObject.GetComponent<FeedbackSystem>() ?? gameObject.AddComponent<FeedbackSystem>();
        }

        if (whisperSystem == null)
        {
            whisperSystem = gameObject.GetComponent<WhisperSystem>() ?? gameObject.AddComponent<WhisperSystem>();
        }

        Transform probe = probeController != null ? probeController.transform : null;
        scanSystem.SetProbe(probe);
        signalSystem.SetProbe(probe);
        localInfluenceSystem.SetProbe(probe);
        feedbackSystem.SetProbe(probe);
        whisperSystem.SetListener(probe);

        if (feedbackSystem != null && probe != null)
        {
            Light probeLight = probe.GetComponentInChildren<Light>();
            if (probeLight == null)
            {
                GameObject lightGo = new GameObject("ProbeLight");
                lightGo.transform.SetParent(probe, false);
                lightGo.transform.localPosition = Vector3.zero;
                probeLight = lightGo.AddComponent<Light>();
                probeLight.type = LightType.Point;
                probeLight.range = 12f;
                probeLight.intensity = 1.2f;
            }

            feedbackSystem.SetProbeLight(probeLight);
        }
    }
}
