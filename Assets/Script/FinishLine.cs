using UnityEngine;

/// <summary>
/// FinishLine — แนบ Script นี้เข้า GameObject เส้นชัย
///
/// วิธีติดตั้ง:
///   1. สร้าง GameObject เส้นชัย → Add Component → FinishLine
///   2. ต้องมี Collider2D และตั้ง Is Trigger = true
///   3. Tag ของตัวละครต้องเป็น "Player"
/// </summary>
public class FinishLine : MonoBehaviour
{
    [Header("Visual Hint")]
    public GameObject lockedVisual;    // สีแดง / ไอคอนล็อก (ตอนยังเก็บไม่ครบ)
    public GameObject unlockedVisual;  // สีทอง / ไอคอนเปิด (ตอนพร้อมชนะ)

    void Update()
    {
        // อัพเดต visual ตาม state
        if (GameManager.Instance == null) return;

        bool ready = GameManager.Instance.IsAllCollected();

        if (lockedVisual   != null) lockedVisual.SetActive(!ready);
        if (unlockedVisual != null) unlockedVisual.SetActive(ready);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.IsAllCollected())
        {
            // เก็บครบ → ชนะ!
            GameManager.Instance.TriggerWin();
        }
        else
        {
            // ยังเก็บไม่ครบ → แจ้งเตือน
            GameManager.Instance.ShowNotReady();
        }
    }
}
