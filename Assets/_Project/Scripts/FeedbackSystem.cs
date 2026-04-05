using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FeedbackSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform  probe;
    [SerializeField] private Light      probeLight;
    [SerializeField] private ProbeController probeController;

    [Header("Audio")]
    [SerializeField] private AudioClip  stableHum;
    [SerializeField] private AudioClip  collapseHum;
    [SerializeField] private AudioClip  lifePulse;

    private AudioSource audioSource;
    private float       blendTarget;

    private void Awake()
    {
        audioSource          = GetComponent<AudioSource>();
        audioSource.loop     = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void Start()
    {
        if (!audioSource.isPlaying) audioSource.Play();
    }

    private void Update()
    {
        if (probe == null || SimulationManager.Instance == null) return;

        UniverseSample s = SimulationManager.Instance.Sample(probe.position);

        // Light
        if (probeLight != null)
        {
            probeLight.color     = Color.Lerp(Color.cyan, new Color(1f, 0.55f, 0.35f), s.radiation);
            probeLight.intensity = 0.8f + s.gravityPotential * 1.2f;
        }

        // Audio: pitch and volume encode state
        // Stable  = higher pitch, moderate volume
        // Collapse = low pitch, high volume
        // Life    = rhythmic (achieved via pitch modulation)
        float lifePulse = Mathf.Sin(Time.time * 3f) * 0.12f * s.lifeProbability;
        audioSource.pitch  = Mathf.Lerp(0.75f, 1.4f, 1f - s.entropy) + lifePulse;
        audioSource.volume = Mathf.Lerp(0.15f, 0.7f, s.gravityPotential);
    }

    public void SetProbe(Transform t)          => probe          = t;
    public void SetProbeLight(Light l)         => probeLight     = l;
    public void SetProbeController(ProbeController pc) => probeController = pc;
}
