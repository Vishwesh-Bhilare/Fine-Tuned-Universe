using UnityEngine;

public class ObservatoryController : MonoBehaviour
{
    [SerializeField] private Camera observatoryCamera;
    [SerializeField] private float  minDistance  = 4f;
    [SerializeField] private float  maxDistance  = 600f;
    [SerializeField] private float  zoomSpeed    = 100f;
    [SerializeField] private float  panSpeed     = 0.4f;

    private float   currentDistance = 120f;
    private Vector3 focusPoint      = Vector3.zero;
    private float   yaw;
    private float   pitch = 30f;

    private void OnEnable()
    {
        if (SimulationManager.Instance != null)
            SimulationManager.Instance.SetMode(GameMode.Observatory);
    }

    private void Update()
    {
        if (observatoryCamera == null) return;

        // Zoom
        float scroll = GameInput.MouseScrollDelta().y;
        currentDistance = Mathf.Clamp(
            currentDistance - scroll * zoomSpeed * Time.deltaTime,
            minDistance, maxDistance);

        // Orbit with right mouse drag
#if ENABLE_INPUT_SYSTEM
        bool  rmb     = UnityEngine.InputSystem.Mouse.current != null &&
                        UnityEngine.InputSystem.Mouse.current.rightButton.isPressed;
#else
        bool rmb = Input.GetMouseButton(1);
#endif
        if (rmb)
        {
            yaw   += GameInput.GetAxis("Mouse X") * 60f * Time.deltaTime;
            pitch -= GameInput.GetAxis("Mouse Y") * 60f * Time.deltaTime;
            pitch  = Mathf.Clamp(pitch, 5f, 85f);
        }

        // Pan with middle mouse
#if ENABLE_INPUT_SYSTEM
        bool mmb = UnityEngine.InputSystem.Mouse.current != null &&
                   UnityEngine.InputSystem.Mouse.current.middleButton.isPressed;
#else
        bool mmb = Input.GetMouseButton(2);
#endif
        if (mmb)
        {
            focusPoint -= observatoryCamera.transform.right   * GameInput.GetAxis("Mouse X") * panSpeed * currentDistance * Time.deltaTime;
            focusPoint -= observatoryCamera.transform.up      * GameInput.GetAxis("Mouse Y") * panSpeed * currentDistance * Time.deltaTime;
        }

        // Apply
        Quaternion rot    = Quaternion.Euler(pitch, yaw, 0f);
        Vector3    offset = rot * new Vector3(0f, 0f, -currentDistance);

        observatoryCamera.transform.position = focusPoint + offset;
        observatoryCamera.transform.LookAt(focusPoint);
    }

    public Camera GetCamera() => observatoryCamera;
}
