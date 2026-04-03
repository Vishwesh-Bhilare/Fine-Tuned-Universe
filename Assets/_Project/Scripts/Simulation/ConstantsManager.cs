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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (constants.gravity <= 0f)
        {
            constants = UniverseConstants.Default;
        }
    }

    public void ApplyConstants(UniverseConstants next)
    {
        constants = next;
        OnConstantsChanged?.Invoke(constants);
    }

    public void SetGravity(float value)
    {
        constants.gravity = value;
        OnConstantsChanged?.Invoke(constants);
    }

    public void SetStrongForce(float value)
    {
        constants.strongForce = value;
        OnConstantsChanged?.Invoke(constants);
    }

    public void SetElectromagnetism(float value)
    {
        constants.electromagnetism = value;
        OnConstantsChanged?.Invoke(constants);
    }

    public void SetEntropy(float value)
    {
        constants.entropy = value;
        OnConstantsChanged?.Invoke(constants);
    }

    public void SetDarkEnergy(float value)
    {
        constants.darkEnergy = value;
        OnConstantsChanged?.Invoke(constants);
    }
}
