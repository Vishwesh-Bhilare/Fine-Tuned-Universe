using System.Text;
using UnityEngine;

/// <summary>
/// Observatory Mode HUD: constant sliders, universe snapshot, codex count, whisper.
/// All drawn with OnGUI — no Canvas required.
/// </summary>
public class ObservatoryHUD : MonoBehaviour
{
    [SerializeField] private WhisperSystem  whisperSystem;
    [SerializeField] private CodexManager   codexManager;
    [SerializeField] private PersistenceManager persistence;

    private string seedInputBuffer = "";
    private bool   editingSeed     = false;

    private static readonly Color PanelBg    = new Color(0f, 0f, 0f, 0.60f);
    private static readonly Color AccentCyan = new Color(0.3f, 0.9f, 0.85f, 1f);
    private static readonly Color White80    = new Color(1f, 1f, 1f, 0.8f);

    private void OnGUI()
    {
        if (SimulationManager.Instance == null) return;
        if (SimulationManager.Instance.CurrentMode != GameMode.Observatory) return;

        float sw = Screen.width;
        float sh = Screen.height;

        DrawConstantsPanel(sw, sh);
        DrawUniverseSnapshot(sw, sh);
        DrawSeedControls(sw, sh);
        DrawWhisper(sw, sh);
        DrawHelp(sw, sh);
    }

    // ── Left panel: constant sliders ─────────────────────────────────────────
    private void DrawConstantsPanel(float sw, float sh)
    {
        ConstantsManager cm = SimulationManager.Instance.ConstantsManager;
        if (cm == null) return;

        UniverseConstants c = cm.Constants;
        float panH = 290f;
        Rect  panel = new Rect(14f, sh * 0.5f - panH * 0.5f, 270f, panH);

        GUI.color = PanelBg;
        GUI.Box(panel, GUIContent.none);
        GUI.color = Color.white;

        var title = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13,
            fontStyle = FontStyle.Bold,
            normal    = { textColor = AccentCyan }
        };

        float lx = panel.x + 12f;
        float ty = panel.y + 12f;

        GUI.Label(new Rect(lx, ty, 240f, 20f), "UNIVERSAL CONSTANTS", title);

        ty += 28f;
        c.gravity          = DrawConstantSlider(lx, ty,      "GRAVITY",        c.gravity,          0.1f, 10f); ty += 42f;
        c.strongForce      = DrawConstantSlider(lx, ty,      "STRONG FORCE",   c.strongForce,      0.1f, 10f); ty += 42f;
        c.electromagnetism = DrawConstantSlider(lx, ty,      "ELECTROMAG.",    c.electromagnetism, 0.1f, 10f); ty += 42f;
        c.entropy          = DrawConstantSlider(lx, ty,      "ENTROPY",        c.entropy,          0f,   5f);  ty += 42f;
        c.darkEnergy       = DrawConstantSlider(lx, ty,      "DARK ENERGY",    c.darkEnergy,       0f,   5f);

        cm.ApplyConstants(c);

        // Stability readout
        ty = panel.y + panH - 34f;
        var stab = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = White80 } };
        float stability = c.StabilityBalance;
        Color stabCol   = Color.Lerp(Color.red, Color.green, stability);
        GUI.color = stabCol;
        GUI.Label(new Rect(lx, ty, 240f, 22f), $"STABILITY  {stability:0.00}", stab);
        GUI.color = Color.white;
    }

    private float DrawConstantSlider(float x, float y, string label, float value, float min, float max)
    {
        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            normal   = { textColor = White80 }
        };
        var valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight,
            normal    = { textColor = AccentCyan }
        };

        GUI.Label(new Rect(x, y, 160f, 16f), label, labelStyle);
        GUI.Label(new Rect(x + 160f, y, 90f, 16f), value.ToString("0.00"), valueStyle);
        float next = GUI.HorizontalSlider(new Rect(x, y + 18f, 246f, 14f), value, min, max);
        return next;
    }

    // ── Right panel: universe snapshot ───────────────────────────────────────
    private void DrawUniverseSnapshot(float sw, float sh)
    {
        UniverseSample origin = SimulationManager.Instance.Sample(Vector3.zero);
        float panH = 170f;
        Rect  panel = new Rect(sw - 254f, sh * 0.5f - panH * 0.5f, 240f, panH);

        GUI.color = PanelBg;
        GUI.Box(panel, GUIContent.none);
        GUI.color = Color.white;

        var title = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 11,
            fontStyle = FontStyle.Bold,
            normal    = { textColor = AccentCyan }
        };
        var body = new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = White80 } };
        var val  = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight,
            normal    = { textColor = Color.white }
        };

        float lx = panel.x + 10f;
        float rx = panel.x + 130f;
        float ty = panel.y + 10f;
        float ls = 22f;

        GUI.Label(new Rect(lx, ty, 220f, 18f), "ORIGIN SNAPSHOT", title); ty += 26f;

        GUI.Label(new Rect(lx, ty,       110f, 18f), "ARCHETYPE",    body);
        GUI.Label(new Rect(rx, ty,        90f, 18f), origin.planetArchetype.ToString(), val); ty += ls;

        GUI.Label(new Rect(lx, ty,       110f, 18f), "LIFE P.",      body);
        GUI.Label(new Rect(rx, ty,        90f, 18f), origin.lifeProbability.ToString("0.00"), val); ty += ls;

        GUI.Label(new Rect(lx, ty,       110f, 18f), "DENSITY",      body);
        GUI.Label(new Rect(rx, ty,        90f, 18f), origin.density.ToString("0.00"), val); ty += ls;

        GUI.Label(new Rect(lx, ty,       110f, 18f), "RADIATION",    body);
        GUI.Label(new Rect(rx, ty,        90f, 18f), origin.radiation.ToString("0.00"), val); ty += ls;

        GUI.Label(new Rect(lx, ty,       110f, 18f), "SIM TIME",     body);
        GUI.Label(new Rect(rx, ty,        90f, 18f), SimulationManager.Instance.SimulationTime.ToString("0.0"), val); ty += ls;

        if (codexManager != null)
        {
            GUI.Label(new Rect(lx, ty,   110f, 18f), "CODEX",        body);
            GUI.Label(new Rect(rx, ty,    90f, 18f), codexManager.EntryCount.ToString(), val);
        }
    }

    // ── Top-centre: seed input ────────────────────────────────────────────────
    private void DrawSeedControls(float sw, float sh)
    {
        string currentSeed = SimulationManager.Instance.SeedManager.Seed;
        float  cx          = sw * 0.5f;

        GUI.color = PanelBg;
        GUI.Box(new Rect(cx - 160f, 14f, 320f, 46f), GUIContent.none);
        GUI.color = Color.white;

        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 12,
            fontStyle = FontStyle.Bold,
            normal    = { textColor = AccentCyan }
        };
        GUI.Label(new Rect(cx - 155f, 20f, 60f, 22f), "SEED", labelStyle);

        if (editingSeed)
        {
            // Text field for seed edit
            var tfStyle = new GUIStyle(GUI.skin.textField) { fontSize = 12 };
            seedInputBuffer = GUI.TextField(new Rect(cx - 88f, 21f, 120f, 22f), seedInputBuffer, 6, tfStyle);

            var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 10 };
            if (GUI.Button(new Rect(cx + 36f, 21f, 50f, 22f), "SET", btnStyle))
            {
                SimulationManager.Instance.SeedManager.SetSeed(seedInputBuffer);
                editingSeed = false;
            }
            if (GUI.Button(new Rect(cx + 90f, 21f, 50f, 22f), "✕", btnStyle))
                editingSeed = false;
        }
        else
        {
            var seedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white }
            };
            GUI.Label(new Rect(cx - 88f, 16f, 120f, 28f), currentSeed, seedStyle);

            var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 10 };
            if (GUI.Button(new Rect(cx + 38f, 21f, 48f, 22f), "EDIT", btnStyle))
            {
                seedInputBuffer = currentSeed;
                editingSeed     = true;
            }

            if (persistence != null &&
                GUI.Button(new Rect(cx + 90f, 21f, 50f, 22f), "SAVE", btnStyle))
                persistence.Save();
        }
    }

    // ── Bottom-centre: whisper ────────────────────────────────────────────────
    private void DrawWhisper(float sw, float sh)
    {
        if (whisperSystem == null) return;

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter,
            wordWrap  = true,
            normal    = { textColor = new Color(0.85f, 0.85f, 1f, 0.65f) }
        };

        GUI.Label(new Rect(sw * 0.5f - 260f, sh - 52f, 520f, 40f),
            $"\"{whisperSystem.LastLine}\"", style);
    }

    // ── Bottom-right: help ────────────────────────────────────────────────────
    private void DrawHelp(float sw, float sh)
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            wordWrap = true,
            normal   = { textColor = new Color(1f, 1f, 1f, 0.35f) }
        };

        string help = "[P] Launch Probe   [Scroll] Zoom   [RMB Drag] Orbit   [MMB Drag] Pan";
        GUI.Label(new Rect(sw - 460f, sh - 24f, 450f, 18f), help, style);
    }
}
