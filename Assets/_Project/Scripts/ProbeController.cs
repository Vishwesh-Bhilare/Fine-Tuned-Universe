using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProbeController : MonoBehaviour
{
    public enum MovementMode { Drift, OrbitAssist, Anchor }

    [Header("Movement")]
    [SerializeField] private float thrustPower    = 8f;
    [SerializeField] private float steerTorque    = 4f;
    [SerializeField] private float orbitRadius    = 30f;
    [SerializeField] private Transform anchorTarget;

    private Rigidbody rb;
    private MovementMode mode = MovementMode.Drift;
    public  MovementMode Mode => mode;

    // Feedback state
    private float currentJitter;
    private float currentDrag;

    private void Awake()
    {
        rb            = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping       = 0.02f;
        rb.angularDamping = 0.1f;
    }

    private void OnEnable()
    {
        if (SimulationManager.Instance != null)
        {
            transform.position = SimulationManager.Instance.GetDeterministicProbeSpawn();
            SimulationManager.Instance.SetMode(GameMode.Probe);
        }
    }

    private void Update()
    {
        if (GameInput.GetKeyDown(KeyCode.Tab))
            mode = (MovementMode)(((int)mode + 1) % 3);
    }

    private void FixedUpdate()
    {
        if (SimulationManager.Instance == null) return;

        UniverseSample local = SimulationManager.Instance.Sample(transform.position);

        // Entropy introduces jitter in controls
        currentJitter = local.entropy;
        float jitter  = (DeterministicNoise.Sample3D(
                            transform.position.x, transform.position.y, transform.position.z,
                            SimulationManager.Instance.SeedManager.SeedValue) - 0.5f)
                        * local.entropy * 0.6f;

        // Gravity wells slow thrust
        float thrustScale = Mathf.Lerp(1f, 0.35f, local.gravityPotential);

        float thrustInput  = GameInput.GetAxis("Vertical");
        float strafeInput  = GameInput.GetAxis("Horizontal");

        Vector3 force = (transform.forward * thrustInput + transform.right * strafeInput * 0.7f)
                        * thrustPower * thrustScale;
        force += transform.right * jitter;

        rb.AddForce(force, ForceMode.Acceleration);

        Vector3 torque = new Vector3(
            -GameInput.GetAxis("Mouse Y"),
             GameInput.GetAxis("Mouse X"),
            -strafeInput * 0.25f) * steerTorque;
        rb.AddTorque(torque, ForceMode.Acceleration);

        // Adaptive drag: heavier near gravity wells
        rb.linearDamping = Mathf.Lerp(0.02f, 0.18f, local.gravityPotential);

        switch (mode)
        {
            case MovementMode.OrbitAssist: ApplyOrbitAssist(local.gravityPotential); break;
            case MovementMode.Anchor:      ApplyAnchor();      break;
        }
    }

    private void ApplyOrbitAssist(float gravPot)
    {
        var nearby = Physics.OverlapSphere(transform.position, orbitRadius);
        Transform best      = null;
        float     bestScore = 0f;

        foreach (var col in nearby)
        {
            float dist  = Vector3.Distance(transform.position, col.transform.position);
            float score = 1f / Mathf.Max(1f, dist);
            if (score > bestScore) { bestScore = score; best = col.transform; }
        }

        if (best == null) return;

        Vector3 toBody  = best.position - transform.position;
        Vector3 tangent = Vector3.Cross(toBody.normalized, Vector3.up).normalized;
        rb.AddForce(toBody.normalized * gravPot * 10f + tangent * 2.5f, ForceMode.Acceleration);
    }

    private void ApplyAnchor()
    {
        if (anchorTarget == null) { rb.linearVelocity *= 0.98f; return; }
        Vector3 desired    = anchorTarget.position - transform.forward * 4f;
        Vector3 correction = desired - transform.position;
        rb.AddForce(correction * 1.8f, ForceMode.Acceleration);
        rb.linearVelocity *= 0.92f;
    }

    public void SetAnchor(Transform t) => anchorTarget = t;

    // Returns normalised jitter 0-1 for feedback
    public float GetJitterFactor() => currentJitter;
    public float GetGravityFactor()
    {
        if (SimulationManager.Instance == null) return 0f;
        return SimulationManager.Instance.Sample(transform.position).gravityPotential;
    }
}
