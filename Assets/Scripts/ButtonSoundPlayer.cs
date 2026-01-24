using UnityEngine;

public class ButtonSoundPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField]
    private AudioClip clickSound;

    [SerializeField]
    private AudioSource audioSource;

    [Header("Pitch Variation")]
    [SerializeField]
    private float pitchMin = 0.9f;

    [SerializeField]
    private float pitchMax = 1.1f;

    [Header("Volume")]
    [SerializeField]
    private float volume = 1f;

    private void Awake()
    {
        // Si aucun AudioSource n'est assigné, en créer un
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configurer l'AudioSource
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Son 2D
    }

    public void PlayClickSound()
    {
        if (clickSound == null || audioSource == null)
            return;

        // Variation aléatoire du pitch
        float randomPitch = Random.Range(pitchMin, pitchMax);
        audioSource.pitch = randomPitch;
        audioSource.volume = volume;

        // Jouer le son
        audioSource.PlayOneShot(clickSound);
    }
}
