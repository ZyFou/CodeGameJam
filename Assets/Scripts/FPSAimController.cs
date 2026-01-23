using UnityEngine;
using UnityEngine.InputSystem;

public class FPSAimController : MonoBehaviour
{
    [Header("Refs")]
    public Transform yawRoot;     // PlayerRig
    public Transform pitchRoot;   // CameraPivot
    public Camera cam;

    [Header("Look")]
    public float sensitivity = 0.12f;
    public float pitchMin = -80f;
    public float pitchMax = 80f;

    [Header("Invert Axis")]
    public bool invertX = false;  // gauche / droite
    public bool invertY = false;  // haut / bas

    [Header("Raycast")]
    public float maxDistance = 5f;
    public LayerMask interactMask = ~0;

    float yaw;
    float pitch;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (yawRoot == null) yawRoot = transform;
        if (pitchRoot == null) pitchRoot = cam.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
        HandleClick();
    }

    void Look()
    {
        Vector2 delta = Mouse.current.delta.ReadValue() * sensitivity;

        float x = delta.x * (invertX ? -1f : 1f);
        float y = delta.y * (invertY ? -1f : 1f);

        yaw += x;
        pitch -= y;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        yawRoot.localRotation = Quaternion.Euler(0f, yaw, 0f);
        pitchRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleClick()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactMask))
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var clickable = hit.collider.GetComponentInParent<ClickableTarget>();
                if (clickable != null)
                    clickable.Click();
            }
        }
    }
}
