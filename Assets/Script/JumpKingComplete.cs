using UnityEngine;

/// <summary>
/// Jump King-style controller + Water Support
/// - กระโดดและเดินได้ในน้ำ (BuoyancyEffector2D)
/// - ตรวจจับน้ำด้วย OnTriggerEnter2D/Exit2D (Is Trigger = true)
/// - Tag น้ำต้องเป็น "Water"
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class JumpKingComplete : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────────
    [Header("Movement Settings")]
    public float moveSpeed           = 5f;
    public float airDragCoefficient  = 0.5f;

    [Header("Jump Settings")]
    public float maxChargeTime  = 1.5f;
    public float jumpAngle      = 65f;
    public float minJumpPower   = 3f;
    public float maxJumpPower   = 14f;

    [Header("Water Settings")]
    public float waterMoveSpeed      = 3f;    // ความเร็วในน้ำ (ช้ากว่าบกนิดหน่อย)
    public float waterJumpPowerScale = 0.7f;  // กระโดดในน้ำได้แรงน้อยกว่า (0.7 = 70%)
    public float waterDrag           = 2f;    // แรงต้านน้ำเพิ่มเติม

    // ─────────────────────────────────────────────
    //  Public State (อ่านโดย ChargeBarUI)
    // ─────────────────────────────────────────────
    [HideInInspector] public float chargeRatio;
    [HideInInspector] public bool  isFullCharge;

    // ─────────────────────────────────────────────
    //  Private
    // ─────────────────────────────────────────────
    private Rigidbody2D rb;
    private float       chargeStartTime;
    private bool        isCharging;
    private bool        isGrounded = true;
    private bool        isInWater  = false;   // ← ตัวแปรใหม่ตรวจจับน้ำ
    private float       horizontalInput;
    private float       facingDir  = 1f;

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // ── 1. Horizontal input ─────────────────
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
        {
            facingDir = horizontalInput;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDir;
            transform.localScale = scale;
        }

        // ── 2. Start charging ─────────────────────
        // กระโดดได้ทั้งตอนอยู่บนพื้นและในน้ำ
        bool canJump = isGrounded || isInWater;

        if (Input.GetKeyDown(KeyCode.Space) && canJump && !isCharging)
        {
            isCharging      = true;
            isFullCharge    = false;
            chargeStartTime = Time.time;
            chargeRatio     = 0f;

            // หยุดการเดินขณะชาร์จ
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // ── 3. Update charge ratio ───────────────
        if (isCharging)
        {
            float elapsed = Time.time - chargeStartTime;
            chargeRatio  = Mathf.Clamp01(elapsed / maxChargeTime);
            isFullCharge = (chargeRatio >= 1f);
        }

        // ── 4. Release to jump ───────────────────
        if (Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            Jump(chargeRatio);
            ResetCharge();
        }
    }

    void FixedUpdate()
    {
        // ── 5. Movement ─────────────────────────
        if (!isCharging)
        {
            if (isGrounded)
            {
                // เดินบนพื้นปกติ
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
            }
            else if (isInWater)
            {
                // เดินในน้ำ (ช้ากว่า + ลอยตัว BuoyancyEffector จัดการให้แล้ว)
                rb.linearVelocity = new Vector2(horizontalInput * waterMoveSpeed, rb.linearVelocity.y);
            }
        }

        // ── 6. Air resistance  F = -k·v ──────────
        if (!isGrounded && !isInWater)
        {
            Vector2 drag = -rb.linearVelocity * airDragCoefficient;
            rb.AddForce(drag);
        }

        // ── 7. Water drag  F = -k·v (แรงกว่า) ───
        if (isInWater)
        {
            Vector2 waterResistance = -rb.linearVelocity * waterDrag;
            rb.AddForce(waterResistance);
        }
    }

    // ─────────────────────────────────────────────
    //  Jump (Projectile Motion)
    // ─────────────────────────────────────────────
    void Jump(float ratio)
    {
        float power    = Mathf.Lerp(minJumpPower, maxJumpPower, ratio);

        // ถ้าอยู่ในน้ำ ลดพลังกระโดด
        if (isInWater)
            power *= waterJumpPowerScale;

        float angleRad = jumpAngle * Mathf.Deg2Rad;
        Vector2 launchVelocity = new Vector2(
            facingDir * Mathf.Cos(angleRad) * power,
            Mathf.Sin(angleRad) * power
        );

        // F = m·v  → Impulse
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(launchVelocity * rb.mass, ForceMode2D.Impulse);

        isGrounded = false;
    }

    void ResetCharge()
    {
        isCharging   = false;
        isFullCharge = false;
        chargeRatio  = 0f;
    }

    // ─────────────────────────────────────────────
    //  Ground Collision
    // ─────────────────────────────────────────────
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    // ─────────────────────────────────────────────
    //  Water Trigger (Is Trigger = true บน Collider น้ำ)
    //  ← ต้องตั้ง Tag ของ Water Object เป็น "Water"
    // ─────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
            isInWater = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
            isInWater = false;
    }
}
