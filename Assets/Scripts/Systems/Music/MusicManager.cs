using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip m_AugmentShopMusic;
    [SerializeField] private AudioClip m_CombatMusic;
    [SerializeField] private AudioClip m_GameOverMusic;
    [SerializeField, Range(0f, 1f)] private float m_DefaultVolume = 0.5f;

    private AudioClip m_CurrentClip;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = Mathf.Clamp01(m_DefaultVolume);
    }

    private void Update()
    {
        if (GameLoopManager.Instance == null)
        {
            return;
        }

        switch (GameLoopManager.Instance.CurrentState)
        {
            case GameLoopManager.GameState.AugmentSelection:
            case GameLoopManager.GameState.Shopping:
                PlayClipIfNeeded(m_AugmentShopMusic, true);
                break;
            case GameLoopManager.GameState.Combat:
                if (m_CombatMusic != null)
                {
                    PlayClipIfNeeded(m_CombatMusic, true);
                }
                else
                {
                    StopMusic();
                }
                break;
            case GameLoopManager.GameState.GameOver:
                if (m_GameOverMusic != null)
                {
                    PlayClipIfNeeded(m_GameOverMusic, true);
                }
                else
                {
                    StopMusic();
                }
                break;
        }
    }

    public void PlayMusic()
    {
        audioSource.Play();
    }

    public void StopMusic()
    {
        m_CurrentClip = null;
        audioSource.Stop();
    }

    public void PauseMusic()
    {
        audioSource.Pause();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    private void PlayClipIfNeeded(AudioClip clip, bool loop)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        if (m_CurrentClip == clip && audioSource.isPlaying)
        {
            return;
        }

        m_CurrentClip = clip;
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }
}
