using System.Collections.Generic;
using UnityEngine;

public class CodexManager : MonoBehaviour
{
    private readonly HashSet<string> entries = new HashSet<string>();

    public int EntryCount => entries.Count;

    public void RegisterScan(ScanResult result)
    {
        entries.Add(result.signature);
    }

    public bool Contains(string signature)
    {
        return entries.Contains(signature);
    }

    public IReadOnlyCollection<string> GetEntries()
    {
        return entries;
    }
}
