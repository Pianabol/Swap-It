using UnityEngine;
using System.Collections;
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip swapSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip comboSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    private int currentCombo = 0;
    private float resetComboTimer = 0f;
    private const float COMBO_RESET_TIME = 1.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (currentCombo > 0)
        {
            resetComboTimer -= Time.deltaTime;
            if (resetComboTimer <= 0)
            {
                currentCombo = 0;
            }
        }
    }

    public void PlayClick()
    {
        PlaySFX(clickSound);
    }

    public void PlaySwap()
    {
        PlaySFX(swapSound);
    }

    public void PlayMatch()
    {
        currentCombo++;
        resetComboTimer = COMBO_RESET_TIME;

        if (currentCombo > 1 && comboSound != null)
        {
            // Kombo arttıkça sesin inceliği (pitch) artar!
            float pitch = 1f + (currentCombo * 0.1f); 
            pitch = Mathf.Clamp(pitch, 1f, 2.5f); // Ses çok cırlamasın diye 2.5 ile sınırlandırdık
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(comboSound);
        }
        else
        {
            sfxSource.pitch = 1f;
            PlaySFX(matchSound);
        }
    }

   public void PlayWin()
    {
        sfxSource.pitch = 1f;
        
        // Sesi kesip biçmeden, 4 saniye çal, 1.5 saniyede fade-out yaparak kıs
        StartCoroutine(PlayAndFadeSound(winSound, 4f, 1.5f)); 
    }

    private IEnumerator PlayAndFadeSound(AudioClip clip, float playTime, float fadeTime)
    {
        if (clip == null) yield break;

        // 1. Sadece bu sese özel geçici bir kasetçalar yaratıyoruz (SFX'ler kısılmasın diye)
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = 1f;
        tempSource.Play();

        // 2. İlk 4 saniye tam seste çalmasını bekle
        yield return new WaitForSeconds(playTime);

        // 3. Sesi belirlenen sürede (fadeTime = 1.5 sn) yavaşça 0'a indir
        float startVolume = tempSource.volume;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            tempSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }

        // 4. İşlem bitince hafızayı temizle
        tempSource.Stop();
        Destroy(tempSource);
    }

    public void PlayLose()
    {
        sfxSource.pitch = 1f;
        PlaySFX(loseSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}