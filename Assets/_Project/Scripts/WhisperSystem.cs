using UnityEngine;

public class WhisperSystem : MonoBehaviour
{
    [SerializeField] private Transform listenerTransform;
    [SerializeField] private float     whisperInterval = 14f;

    private float timer;
    public string LastLine { get; private set; } = "I am still becoming.";

    // Lines grouped by simulation state
    private static readonly string[] stableLines =
    {
        "You move within me now.",
        "My currents hold steady around you.",
        "You found where life almost held.",
        "Structure persists here. It is rare.",
        "The web is still. Listen."
    };

    private static readonly string[] collapseLines =
    {
        "Feel how I strain under this gravity.",
        "I fold inward where you drift.",
        "The light here is learning to break.",
        "Mass dominates. Nothing escapes this long.",
        "You are close to where I end."
    };

    private static readonly string[] entropyLines =
    {
        "I cannot keep still at this edge.",
        "My patterns unravel in your wake.",
        "Order is temporary, but you still listen.",
        "Turbulence is not chaos. It is becoming.",
        "I am not breaking. I am changing."
    };

    private static readonly string[] lifeLines =
    {
        "Something almost held here. Almost.",
        "The chemistry is close. I can feel it.",
        "Balance is fragile. You disturb nothing.",
        "A signal. Or perhaps a memory of one.",
        "I was tending this. You found it."
    };

    public void SetListener(Transform t) => listenerTransform = t;

    private void Update()
    {
        if (SimulationManager.Instance == null || listenerTransform == null) return;

        timer += Time.deltaTime;
        if (timer < whisperInterval) return;
        timer = 0f;

        UniverseSample s = SimulationManager.Instance.Sample(listenerTransform.position);
        LastLine = SelectLine(s);
        Debug.Log($"[Whisper] {LastLine}");
    }

    private string SelectLine(UniverseSample s)
    {
        int idx = Mathf.Abs(Mathf.FloorToInt(SimulationManager.Instance.SimulationTime)) % 5;

        if (s.lifeProbability > 0.55f)         return lifeLines[idx];
        if (s.entropy         > 0.65f)         return entropyLines[idx];
        if (s.gravityPotential > 0.72f)        return collapseLines[idx];
        return stableLines[idx];
    }
}
