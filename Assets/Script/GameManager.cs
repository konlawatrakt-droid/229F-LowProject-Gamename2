using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// GameManager — ระบบเก็บของ + เปลี่ยน Scene เมื่อชนะ
/// </summary>
public class GameManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Singleton
    // ─────────────────────────────────────────────
    public static GameManager Instance;

    // ─────────────────────────────────────────────
    //  UI References
    // ─────────────────────────────────────────────
    [Header("UI References")]
    public TextMeshProUGUI collectibleText;
    public GameObject      notReadyPanel;
    public TextMeshProUGUI notReadyText;

    [Header("Transition UI")]
    public CanvasGroup fadePanel;        // Image สีดำ CanvasGroup สำหรับ Fade
    public float       fadeDuration = 1f;

    // ─────────────────────────────────────────────
    //  Scene Settings
    // ─────────────────────────────────────────────
    [Header("Next Scene")]
    public string nextSceneName = "";    // ชื่อ Scene ถัดไป (ต้องเพิ่มใน Build Settings)
    public float  winDelay      = 0.5f; // หน่วงก่อน Fade (วินาที)

    // ─────────────────────────────────────────────
    //  State
    // ─────────────────────────────────────────────
    [HideInInspector] public int totalCollectibles;
    [HideInInspector] public int collectedCount;

    // ─────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        totalCollectibles = FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None).Length;
        collectedCount    = 0;

        if (notReadyPanel != null) notReadyPanel.SetActive(false);

        // ซ่อน fade panel ตอนเริ่ม
        if (fadePanel != null)
        {
            fadePanel.alpha          = 0f;
            fadePanel.blocksRaycasts = false;
        }

        UpdateUI();
    }

    // ─────────────────────────────────────────────
    //  Public Methods
    // ─────────────────────────────────────────────
    public void OnItemCollected()
    {
        collectedCount++;
        UpdateUI();
    }

    public bool IsAllCollected() => collectedCount >= totalCollectibles;

    public void TriggerWin()
    {
        StartCoroutine(WinSequence());
    }

    public void ShowNotReady()
    {
        if (notReadyPanel == null) return;
        notReadyPanel.SetActive(true);
        if (notReadyText != null)
            notReadyText.text = "ยังเก็บของไม่ครบ!\n" + collectedCount + " / " + totalCollectibles;

        CancelInvoke(nameof(HideNotReady));
        Invoke(nameof(HideNotReady), 2f);
    }

    // ─────────────────────────────────────────────
    //  Win Sequence (หน่วง → Fade → Load Scene)
    // ─────────────────────────────────────────────
    IEnumerator WinSequence()
    {
        // หน่วงนิดหน่อยก่อน Fade
        yield return new WaitForSeconds(winDelay);

        // Fade to black
        if (fadePanel != null)
        {
            fadePanel.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadePanel.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            fadePanel.alpha = 1f;
        }

        // โหลด Scene ถัดไป
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("[GameManager] ยังไม่ได้ตั้งชื่อ nextSceneName!");
    }

    // ─────────────────────────────────────────────
    void HideNotReady() { if (notReadyPanel != null) notReadyPanel.SetActive(false); }

    void UpdateUI()
    {
        if (collectibleText != null)
            collectibleText.text = collectedCount + " / " + totalCollectibles;
    }
}
