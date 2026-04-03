using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FeedbackSystem : MonoBehaviour
{
    [SerializeField] private Transform probe;
    [SerializeField] private Light probeLight;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = true;
    }

    private void Update()
    {
        if (probe == null || SimulationManager.Instance == null)
        {
            return;
        }

        UniverseSample sample = SimulationManager.Instance.Sample(probe.position);

        if (probeLight != null)
        {
            probeLight.color = Color.Lerp(Color.cyan, new Color(1f, 0.55f, 0.35f), sample.radiation);
            probeLight.intensity = 0.8f + sample.gravityPotential * 1.2f;
        }

        audioSource.pitch = Mathf.Lerp(0.8f, 1.35f, 1f - sample.entropy);
        audioSource.volume = Mathf.Lerp(0.2f, 0.65f, sample.gravityPotential);
    }

    public void SetProbe(Transform probeTransform)
    {
        probe = probeTransform;
    }

    public void SetProbeLight(Light lightSource)
    {
        probeLight = lightSource;
    }
}
