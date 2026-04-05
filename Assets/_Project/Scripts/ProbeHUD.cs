using System.Text;
using UnityEngine;

/// <summary>
/// Minimal probe HUD: Probe Active indicator, scan reticle, telemetry ring data, signal lock.
/// Drawn via OnGUI — no Canvas required.
/// </summary>
public class ProbeHUD : MonoBehaviour
{
    [SerializeField] private ProbeController     probeController;
    [SerializeField] private SignalSystem        signalSystem;
    [SerializeField] private LocalInfluenceSystem influenceSystem;
    [SerializeField] private ScanSystem          scanSystem;
    [SerializeField] private WhisperSystem       whisperSystem;
    [SerializeField] private CodexManager        codexManager;

    private ScanResult? lastScan;
    private readonly StringBuilder sb = new(256);

    // Colors
    private static readonly Color PanelBg      = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color AccentCyan   = new Color(0.3f, 0.9f, 0.85f, 1f);
    private static readonly Color AccentOrange = new Color(0.9f, 0.55f, 0.2f, 1f);
    private static readonly Color White70      = new Color(1f, 1f, 1f, 0.7f);

    public void SetLastScan(ScanResult r) => lastScan = r;

    private void OnGUI()
    {
        if (SimulationManager.Instance == null) return;
        if (SimulationManager.Instance.CurrentMode != GameMode.Probe) return;

        float sw = Screen.width;
        float sh = Screen.height;

        DrawProbeActiveIndicator(sw, sh);
        DrawReticle(sw, sh);
        DrawTelemetryPanel(sw, sh);
        DrawSignalLock(sw, sh);
        DrawWhisper(sw, sh);
        DrawInfluenceStatus(sw, sh);
    }

    // ── Top-centre: "PROBE ACTIVE" badge ─────────────────────────────────────
    private void DrawProbeActiveIndicator(float sw, float sh)
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = AccentCyan }
        };

        GUI.color = PanelBg;
        GUI.Box(new Rect(sw * 0.5f - 70f, 14f, 140f, 26f), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(sw * 0.5f - 70f, 14f, 140f, 26f), "◈  PROBE ACTIVE  ◈", style);
    }

    // ── Centre: scan reticle ──────────────────────────────────────────────────
    private void DrawReticle(float sw, float sh)
    {
        float cx = sw * 0.5f;
        float cy = sh * 0.5f;
        float r  = 18f;

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 20,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = new Color(1f, 1f, 1f, 0.55f) }
        };

        // Cross-hair
        GUI.Label(new Rect(cx - 20f, cy - 20f, 40f, 40f), "⊕", style);

        // Scan range arc hint
        if (scanSystem != null)
        {
            var hint = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 10,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(0.3f, 0.9f, 0.85f, 0.45f) }
            };
            GUI.Label(new Rect(cx - 60f, cy + 26f, 120f, 18f), "[SPACE] scan", hint);
        }
    }

    // ── Bottom-left: telemetry ring (compact) ─────────────────────────────────
    private void DrawTelemetryPanel(float sw, float sh)
    {
        if (probeController == null || SimulationManager.Instance == null) return;

        UniverseSample s = SimulationManager.Instance.Sample(probeController.transform.position);

        float panH = 140f;
        Rect  panel = new Rect(14f, sh - panH - 14f, 230f, panH);

        GUI.color = PanelBg;
        GUI.Box(panel, GUIContent.none);
        GUI.color = Color.white;

        var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = White70 } };
        var valueStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold,
                                                         normal  = { textColor = AccentCyan } };

        float lx = panel.x + 10f;
        float rx = panel.x + 130f;
        float ty = panel.y + 10f;
        float ls = 22f;

        GUI.Label(new Rect(lx, ty,       100f, 18f), "DENSITY",           labelStyle);
        GUI.Label(new Rect(rx, ty,       90f,  18f), s.density.ToString("0.00"),        valueStyle);
        GUI.Label(new Rect(lx, ty + ls,  100f, 18f), "RADIATION",         labelStyle);
        GUI.Label(new Rect(rx, ty + ls,  90f,  18f), s.radiation.ToString("0.00"),      valueStyle);
        GUI.Label(new Rect(lx, ty+ls*2,  100f, 18f), "ENTROPY",           labelStyle);
        GUI.Label(new Rect(rx, ty+ls*2,  90f,  18f), s.entropy.ToString("0.00"),        valueStyle);
        GUI.Label(new Rect(lx, ty+ls*3,  100f, 18f), "LIFE P.",           labelStyle);
        GUI.Label(new Rect(rx, ty+ls*3,  90f,  18f), s.lifeProbability.ToString("0.00"), valueStyle);
        GUI.Label(new Rect(lx, ty+ls*4,  100f, 18f), "MODE",              labelStyle);
        GUI.Label(new Rect(rx, ty+ls*4,  90f,  18f), probeController.Mode.ToString(),  valueStyle);

        // Last scan
        if (lastScan.HasValue)
        {
            var scanStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.85f, 0.5f, 0.85f) } };
            GUI.Label(new Rect(lx, ty + ls * 5, 210f, 36f),
                $"LAST SCAN: {lastScan.Value.signature}", scanStyle);
        }
    }

    // ── Right-centre: signal lock ─────────────────────────────────────────────
    private void DrawSignalLock(float sw, float sh)
    {
        if (signalSystem == null) return;

        bool locked = signalSystem.TryGetSignalDirection(out _, out string sigType);
        float px = sw - 200f;
        float py = sh * 0.5f - 40f;

        GUI.color = PanelBg;
        GUI.Box(new Rect(px, py, 186f, 80f), GUIContent.none);
        GUI.color = Color.white;

        var hdrStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 11,
            fontStyle = FontStyle.Bold,
            normal    = { textColor = locked ? AccentOrange : White70 }
        };
        var subStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = White70 } };

        GUI.Label(new Rect(px + 10f, py + 8f,  160f, 18f),
            locked ? $"▶ SIGNAL  [{sigType.ToUpper()}]" : "  SIGNAL  [SEARCHING]", hdrStyle);

        // Frequency tuner visual
        float freq = signalSystem.TuneFrequency;
        GUI.Label(new Rect(px + 10f, py + 30f, 160f, 18f), $"FREQ  {freq:0.00}", subStyle);
        DrawBar(new Rect(px + 10f, py + 50f, 166f, 10f), freq, locked ? AccentOrange : new Color(0.4f, 0.4f, 0.5f, 0.8f));

        var hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 9, normal = { textColor = new Color(1,1,1,0.4f) } };
        GUI.Label(new Rect(px + 10f, py + 63f, 160f, 14f), "[Q/E] tune frequency", hintStyle);
    }

    // ── Bottom-centre: whisper line ───────────────────────────────────────────
    private void DrawWhisper(float sw, float sh)
    {
        if (whisperSystem == null) return;

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 12,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter,
            wordWrap  = true,
            normal    = { textColor = new Color(0.85f, 0.85f, 1f, 0.6f) }
        };

        GUI.Label(new Rect(sw * 0.5f - 200f, sh - 48f, 400f, 36f),
            $"\"{whisperSystem.LastLine}\"", style);
    }

    // ── Top-right: influence pulse status ────────────────────────────────────
    private void DrawInfluenceStatus(float sw, float sh)
    {
        if (influenceSystem == null) return;
        if (influenceSystem.ActiveType == LocalInfluenceSystem.InfluenceType.None) return;

        float rem = influenceSystem.TimeRemaining();
        float px  = sw - 200f;

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            normal   = { textColor = new Color(0.6f, 1f, 0.7f, 0.85f) }
        };

        GUI.color = PanelBg;
        GUI.Box(new Rect(px, 14f, 186f, 44f), GUIContent.none);
        GUI.color = Color.white;

        GUI.Label(new Rect(px + 10f, 18f, 170f, 18f),
            $"PULSE  {influenceSystem.ActiveType}", style);
        DrawBar(new Rect(px + 10f, 38f, 166f, 8f),
            rem / 8f, new Color(0.4f, 1f, 0.5f, 0.7f));
    }

    // ── Utility: simple horizontal progress bar ───────────────────────────────
    private static void DrawBar(Rect r, float t, Color col)
    {
        GUI.color = new Color(0f, 0f, 0f, 0.5f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);

        GUI.color = col;
        GUI.DrawTexture(new Rect(r.x, r.y, r.width * Mathf.Clamp01(t), r.height), Texture2D.whiteTexture);

        GUI.color = Color.white;
    }
}
