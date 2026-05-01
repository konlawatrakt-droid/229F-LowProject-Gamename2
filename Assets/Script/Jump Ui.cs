using UnityEngine;
using UnityEngine.UI;

public class ChargeBarUI : MonoBehaviour
{
    [Header("References")]
    public JumpKingComplete playerController; // ลาก Player มาใส่ช่องนี้
    public Slider chargeSlider;               // ลาก UI Slider มาใส่ช่องนี้

    [Header("Color Settings (Optional)")]
    public Image fillImage;                   // ลาก Fill ของ Slider มาใส่
    public Color normalColor = Color.yellow;  // สีตอนกำลังชาร์จ
    public Color fullChargeColor = Color.red; // สีตอนชาร์จเต็ม

    void Start()
    {
        // ตั้งค่า Slider ให้รองรับค่า 0 ถึง 1 ให้ตรงกับสคริปต์หลัก
        if (chargeSlider != null)
        {
            chargeSlider.minValue = 0f;
            chargeSlider.maxValue = 1f;
            chargeSlider.value = 0f;
        }
    }

    void Update()
    {
        // ตรวจสอบว่ามีการใส่ Reference ครบถ้วน
        if (playerController != null && chargeSlider != null)
        {
            // อัปเดตค่าหลอดตาม chargeRatio จากสคริปต์ผู้เล่น
            chargeSlider.value = playerController.chargeRatio;

            // เปลี่ยนสีหลอดเมื่อชาร์จเต็ม (ถ้ามีการใส่ Image อ้างอิงไว้)
            if (fillImage != null)
            {
                if (playerController.isFullCharge)
                {
                    fillImage.color = fullChargeColor;
                }
                else
                {
                    fillImage.color = normalColor;
                }
            }
        }
    }
}