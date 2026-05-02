using UnityEngine;

/// <summary>
/// BGMManager — เปิดเพลง Background ตลอดเกม
///
/// วิธีติดตั้ง:
///   1. สร้าง Empty GameObject ชื่อ "BGMManager"
///   2. แนบ Script นี้เข้า
///   3. ลาก AudioClip (เพลง) เข้าช่อง bgmClip
///   4. ปรับ volume ได้ตาม Inspector
/// </summary>
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("Audio Settings")]
    public AudioClip bgmClip;
    [Range(0f, 1f)]
    public float volume = 0.5f;
    public bool  playOnStart = true;
    public bool  loop        = true;

    [Header("Fade Settings")]
    public bool  fadeIn       = true;   // Fade เพลงตอนเริ่ม
    public float fadeInTime   = 2f;     // วินาที

    private AudioSource audioSource;

    // ─────────────────────────────────────────────
    void Awake()
    {
        // Singleton — ข้าม Scene ได้ไม่ขาด
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // ไม่ถูกทำลายเมื่อเปลี่ยน Scene
        }
        else
        {
            Destroy(gameObject);  // ถ้ามีอยู่แล้วให้ทำลายตัวใหม่
            return;
        }

        // Setup AudioSource
        audioSource           = gameObject.AddComponent<AudioSource>();
        audioSource.clip      = bgmClip;
        audioSource.loop      = loop;
        audioSource.playOnAwake = false;
        audioSource.volume    = fadeIn ? 0f : volume;
    }

    void Start()
    {
        if (playOnStart && bgmClip != null)
        {
            audioSource.Play();
            if (fadeIn) StartCoroutine(FadeIn());
        }
    }

    // ─────────────────────────────────────────────
    //  Public Controls
    // ─────────────────────────────────────────────

    /// <summary>เปลี่ยนเพลง (fade out เพลงเก่า → fade in เพลงใหม่)</summary>
    public void PlayBGM(AudioClip newClip)
    {
        if (newClip == bgmClip) return;
        StopAllCoroutines();
        StartCoroutine(SwitchBGM(newClip));
    }

    /// <summary>หยุดเพลง</summary>
    public void StopBGM()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut(() => audioSource.Stop()));
    }

    /// <summary>ตั้ง Volume (0–1)</summary>
    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        audioSource.volume = volume;
    }

    // ─────────────────────────────────────────────
    //  Coroutines
    // ─────────────────────────────────────────────
    System.Collections.IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, t / fadeInTime);
            yield return null;
        }
        audioSource.volume = volume;
    }

    System.Collections.IEnumerator FadeOut(System.Action onDone = null)
    {
        float startVol = audioSource.volume;
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, t / fadeInTime);
            yield return null;
        }
        audioSource.volume = 0f;
        onDone?.Invoke();
    }

    System.Collections.IEnumerator SwitchBGM(AudioClip newClip)
    {
        // Fade out เพลงเก่า
        yield return StartCoroutine(FadeOut());

        // เปลี่ยนเพลง
        bgmClip            = newClip;
        audioSource.clip   = newClip;
        audioSource.Play();

        // Fade in เพลงใหม่
        yield return StartCoroutine(FadeIn());
    }
}
