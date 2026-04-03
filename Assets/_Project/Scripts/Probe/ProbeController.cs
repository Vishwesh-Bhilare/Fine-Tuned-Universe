using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProbeController : MonoBehaviour
{
    public enum ProbeMovementMode { Drift, OrbitAssist, Anchor }

    [SerializeField] private float thrustPower = 8f;
    [SerializeField] private float steerTorque = 4f;
    [SerializeField] private float orbitAssistRadius = 30f;
    [SerializeField] private Transform anchorTarget;

    private Rigidbody rb;
    private ProbeMovementMode mode = ProbeMovementMode.Drift;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0.02f;
        rb.angularDrag = 0.1f;
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
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            mode = (ProbeMovementMode)(((int)mode + 1) % 3);
        }
    }

    private void FixedUpdate()
    {
        UniverseSample local = SimulationManager.Instance.Sample(transform.position);
        float entropyNoise = (DeterministicNoise.Sample3D(transform.position.x, transform.position.y, transform.position.z, SimulationManager.Instance.SeedManager.SeedValue) - 0.5f)
                             * local.entropy * 0.6f;

        float thrustInput = Input.GetAxis("Vertical");
        float strafeInput = Input.GetAxis("Horizontal");

        Vector3 inputForce = (transform.forward * thrustInput) + (transform.right * strafeInput * 0.7f);
        inputForce *= thrustPower * Mathf.Lerp(1f, 0.45f, local.gravityPotential);
        inputForce += transform.right * entropyNoise;

        rb.AddForce(inputForce, ForceMode.Acceleration);

        Vector3 torque = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), -strafeInput * 0.25f) * steerTorque;
        rb.AddTorque(torque, ForceMode.Acceleration);

        switch (mode)
        {
            case ProbeMovementMode.OrbitAssist:
                ApplyOrbitAssist(local.gravityPotential);
                break;
            case ProbeMovementMode.Anchor:
                ApplyAnchor();
                break;
        }
    }

    public void SetAnchor(Transform target)
    {
        anchorTarget = target;
    }

    private void ApplyOrbitAssist(float gravityPotential)
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, orbitAssistRadius);
        Transform best = null;
        float bestScore = 0f;

        for (int i = 0; i < nearby.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, nearby[i].transform.position);
            float score = 1f / Mathf.Max(1f, dist);
            if (score > bestScore)
            {
                bestScore = score;
                best = nearby[i].transform;
            }
        }

        if (best == null) return;

        Vector3 toBody = (best.position - transform.position);
        Vector3 tangent = Vector3.Cross(toBody.normalized, Vector3.up).normalized;
        rb.AddForce((toBody.normalized * gravityPotential * 10f) + (tangent * 2.5f), ForceMode.Acceleration);
    }

    private void ApplyAnchor()
    {
        if (anchorTarget == null)
        {
            rb.velocity *= 0.98f;
            return;
        }

        Vector3 desired = anchorTarget.position - (transform.forward * 4f);
        Vector3 correction = (desired - transform.position);
        rb.AddForce(correction * 1.8f, ForceMode.Acceleration);
        rb.velocity *= 0.92f;
    }
}
