using UnityEngine;

public class ObservatoryController : MonoBehaviour
{
    [SerializeField] private Camera observatoryCamera;
    [SerializeField] private float minScaleDistance = 4f;
    [SerializeField] private float maxScaleDistance = 600f;
    [SerializeField] private float zoomSpeed = 100f;

    private float currentDistance = 120f;

    private void OnEnable()
    {
        if (SimulationManager.Instance != null)
        {
            SimulationManager.Instance.SetMode(GameMode.Observatory);
        }
    }

    private void Update()
    {
        if (observatoryCamera == null)
        {
            return;
        }

        float scroll = Input.mouseScrollDelta.y;
        currentDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed * Time.deltaTime, minScaleDistance, maxScaleDistance);

        observatoryCamera.transform.position = new Vector3(0f, currentDistance * 0.4f, -currentDistance);
        observatoryCamera.transform.LookAt(Vector3.zero);
    }
}
