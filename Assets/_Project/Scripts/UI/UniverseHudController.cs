using UnityEngine;
using UnityEngine.UI;

public class UniverseHudController : MonoBehaviour
{
    [Header("Constant Sliders")]
    [SerializeField] private Slider gravitySlider;
    [SerializeField] private Slider strongForceSlider;
    [SerializeField] private Slider electromagnetismSlider;
    [SerializeField] private Slider entropySlider;
    [SerializeField] private Slider darkEnergySlider;

    [Header("Probe UI")]
    [SerializeField] private CanvasGroup probeIndicator;

    private ConstantsManager constantsManager;

    private void Start()
    {
        constantsManager = SimulationManager.Instance.ConstantsManager;
        UniverseConstants c = constantsManager.Constants;

        BindSlider(gravitySlider, c.gravity, constantsManager.SetGravity);
        BindSlider(strongForceSlider, c.strongForce, constantsManager.SetStrongForce);
        BindSlider(electromagnetismSlider, c.electromagnetism, constantsManager.SetElectromagnetism);
        BindSlider(entropySlider, c.entropy, constantsManager.SetEntropy);
        BindSlider(darkEnergySlider, c.darkEnergy, constantsManager.SetDarkEnergy);
    }

    private void Update()
    {
        if (probeIndicator == null || SimulationManager.Instance == null)
        {
            return;
        }

        bool active = SimulationManager.Instance.CurrentMode == GameMode.Probe;
        probeIndicator.alpha = active ? 1f : 0f;
        probeIndicator.blocksRaycasts = active;
    }

    private static void BindSlider(Slider slider, float initialValue, UnityEngine.Events.UnityAction<float> onChange)
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(initialValue);
        slider.onValueChanged.AddListener(onChange);
    }
}
