using System;
using UnityEngine;

public class ConstantsManager : MonoBehaviour
{
    public static ConstantsManager Instance { get; private set; }

    [SerializeField] private UniverseConstants constants = default;

    public UniverseConstants Constants => constants;
    public event Action<UniverseConstants> OnConstantsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (constants.gravity <= 0f) constants = UniverseConstants.Default;
    }

    public void ApplyConstants(UniverseConstants next)
    {
        constants = next;
        OnConstantsChanged?.Invoke(constants);
    }

    public void SetGravity(float v)          { constants.gravity          = v; OnConstantsChanged?.Invoke(constants); }
    public void SetStrongForce(float v)      { constants.strongForce      = v; OnConstantsChanged?.Invoke(constants); }
    public void SetElectromagnetism(float v) { constants.electromagnetism = v; OnConstantsChanged?.Invoke(constants); }
    public void SetEntropy(float v)          { constants.entropy          = v; OnConstantsChanged?.Invoke(constants); }
    public void SetDarkEnergy(float v)       { constants.darkEnergy       = v; OnConstantsChanged?.Invoke(constants); }
}
