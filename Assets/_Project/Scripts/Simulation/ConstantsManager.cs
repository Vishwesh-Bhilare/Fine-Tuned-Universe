using UnityEngine;

public class ConstantsManager : MonoBehaviour
{
    public static ConstantsManager Instance;

    public UniverseConstants constants = new UniverseConstants();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
