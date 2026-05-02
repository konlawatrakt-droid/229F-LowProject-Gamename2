using UnityEngine;

/// <summary>
/// CollectibleItem — แนบ Script นี้เข้าทุก GameObject ที่ให้เก็บ
///
/// วิธีติดตั้ง:
///   1. GameObject ที่เป็นของ → Add Component → CollectibleItem
///   2. ต้องมี Collider2D และตั้ง Is Trigger = true
///   3. Tag ของตัวละครต้องเป็น "Player"
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    [Header("Effects")]
    public GameObject collectEffect;   // Particle / Effect ตอนเก็บ (optional)
    public AudioClip  collectSound;    // เสียงตอนเก็บ (optional)

    [Header("Animation")]
    public bool  floatUp    = true;    // ลอยขึ้น-ลงเบา ๆ
    public float floatSpeed = 2f;
    public float floatHeight = 0.2f;
    public bool  rotate     = true;
    public float rotateSpeed = 90f;    // องศา/วินาที

    private Vector3    startPos;
    private AudioSource audioSrc;

    void Start()
    {
        startPos = transform.position;
        audioSrc = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Animation ลอย
        if (floatUp)
        {
            float y = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }

        // Animation หมุน
        if (rotate)
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // เล่นเสียง
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Spawn effect
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        // แจ้ง GameManager
        GameManager.Instance?.OnItemCollected();

        // ทำลายตัวเอง
        Destroy(gameObject);
    }
}
