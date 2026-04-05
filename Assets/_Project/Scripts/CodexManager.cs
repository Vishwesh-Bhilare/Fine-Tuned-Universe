using System.Collections.Generic;
using UnityEngine;

public class CodexManager : MonoBehaviour
{
    public static CodexManager Instance { get; private set; }

    private readonly HashSet<string>              signatures = new();
    private readonly List<(string sig, string type, float time)> log = new();

    public int EntryCount => signatures.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterScan(ScanResult result)
    {
        if (signatures.Add(result.signature))
        {
            log.Add((result.signature, result.scanType, SimulationManager.Instance?.SimulationTime ?? 0f));
            Debug.Log($"[Codex] New entry #{signatures.Count}: {result.signature}");
        }
    }

    public bool Contains(string sig) => signatures.Contains(sig);

    public IReadOnlyList<(string sig, string type, float time)> GetLog() => log;

    // Returns summary for HUD display
    public string GetLatestSummary()
    {
        if (log.Count == 0) return "No scans yet.";
        var (sig, type, time) = log[^1];
        return $"[{type.ToUpper()}] {sig.Split('|')[0]}  t={time:0.0}";
    }
}
