using UnityEngine;

public class WhisperSystem : MonoBehaviour
{
    [SerializeField] private Transform listenerTransform;
    [SerializeField] private float whisperInterval = 12f;

    private float timer;
    public string LastLine { get; private set; } = "I am still becoming.";

    private static readonly string[] stableLines =
    {
        "You move within me now.",
        "My currents hold steady around you.",
        "You found where life almost held."
    };

    private static readonly string[] collapseLines =
    {
        "Feel how I strain under this gravity.",
        "I fold inward where you drift.",
        "The light here is learning to break."
    };

    private static readonly string[] entropyLines =
    {
        "I cannot keep still at this edge.",
        "My patterns unravel in your wake.",
        "Order is temporary, but you still listen."
    };

    public void SetListener(Transform target)
    {
        listenerTransform = target;
    }

    private void Update()
    {
        if (SimulationManager.Instance == null || listenerTransform == null)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer < whisperInterval)
        {
            return;
        }

        timer = 0f;
        UniverseSample sample = SimulationManager.Instance.Sample(listenerTransform.position);

        LastLine = ResolveLine(sample);
        Debug.Log($"[Whisper] {LastLine}");
    }

    private string ResolveLine(UniverseSample sample)
    {
        int idx = Mathf.Abs(Mathf.FloorToInt(SimulationManager.Instance.SimulationTime)) % 3;

        if (sample.entropy > 0.65f)
        {
            return entropyLines[idx];
        }

        if (sample.gravityPotential > 0.72f)
        {
            return collapseLines[idx];
        }

        return stableLines[idx];
    }
}
