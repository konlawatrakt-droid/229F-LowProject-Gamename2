using UnityEngine;
using UnityEngine.SceneManagement; // ต้องมีบรรทัดนี้เพื่อใช้คำสั่งเปลี่ยนฉาก/รีสตาร์ท

public class PlayerDeath : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject restartButtonUI; // ลากปุ่ม Restart (หรือหน้าต่าง Game Over) มาใส่ช่องนี้

    void Start()
    {
        // ซ่อนปุ่ม Restart ไว้ตอนเริ่มเกม
        if (restartButtonUI != null)
        {
            restartButtonUI.SetActive(false);
        }
    }

    // ฟังก์ชันนี้จะทำงานเมื่อตัวละครไปโดน Hit box ที่ติ๊ก Is Trigger ไว้
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // เช็คว่าสิ่งที่เราไปชนมี Tag ชื่อ "Danger" (เช่น หนาม, ลาวา, หรือพื้นที่ตกแมพ)
        if (collision.CompareTag("Danger"))
        {
            GameOver();
        }
    }

    void GameOver()
    {
        // 1. ทำให้ตัวละครหยุดนิ่ง (ไม่บังคับ แต่แนะนำสำหรับเกมกระโดด)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static; // ทำให้ขยับ/ตกไม่ได้อีก
        }

        // 2. แสดงปุ่ม Restart ขึ้นมา
        if (restartButtonUI != null)
        {
            restartButtonUI.SetActive(true);
        }
    }

    // ฟังก์ชันนี้สำหรับผูกกับปุ่มบนหน้าจอ (UI Button)
    public void RestartGame()
    {
        // โหลดฉากปัจจุบันใหม่ (เริ่มต้นใหม่)
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}