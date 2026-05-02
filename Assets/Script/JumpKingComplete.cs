using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class FurballAdventure : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────────
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float airDragCoefficient = 0.5f;

    [Header("Jump Settings")]
    public float maxChargeTime = 1.5f;
    public float jumpAngle = 65f;
    public float minJumpPower = 3f;
    public float maxJumpPower = 14f;

    [Header("Water Settings")]
    public float waterMoveSpeed = 3f;
    public float waterJumpPowerScale = 0.7f;
    public float waterDrag = 2f;

    [Header("Sound Effects")]
    public AudioClip chargeStartSound;  // เสียงตอนเริ่มชาร์จ
    public AudioClip chargeFullSound;   // เสียงตอนชาร์จเต็ม
    public AudioClip jumpSound;         // เสียงตอนกระโดด
    public AudioClip landSound;         // เสียงตอนลงพื้น
    public AudioClip waterSplashSound;  // เสียงตอนตกน้ำ

    [Range(0f, 1f)] public float sfxVolume = 1f;

    // ─────────────────────────────────────────────
    //  Public State (อ่านโดย ChargeBarUI)
    // ─────────────────────────────────────────────
    [HideInInspector] public float chargeRatio;
    [HideInInspector] public bool isFullCharge;

    // ─────────────────────────────────────────────
    //  Private
    // ─────────────────────────────────────────────
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private float chargeStartTime;
    private bool isCharging;
    private bool isGrounded = true;
    private bool isInWater = false;
    private bool wasFullCharge = false;  // ป้องกันเสียง chargeFullSound เล่นซ้ำ
    private float horizontalInput;
    private float facingDir = 1f;

    // ─────────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
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

        // ── 2. Start charging ────────────────────
        bool canJump = isGrounded || isInWater;

        if (Input.GetKeyDown(KeyCode.Space) && canJump && !isCharging)
        {
            isCharging = true;
            isFullCharge = false;
            wasFullCharge = false;
            chargeStartTime = Time.time;
            chargeRatio = 0f;

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            PlaySound(chargeStartSound);  // 🔊 เสียงเริ่มชาร์จ
        }

        // ── 3. Update charge ratio ───────────────
        if (isCharging)
        {
            float elapsed = Time.time - chargeStartTime;
            chargeRatio = Mathf.Clamp01(elapsed / maxChargeTime);
            isFullCharge = (chargeRatio >= 1f);

            // 🔊 เสียงชาร์จเต็ม (เล่นครั้งเดียว)
            if (isFullCharge && !wasFullCharge)
            {
                wasFullCharge = true;
                PlaySound(chargeFullSound);
            }
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
        // ── 5. Movement ──────────────────────────
        if (!isCharging)
        {
            if (isGrounded)
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
            else if (isInWater)
                rb.linearVelocity = new Vector2(horizontalInput * waterMoveSpeed, rb.linearVelocity.y);
        }

        // ── 6. Air resistance  F = -k·v ──────────
        if (!isGrounded && !isInWater)
        {
            Vector2 drag = -rb.linearVelocity * airDragCoefficient;
            rb.AddForce(drag);
        }

        // ── 7. Water drag ────────────────────────
        if (isInWater)
        {
            Vector2 waterResistance = -rb.linearVelocity * waterDrag;
            rb.AddForce(waterResistance);
        }
    }

    // ─────────────────────────────────────────────
    //  Jump
    // ─────────────────────────────────────────────
    void Jump(float ratio)
    {
        float power = Mathf.Lerp(minJumpPower, maxJumpPower, ratio);
        if (isInWater) power *= waterJumpPowerScale;

        float angleRad = jumpAngle * Mathf.Deg2Rad;
        Vector2 launchVelocity = new Vector2(
            facingDir * Mathf.Cos(angleRad) * power,
            Mathf.Sin(angleRad) * power
        );

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(launchVelocity * rb.mass, ForceMode2D.Impulse);
        isGrounded = false;

        PlaySound(jumpSound);  // 🔊 เสียงกระโดด
    }

    void ResetCharge()
    {
        isCharging = false;
        isFullCharge = false;
        wasFullCharge = false;
        chargeRatio = 0f;
    }

    // ─────────────────────────────────────────────
    //  Sound Helper
    // ─────────────────────────────────────────────
    void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, sfxVolume);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            PlaySound(landSound);  // 🔊 เสียงลงพื้น
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

   
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = true;
            PlaySound(waterSplashSound);  // 🔊 เสียงตกน้ำ
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
            isInWater = false;
    }
}