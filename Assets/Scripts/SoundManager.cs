using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance = null;

    public AudioSource audioSource;
    public AudioClip mapSoundClip;
    public AudioClip cameraClickSoundClip;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void PlayMapSound()
    {
        if (audioSource != null && mapSoundClip != null)
        {
            audioSource.PlayOneShot(mapSoundClip);
        }
    }

    public void PlayCameraClickSound()
    {
        if (audioSource != null && cameraClickSoundClip != null)
        {
            audioSource.PlayOneShot(cameraClickSoundClip);
        }
    }
}
