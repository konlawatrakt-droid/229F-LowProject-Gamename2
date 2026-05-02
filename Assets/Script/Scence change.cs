using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นต้องมีเพื่อจัดการฉาก

public class SceneController : MonoBehaviour
{
    // ฟังก์ชันสำหรับโหลดฉาก โดยรับค่าเป็น "ชื่อฉาก" (String)
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ฟังก์ชันสำหรับออกจากเกม
    public void QuitGame()
    {
        // แสดงข้อความใน Console เพื่อให้รู้ว่าปุ่มทำงานตอนรันใน Unity Editor
        Debug.Log("Quit Game Working!"); 
        
        // คำสั่งปิดเกม (จะเห็นผลจริงๆ ตอน Build เกมออกมาเล่นแล้วเท่านั้น)
        Application.Quit();
    }
}