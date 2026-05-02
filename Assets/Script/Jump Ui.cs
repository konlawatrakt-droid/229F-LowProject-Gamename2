using UnityEngine;
using UnityEngine.UI;

public class ChargeBarUI : MonoBehaviour
{
    [Header("References")]
    public Slider chargeSlider;
    public FurballAdventure player;

    [Header("Fill Colors")]
    public Color normalColor = new Color(0.2f, 0.85f, 1f, 1f);  // ฟ้า
    public Color fullColor = new Color(1f, 0.3f, 0.1f, 1f); // แดงส้ม ตอนเต็ม

    [Header("Pulse When Full")]
    public bool pulseWhenFull = true;
    public float pulseSpeed = 8f;
    public float pulseAmount = 0.06f;

    [Header("Auto Hide")]
    public bool hideWhenIdle = true;

    [Header("Position (offset above player head)")]
    public Vector2 screenOffset = new Vector2(0f, 80f);

    // ─────────────────────────────────────────────
    private CanvasGroup canvasGroup;
    private RectTransform sliderRect;
    private Image fillImage;   // Fill Area > Fill ของ Slider
    private Vector3 originalScale;

    // ─────────────────────────────────────────────
    void Awake()
    {
        sliderRect = chargeSlider.GetComponent<RectTransform>();
        originalScale = sliderRect.localScale;

        // หา Image ที่เป็น Fill ของ Slider
        // โครงสร้าง default: Slider → Fill Area → Fill (Image)
        Transform fillArea = chargeSlider.transform.Find("Fill Area");
        if (fillArea != null)
        {
            Transform fill = fillArea.Find("Fill");
            if (fill != null)
                fillImage = fill.GetComponent<Image>();
        }

        // CanvasGroup สำหรับ fade in/out
        canvasGroup = chargeSlider.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = chargeSlider.gameObject.AddComponent<CanvasGroup>();

        // ตั้งค่าเริ่มต้น Slider
        chargeSlider.minValue = 0f;
        chargeSlider.maxValue = 1f;
        chargeSlider.value = 0f;
        chargeSlider.interactable = false;
    }

    void Update()
    {
        if (player == null || chargeSlider == null) return;

        float ratio = player.chargeRatio;
        bool charging = ratio > 0f;
        bool fullCharge = player.isFullCharge;

        // ── 1. Show / Hide ───────────────────────
        if (hideWhenIdle)
        {
            float target = charging ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.deltaTime * 14f);
        }
        else
        {
            canvasGroup.alpha = 1f;
        }

        // ── 2. Slider value ──────────────────────
        chargeSlider.value = ratio;

        // ── 3. Fill color ────────────────────────
        if (fillImage != null)
            fillImage.color = fullCharge ? fullColor : normalColor;

        // ── 4. Pulse ตอนเต็ม ─────────────────────
        if (pulseWhenFull && fullCharge)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            sliderRect.localScale = originalScale * pulse;
        }
        else
        {
            sliderRect.localScale = originalScale;
        }

        // ── 5. ตาม Player ────────────────────────
        FollowPlayer();
    }

    void FollowPlayer()
    {
        if (player == null || Camera.main == null) return;

        Vector3 worldPos = player.transform.position;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0) { canvasGroup.alpha = 0f; return; }

        screenPos.x += screenOffset.x;
        screenPos.y += screenOffset.y;

        sliderRect.position = screenPos;
    }
}