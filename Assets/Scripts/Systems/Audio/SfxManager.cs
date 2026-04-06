using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource m_AudioSource;

    [Header("Enemy")]
    [SerializeField] private AudioClip m_EnemyDamage;
    [SerializeField] private AudioClip m_EnemyDamageAlt;
    [SerializeField] private AudioClip m_EnemyDeath;

    [Header("Round")]
    [SerializeField] private AudioClip m_RoundComplete;
    [SerializeField] private AudioClip m_RoundLost;

    [Header("Towers")]
    [SerializeField] private AudioClip m_TowerPickup;
    [SerializeField] private AudioClip m_TowerDrop;

    [Header("UI")]
    [SerializeField] private AudioClip m_UiNoise1;
    [SerializeField] private AudioClip m_UiNoise2;

    [Header("Limits")]
    [SerializeField] private float m_MinHitInterval = 0.05f;

    private float m_LastHitTime;
    private bool m_UsePrimaryUiClip = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureAudioSource();
    }

    private void EnsureAudioSource()
    {
        if (m_AudioSource == null)
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        if (m_AudioSource == null)
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
        }

        m_AudioSource.playOnAwake = false;
        m_AudioSource.loop = false;
        m_AudioSource.spatialBlend = 0f;
    }

    public void PlayEnemyHit()
    {
        if (Time.unscaledTime - m_LastHitTime < m_MinHitInterval)
        {
            return;
        }

        m_LastHitTime = Time.unscaledTime;
        PlayOneShot(SelectRandom(m_EnemyDamage, m_EnemyDamageAlt));
    }

    public void PlayEnemyDeath()
    {
        PlayOneShot(m_EnemyDeath);
    }

    public void PlayRoundComplete()
    {
        PlayOneShot(m_RoundComplete);
    }

    public void PlayRoundLost()
    {
        PlayOneShot(m_RoundLost);
    }

    public void PlayTowerPickup()
    {
        PlayOneShot(m_TowerPickup);
    }

    public void PlayTowerDrop()
    {
        PlayOneShot(m_TowerDrop);
    }

    public void PlayUiClick()
    {
        PlayOneShot(SelectUiClip());
    }

    private void PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || m_AudioSource == null)
        {
            return;
        }

        m_AudioSource.PlayOneShot(clip, volumeScale);
    }

    private static AudioClip SelectRandom(AudioClip primary, AudioClip secondary)
    {
        if (primary == null)
        {
            return secondary;
        }

        if (secondary == null)
        {
            return primary;
        }

        return Random.value < 0.5f ? primary : secondary;
    }

    private AudioClip SelectUiClip()
    {
        AudioClip primary = m_UiNoise1;
        AudioClip secondary = m_UiNoise2;

        if (primary == null)
        {
            return secondary;
        }

        if (secondary == null)
        {
            return primary;
        }

        AudioClip chosen = m_UsePrimaryUiClip ? primary : secondary;
        m_UsePrimaryUiClip = !m_UsePrimaryUiClip;
        return chosen;
    }

}
