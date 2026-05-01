using UnityEngine;

/// <summary>
/// Jump King-style controller
/// - Projectile motion (angle + charge power)
/// - Charge clamped at maxChargeTime (full charge indicator)
/// - Broadcasts chargeRatio (0–1) for ChargeBarUI to read
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class JumpKingComplete : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────────
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float airDragCoefficient = 0.5f;

    [Header("Jump Settings")]
    public float maxChargeTime   = 1.5f;   // วินาทีที่ชาร์จเต็ม
    public float jumpAngle       = 65f;    // องศาจากแนวนอน
    public float minJumpPower    = 3f;     // พลังต่ำสุด (ชาร์จ 0 วิ)
    public float maxJumpPower    = 14f;    // พลังสูงสุด (ชาร์จเต็ม)

    // ─────────────────────────────────────────────
    //  Public State (อ่านโดย ChargeBarUI)
    // ─────────────────────────────────────────────
    [HideInInspector] public float chargeRatio;   // 0 = ยังไม่ชาร์จ, 1 = เต็ม
    [HideInInspector] public bool  isFullCharge;  // true ตอนชาร์จครบ

    // ─────────────────────────────────────────────
    //  Private
    // ─────────────────────────────────────────────
    private Rigidbody2D rb;
    private float       chargeStartTime;
    private bool        isCharging;
    private bool        isGrounded = true;
    private float       horizontalInput;
    private float       facingDir = 1f;   // +1 ขวา, -1 ซ้าย

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

        // จำทิศทางที่หันล่าสุด (สำหรับ Jump King ทิศ = ทิศที่หัน)
        if (horizontalInput != 0)
        {
            facingDir = horizontalInput;
            // พลิก sprite ตามทิศทาง
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDir;
            transform.localScale = scale;
        }

        // ── 2. Start charging ────────────────────
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCharging)
        {
            isCharging     = true;
            isFullCharge   = false;
            chargeStartTime = Time.time;
            chargeRatio    = 0f;

            // หยุดการเดินขณะชาร์จ (Jump King mechanic)
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
        // ── 5. Ground movement ───────────────────
        if (isGrounded && !isCharging)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        // ── 6. Air resistance  F = -k·v ──────────
        if (!isGrounded)
        {
            Vector2 drag = -rb.linearVelocity * airDragCoefficient;
            rb.AddForce(drag);
        }
    }

    // ─────────────────────────────────────────────
    //  Jump (Projectile Motion)
    // ─────────────────────────────────────────────
    /// <param name="ratio">0–1 ระดับ charge</param>
    void Jump(float ratio)
    {
        // ── Interpolate power ────────────────────
        // ใช้ Lerp ให้แม่นยำ: ratio=0 → minPower, ratio=1 → maxPower
        float power = Mathf.Lerp(minJumpPower, maxJumpPower, ratio);

        // ── Projectile velocity components ───────
        // Vx = power · cos(θ)   Vy = power · sin(θ)
        float angleRad = jumpAngle * Mathf.Deg2Rad;
        Vector2 launchVelocity = new Vector2(
            facingDir * Mathf.Cos(angleRad) * power,   // แนวนอน
            Mathf.Sin(angleRad) * power                 // แนวตั้ง
        );

        // ── Apply as Impulse (F = m·v) ───────────
        // ForceMode2D.Impulse คูณ mass ให้อัตโนมัติ → velocity เปลี่ยนทันที
        rb.linearVelocity = Vector2.zero;                        // clear velocity เก่า
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
    //  Collision
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
}
