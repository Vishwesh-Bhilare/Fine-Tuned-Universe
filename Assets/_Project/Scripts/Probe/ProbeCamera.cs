using UnityEngine;

public class ProbeCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 8f;
    [SerializeField] private float minDistance = 4f;
    [SerializeField] private float maxDistance = 14f;
    [SerializeField] private float orbitSpeed = 85f;

    private float yaw;
    private float pitch = 20f;
    public Transform Target => target;

    public void SetTarget(Transform nextTarget)
    {
        target = nextTarget;
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        yaw += GameInput.GetAxis("Mouse X") * orbitSpeed * Time.deltaTime;
        pitch -= GameInput.GetAxis("Mouse Y") * orbitSpeed * 0.6f * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -15f, 65f);

        distance = Mathf.Clamp(distance - GameInput.MouseScrollDelta().y * 0.7f, minDistance, maxDistance);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rot * new Vector3(0f, 1.8f, -distance);

        transform.position = target.position + offset;
        transform.rotation = rot;
    }
}
