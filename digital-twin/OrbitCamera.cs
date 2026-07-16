using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// OrbitCamera
// Free-look orbit camera that follows the rocket.
// Right-click drag to orbit. Scroll wheel to zoom.
// Middle-click drag to pan. F key to snap back to rocket.
// ─────────────────────────────────────────────────────────────────────────────
public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Orbit")]
    public float orbitSpeed   = 3f;
    public float initialYaw   = 0f;
    public float initialPitch = 20f;

    [Header("Zoom")]
    public float distance    = 80f;
    public float minDistance = 5f;
    public float maxDistance = 15000f;
    public float zoomSensitivity = 0.1f;

    [Header("Pan")]
    public float panSpeed = 0.3f;

    [Header("Smoothing")]
    public float smoothTime = 0.08f;

    // Private
    private float   yaw;
    private float   pitch;
    private Vector3 panOffset;
    private float   smoothDistance;
    private float   distVelocity;

    void Start()
    {
        yaw            = initialYaw;
        pitch          = initialPitch;
        smoothDistance = distance;
        panOffset      = Vector3.zero;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ── Orbit — right mouse button ────────────────────────────────────────
        if (Input.GetMouseButton(1))
        {
            yaw   += Input.GetAxis("Mouse X") * orbitSpeed;
            pitch -= Input.GetAxis("Mouse Y") * orbitSpeed;
            pitch  = Mathf.Clamp(pitch, -80f, 80f);
        }

        // ── Zoom — scroll wheel ───────────────────────────────────────────────
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
            distance -= scroll * zoomSensitivity * distance * 10f;
        distance       = Mathf.Clamp(distance, minDistance, maxDistance);
        smoothDistance = Mathf.SmoothDamp(smoothDistance, distance,
                                          ref distVelocity, smoothTime);

        // ── Pan — middle mouse button ─────────────────────────────────────────
        if (Input.GetMouseButton(2))
        {
            Vector3 right = transform.right   * (-Input.GetAxis("Mouse X") * panSpeed * smoothDistance * 0.01f);
            Vector3 up    = transform.up      * (-Input.GetAxis("Mouse Y") * panSpeed * smoothDistance * 0.01f);
            panOffset += right + up;
        }

        // ── F key — snap back to rocket ───────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.F))
            panOffset = Vector3.zero;

        // ── Apply ─────────────────────────────────────────────────────────────
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3    focusPoint = target.position + panOffset;
        transform.position   = focusPoint - rot * Vector3.forward * smoothDistance;
        transform.LookAt(focusPoint);
    }
}
