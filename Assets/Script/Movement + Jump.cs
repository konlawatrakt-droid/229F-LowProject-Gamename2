using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JumpKingComplete : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float airDragCoefficient = 0.5f; // หัวข้อ E: Air Resistance

    [Header("Jump Settings")]
    public float maxChargeTime = 1.5f;
    public float jumpAngle = 65f;
    public float powerMultiplier = 12f;

    [Header("Components")]
    public Rigidbody2D rb;

    private float chargeStartTime;
    private bool isCharging = false;
    private bool isGrounded = true;
    private float horizontalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. รับค่าการเดิน (Horizontal Movement)
        // จะเดินได้เฉพาะตอนอยู่บนพื้นและไม่ได้ชาร์จกระโดดอยู่ (เลียนแบบ Jump King)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. ระบบชาร์จกระโดด
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isCharging = true;
            chargeStartTime = Time.time;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // หยุดเดินตอนชาร์จ
        }

        if (Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            float duration = Mathf.Min(Time.time - chargeStartTime, maxChargeTime);
            Jump(duration);
            isCharging = false;
        }
    }

    void FixedUpdate()
    {
        // 3. การเคลื่อนที่บนพื้น (หัวข้อ A: Constant Force/Velocity)
        if (isGrounded && !isCharging)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        // 4. การประยุกต์ใช้แรงต้านอากาศ (หัวข้อ E: Air Resistance)
        if (!isGrounded)
        {
            // สูตร: F = -k * v (แรงต้านแปรผันตามความเร็ว)
            Vector2 airResistance = -rb.linearVelocity * airDragCoefficient;
            rb.AddForce(airResistance);
        }
    }

    void Jump(float duration)
    {
        // --- หัวข้อ C: Projectile Motion & การคำนวณจากสูตร ---
        float u = duration * powerMultiplier;
        float angleRad = jumpAngle * Mathf.Deg2Rad;

        // คำนวณทิศทาง (หันไปตามที่กดเดินล่าสุด)
        float direction = horizontalInput != 0 ? horizontalInput : (transform.localScale.x > 0 ? 1 : -1);

        Vector2 velocityVector = new Vector2(
            direction * Mathf.Cos(angleRad) * u,
            Mathf.Sin(angleRad) * u
        );

        // --- การคำนวณ Force จาก F = ma (ตามเกณฑ์สีฟ้าในรูป) ---
        float mass = rb.mass;
        Vector2 force = mass * velocityVector;

        rb.AddForce(force, ForceMode2D.Impulse);
        isGrounded = false;
    }

    // --- หัวข้อ A: Collision Detection ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}