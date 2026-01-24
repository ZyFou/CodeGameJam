using UnityEngine;

[DisallowMultipleComponent]
public class FloatLoop : MonoBehaviour
{
    public enum SpaceMode
    {
        Local,
        World,
    }

    [Header("Space")]
    public SpaceMode spaceMode = SpaceMode.Local;

    [Header("Base Float")]
    [Tooltip("Amplitude (hauteur) du float.")]
    public float amplitude = 0.25f;

    [Tooltip("Vitesse du float (cycles par seconde).")]
    public float speed = 1.0f;

    [Tooltip("Décalage en phase (si tu veux des objets désynchronisés).")]
    public float phaseOffset = 0f;

    [Header("Bounce / Ease")]
    [Tooltip("0 = sin normal, >0 = effet rebond (ease)")]
    [Range(0f, 2f)]
    public float bounce = 0.35f;

    [Tooltip("Randomise un peu l'amplitude au cours du temps.")]
    public float amplitudeRandom = 0.0f;

    [Tooltip("Randomise un peu la vitesse au cours du temps.")]
    public float speedRandom = 0.0f;

    [Header("Shake (Perlin Noise)")]
    [Tooltip("Force du shake en X/Y/Z.")]
    public Vector3 shakeAmount = new Vector3(0.02f, 0.02f, 0.02f);

    [Tooltip("Vitesse du shake (plus haut = plus nerveux).")]
    public float shakeSpeed = 6.0f;

    [Tooltip("Seed pour le shake (différents objets = différents patterns).")]
    public int noiseSeed = 0;

    [Header("Time")]
    public bool useUnscaledTime = false;

    Vector3 basePos;
    float seedX,
        seedY,
        seedZ;

    void Awake()
    {
        basePos = (spaceMode == SpaceMode.Local) ? transform.localPosition : transform.position;

        // Seeds pour perlin (évite que tout bouge pareil)
        float s = (noiseSeed == 0) ? Random.Range(1f, 9999f) : noiseSeed;
        seedX = s + 11.1f;
        seedY = s + 37.7f;
        seedZ = s + 91.3f;
    }

    void OnEnable()
    {
        // Re-capture base pos si l'objet a été déplacé dans l'éditeur / runtime
        basePos = (spaceMode == SpaceMode.Local) ? transform.localPosition : transform.position;
    }

    void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        // Random smooth (Perlin) pour amplitude/speed
        float ampMul = 1f + (amplitudeRandom * (Mathf.PerlinNoise(seedX, t * 0.2f) * 2f - 1f));
        float spdMul = 1f + (speedRandom * (Mathf.PerlinNoise(seedY, t * 0.2f) * 2f - 1f));

        float w = (t + phaseOffset) * speed * spdMul * Mathf.PI * 2f;

        // Sin normal (0..1)
        float sin01 = (Mathf.Sin(w) + 1f) * 0.5f;

        // Bounce: on transforme la courbe pour accentuer bas/haut (effet "rebond/ease")
        float eased = BounceEase01(sin01, bounce);

        // Float vertical (centre = 0)
        float y = (eased - 0.5f) * 2f * amplitude * ampMul;

        // Shake (Perlin -> -1..1)
        float nx = (Mathf.PerlinNoise(seedX, t * shakeSpeed) * 2f - 1f);
        float ny = (Mathf.PerlinNoise(seedY, t * shakeSpeed) * 2f - 1f);
        float nz = (Mathf.PerlinNoise(seedZ, t * shakeSpeed) * 2f - 1f);

        Vector3 shake = new Vector3(nx * shakeAmount.x, ny * shakeAmount.y, nz * shakeAmount.z);

        Vector3 target = basePos + new Vector3(0f, y, 0f) + shake;

        if (spaceMode == SpaceMode.Local)
            transform.localPosition = target;
        else
            transform.position = target;
    }

    // 0 = sin normal, >0 = courbe plus "rebond/ease"
    static float BounceEase01(float x, float bounceAmount)
    {
        if (bounceAmount <= 0f)
            return x;

        // mix entre "smoothstep" et une courbe plus "snappy"
        float smooth = x * x * (3f - 2f * x); // smoothstep
        float snappy = Mathf.Pow(x, 1f + bounceAmount * 2f); // accel vers la fin

        // on remixe selon bounceAmount
        float k = Mathf.Clamp01(bounceAmount);
        return Mathf.Lerp(smooth, snappy, k);
    }

    // Optionnel : si tu veux reset la base position à la demande
    [ContextMenu("Re-capture Base Position")]
    public void RecaptureBasePosition()
    {
        basePos = (spaceMode == SpaceMode.Local) ? transform.localPosition : transform.position;
    }
}
