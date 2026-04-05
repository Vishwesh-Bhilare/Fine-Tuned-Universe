using UnityEngine;

/// <summary>
/// Master game controller. Owns mode transitions, wires probe systems together,
/// and ensures all runtime references are satisfied even if nothing is pre-assigned in the Inspector.
/// </summary>
public class UniverseGameController : MonoBehaviour
{
    // ── Inspector references (all optional — auto-created if missing) ─────────
    [Header("Cameras")]
    [SerializeField] private Camera              observatoryCamera;
    [SerializeField] private Camera              probeCamera;

    [Header("Controllers")]
    [SerializeField] private ObservatoryController observatoryController;
    [SerializeField] private ProbeController       probeController;
    [SerializeField] private ProbeCamera           probeCameraScript;

    [Header("Probe Systems")]
    [SerializeField] private ScanSystem            scanSystem;
    [SerializeField] private SignalSystem          signalSystem;
    [SerializeField] private LocalInfluenceSystem  influenceSystem;
    [SerializeField] private FeedbackSystem        feedbackSystem;
    [SerializeField] private WhisperSystem         whisperSystem;

    [Header("Meta Systems")]
    [SerializeField] private CodexManager          codexManager;
    [SerializeField] private PersistenceManager    persistenceManager;

    [Header("HUDs")]
    [SerializeField] private ObservatoryHUD        observatoryHUD;
    [SerializeField] private ProbeHUD              probeHUD;

    private bool probeActive;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Start()
    {
        EnsureSystems();
        EnsureCameras();
        EnsureProbe();
        EnsureHUDs();
        WireProbeReferences();

        if (scanSystem != null)
            scanSystem.OnScanCompleted += HandleScanCompleted;

        // Try loading previous run
        if (persistenceManager != null && persistenceManager.TryLoad(out _))
            Debug.Log("[GameController] Previous run loaded.");

        SetProbeMode(false);
    }

    private void OnDestroy()
    {
        if (scanSystem != null)
            scanSystem.OnScanCompleted -= HandleScanCompleted;
    }

    private void Update()
    {
        if (SimulationManager.Instance == null) return;

        // Toggle probe mode
        if (GameInput.GetKeyDown(KeyCode.P))
            SetProbeMode(!probeActive);

        // Signal frequency tuning (Q/E)
        if (probeActive && signalSystem != null)
        {
            float delta = 0f;
            if (GameInput.GetKey(KeyCode.Q)) delta -= 0.3f;
            if (GameInput.GetKey(KeyCode.E)) delta += 0.3f;
            if (Mathf.Abs(delta) > 0f)
                signalSystem.SetTuneFrequency(signalSystem.TuneFrequency + delta * Time.deltaTime);
        }
    }

    // ── Mode transition ───────────────────────────────────────────────────────
    private void SetProbeMode(bool active)
    {
        probeActive = active;

        // Observatory elements
        if (observatoryController != null) observatoryController.enabled = !active;
        if (observatoryCamera     != null) observatoryCamera.enabled     = !active;
        if (observatoryHUD        != null) observatoryHUD.enabled        = !active;

        // Probe elements
        if (probeController != null)
        {
            if (active)
                probeController.transform.position = SimulationManager.Instance.GetDeterministicProbeSpawn();
            probeController.gameObject.SetActive(active);
        }
        if (probeCamera       != null) probeCamera.enabled = active;
        if (probeHUD          != null) probeHUD.enabled    = active;

        // Whisper always active, but listener switches
        if (whisperSystem != null)
        {
            Transform listener = active && probeController != null
                ? probeController.transform
                : (observatoryCamera != null ? observatoryCamera.transform : transform);
            whisperSystem.SetListener(listener);
        }

        SimulationManager.Instance.SetMode(active ? GameMode.Probe : GameMode.Observatory);
        Debug.Log($"[GameController] Mode -> {(active ? "Probe" : "Observatory")}");
    }

    // ── Scan callback ─────────────────────────────────────────────────────────
    private void HandleScanCompleted(ScanResult result)
    {
        codexManager?.RegisterScan(result);
        probeHUD?.SetLastScan(result);
    }

    // ── Auto-create missing systems ───────────────────────────────────────────
    private void EnsureSystems()
    {
        if (codexManager     == null) codexManager     = GetOrAdd<CodexManager>();
        if (persistenceManager == null) persistenceManager = GetOrAdd<PersistenceManager>();
        if (whisperSystem    == null) whisperSystem    = GetOrAdd<WhisperSystem>();
        if (scanSystem       == null) scanSystem       = GetOrAdd<ScanSystem>();
        if (signalSystem     == null) signalSystem     = GetOrAdd<SignalSystem>();
        if (influenceSystem  == null) influenceSystem  = GetOrAdd<LocalInfluenceSystem>();
        if (feedbackSystem   == null) feedbackSystem   = GetOrAdd<FeedbackSystem>();
    }

    private void EnsureCameras()
    {
        // Observatory camera — reuse Main Camera if present
        if (observatoryCamera == null)
        {
            observatoryCamera = Camera.main;
            if (observatoryCamera == null)
            {
                var go = new GameObject("ObservatoryCamera");
                observatoryCamera = go.AddComponent<Camera>();
                go.tag = "MainCamera";
                go.AddComponent<AudioListener>();
            }
        }

        if (observatoryController == null)
            observatoryController = observatoryCamera.gameObject.GetComponent<ObservatoryController>()
                                 ?? observatoryCamera.gameObject.AddComponent<ObservatoryController>();

        // Probe camera
        if (probeCamera == null)
        {
            var go = new GameObject("ProbeCamera");
            probeCamera = go.AddComponent<Camera>();
        }

        if (probeCameraScript == null)
            probeCameraScript = probeCamera.gameObject.GetComponent<ProbeCamera>()
                             ?? probeCamera.gameObject.AddComponent<ProbeCamera>();

        probeCamera.gameObject.SetActive(false);
    }

    private void EnsureProbe()
    {
        if (probeController != null) return;

        var probeGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        probeGo.name                     = "Probe";
        probeGo.transform.localScale     = Vector3.one * 0.8f;

        var rb   = probeGo.AddComponent<Rigidbody>();
        rb.mass  = 1f;

        probeController = probeGo.AddComponent<ProbeController>();

        // Probe light
        var lightGo     = new GameObject("ProbeLight");
        lightGo.transform.SetParent(probeGo.transform, false);
        var pLight      = lightGo.AddComponent<Light>();
        pLight.type     = LightType.Point;
        pLight.range    = 14f;
        pLight.intensity = 1.4f;
        pLight.color    = Color.cyan;

        feedbackSystem?.SetProbeLight(pLight);

        probeCameraScript?.SetTarget(probeGo.transform);
    }

    private void EnsureHUDs()
    {
        if (observatoryHUD == null)
        {
            var go = new GameObject("ObservatoryHUD");
            go.transform.SetParent(transform, false);
            observatoryHUD = go.AddComponent<ObservatoryHUD>();
        }

        if (probeHUD == null)
        {
            var go = new GameObject("ProbeHUD");
            go.transform.SetParent(transform, false);
            probeHUD = go.AddComponent<ProbeHUD>();
        }
    }

    private void WireProbeReferences()
    {
        Transform probeT = probeController != null ? probeController.transform : null;

        scanSystem?.SetProbe(probeT);
        signalSystem?.SetProbe(probeT);
        influenceSystem?.SetProbe(probeT);
        feedbackSystem?.SetProbe(probeT);

        // Wire HUD references
        if (probeHUD != null)
        {
            var hud = probeHUD;
            // Use reflection-free field wiring via serialized property approach —
            // instead set them via public setters (add below if needed).
        }
    }

    private T GetOrAdd<T>() where T : Component
    {
        var c = GetComponent<T>();
        return c != null ? c : gameObject.AddComponent<T>();
    }
}
