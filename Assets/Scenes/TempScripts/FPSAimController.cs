using UnityEngine;
using UnityEngine.InputSystem;

public class FPSAimController : MonoBehaviour
{
    [Header("Refs")]
    public Transform yawRoot;     // PlayerRig
    public Transform pitchRoot;   // CameraPivot
    public Camera cam;

    [Header("Look")]
    public float sensitivity = 120f;   // recommandé avec Time.deltaTime
    public float pitchMin = -80f;
    public float pitchMax = 80f;

    [Header("Invert Axis")]
    public bool invertX = false;
    public bool invertY = false;

    [Header("Raycast")]
    public float maxDistance = 5f;
    public LayerMask interactMask; // met Interactable ici

    float yaw;
    float pitch;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (yawRoot == null) yawRoot = transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = yawRoot.localEulerAngles.y;

        if (pitchRoot != null)
        {
            pitch = pitchRoot.localEulerAngles.x;
            if (pitch > 180f) pitch -= 360f;
        }
    }

    void Update()
    {
        if (Mouse.current == null) return;

        Look();
        HandleClick();
    }

    void Look()
    {
        Vector2 delta = Mouse.current.delta.ReadValue() * sensitivity * Time.deltaTime;

        float x = delta.x * (invertX ? -1f : 1f);
        float y = delta.y * (invertY ? -1f : 1f);

        yaw += x;
        pitch -= y;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        yawRoot.localRotation = Quaternion.Euler(0f, yaw, 0f);
        if (pitchRoot != null)
            pitchRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleClick()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactMask))
        {
            var clickable = hit.collider.GetComponentInParent<IClickable>();
            if (clickable != null)
            {
                clickable.Click(new ClickContext { cam = cam, hit = hit, ray = ray });
            }
        }
    }
}
