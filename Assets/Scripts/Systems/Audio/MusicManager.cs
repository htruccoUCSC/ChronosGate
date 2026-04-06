using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MusicController : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip m_AugmentShopMusic;
    [SerializeField] private AudioClip m_CombatMusic;
    [SerializeField] private AudioClip m_GameOverMusic;
    [SerializeField, Range(0f, 1f)] private float m_DefaultVolume = 0.5f;
    [SerializeField] private float m_FadeOutDuration = 1f;
    [SerializeField] private float m_FadeInDuration = 1f;

    private AudioClip m_CurrentClip;
    private AudioClip m_TargetClip;
    private bool m_TargetLoop;
    private Coroutine m_TransitionRoutine;
    private float m_SavedCurrentTime;
    private float m_SavedShopTime;
    private float m_SavedCombatTime;
    private float m_SavedGameOverTime;
    private bool m_WarnedMissingCombat;

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
        AudioClip desiredClip = null;
        bool desiredLoop = true;

        if (GameLoopManager.Instance != null)
        {
            switch (GameLoopManager.Instance.CurrentState)
            {
                case GameLoopManager.GameState.AugmentSelection:
                    desiredClip = m_AugmentShopMusic;
                    break;
                case GameLoopManager.GameState.Combat:
                    desiredClip = m_CombatMusic;
                    break;
                case GameLoopManager.GameState.GameOver:
                    desiredClip = m_GameOverMusic;
                    break;
                default:
                    desiredClip = m_CombatMusic;
                    break;
            }

            RequestClip(desiredClip, desiredLoop);
            return;
        }

        if (GameLoopManagerOld.Instance != null)
        {
            switch (GameLoopManagerOld.Instance.CurrentState)
            {
                case GameLoopManagerOld.GameState.AugmentSelection:
                    desiredClip = m_AugmentShopMusic;
                    break;
                case GameLoopManagerOld.GameState.GameOver:
                    desiredClip = m_GameOverMusic;
                    break;
                default:
                    desiredClip = m_CombatMusic;
                    break;
            }

            RequestClip(desiredClip, desiredLoop);
            return;
        }

        RequestClip(m_CombatMusic, desiredLoop);
    }

    public void PlayMusic()
    {
        audioSource.Play();
    }

    public void ApplyState(GameLoopManager.GameState state)
    {
        AudioClip desiredClip = null;

        switch (state)
        {
            case GameLoopManager.GameState.AugmentSelection:
                desiredClip = m_AugmentShopMusic;
                break;
            case GameLoopManager.GameState.GameOver:
                desiredClip = m_GameOverMusic;
                break;
            default:
                desiredClip = m_CombatMusic;
                break;
        }

        RequestClip(desiredClip, true);
    }

    public void StopMusic()
    {
        SaveClipTime(m_CurrentClip, audioSource != null ? audioSource.time : 0f);
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

    private void RequestClip(AudioClip clip, bool loop)
    {
        if (clip == null)
        {
            if (!m_WarnedMissingCombat && m_CombatMusic == null)
            {
                Debug.LogWarning("[MusicController] Combat music is not assigned. Assign m_CombatMusic in the inspector.");
                m_WarnedMissingCombat = true;
            }

            return;
        }

        if (m_TargetClip == clip && m_TargetLoop == loop && m_TransitionRoutine != null)
        {
            return;
        }

        if (m_TargetClip == clip && m_TargetLoop == loop && m_CurrentClip == clip)
        {
            if (audioSource != null && m_CurrentClip != null && !audioSource.isPlaying)
            {
                audioSource.clip = m_CurrentClip;
                audioSource.loop = loop;
                ApplySavedTime(m_CurrentClip, GetSavedTime(m_CurrentClip));
                audioSource.volume = Mathf.Clamp01(m_DefaultVolume);
                audioSource.Play();
            }

            return;
        }

        m_TargetClip = clip;
        m_TargetLoop = loop;
        m_SavedCurrentTime = GetSavedTime(clip);

        if (m_TransitionRoutine != null)
        {
            StopCoroutine(m_TransitionRoutine);
        }

        m_TransitionRoutine = StartCoroutine(TransitionToClip());
    }

    private IEnumerator TransitionToClip()
    {
        if (audioSource == null)
        {
            yield break;
        }

        float startVolume = audioSource.volume;
        if (audioSource.isPlaying && startVolume > 0f)
        {
            yield return FadeVolume(startVolume, 0f, Mathf.Max(0.01f, m_FadeOutDuration));
        }

        SaveClipTime(m_CurrentClip, audioSource.time);
        audioSource.Stop();
        m_CurrentClip = m_TargetClip;

        if (m_CurrentClip == null)
        {
            audioSource.volume = Mathf.Clamp01(m_DefaultVolume);
            m_TransitionRoutine = null;
            yield break;
        }

        audioSource.clip = m_CurrentClip;
        audioSource.loop = m_TargetLoop;
        ApplySavedTime(m_CurrentClip, m_SavedCurrentTime);
        audioSource.Play();

        float targetVolume = Mathf.Clamp01(m_DefaultVolume);
        audioSource.volume = 0f;
        yield return FadeVolume(0f, targetVolume, Mathf.Max(0.01f, m_FadeInDuration));

        m_TransitionRoutine = null;
    }

    private void SaveClipTime(AudioClip clip, float time)
    {
        if (clip == null)
        {
            return;
        }

        float safeTime = Mathf.Max(0f, time);
        if (clip == m_AugmentShopMusic)
        {
            m_SavedShopTime = safeTime;
        }
        else if (clip == m_CombatMusic)
        {
            m_SavedCombatTime = safeTime;
        }
        else if (clip == m_GameOverMusic)
        {
            m_SavedGameOverTime = safeTime;
        }
    }

    private float GetSavedTime(AudioClip clip)
    {
        if (clip == null)
        {
            return 0f;
        }

        if (clip == m_AugmentShopMusic)
        {
            return m_SavedShopTime;
        }

        if (clip == m_CombatMusic)
        {
            return m_SavedCombatTime;
        }

        if (clip == m_GameOverMusic)
        {
            return m_SavedGameOverTime;
        }

        return 0f;
    }

    private void ApplySavedTime(AudioClip clip, float savedTime)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        float maxTime = Mathf.Max(0f, clip.length - 0.01f);
        audioSource.time = Mathf.Clamp(savedTime, 0f, maxTime);
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        if (audioSource == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            audioSource.volume = Mathf.Lerp(from, to, t);
            yield return null;
        }

        audioSource.volume = to;
    }
}
