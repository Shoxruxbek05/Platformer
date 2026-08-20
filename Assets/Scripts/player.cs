using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Harakat Sozlamalari")]
    public float moveSpeed = 8f;            // Yurish tezligi
    public float jumpForce = 16f;           // Sakrash kuchi

    [Header("Dash Sozlamalari")]
    public float dashSpeed = 1500f;         // Dash (siljish) tezligi. Buni Inspektorda Move Speed'dan kattaroq qilib qo'yasiz.
    public float dashDuration = 0.2f;       // Dash qancha vaqt davom etishi (sekund)
    public float dashCooldown = 1f;         // Keyingi dashgacha kutish vaqti (sekund)

    [Header("Qulash Sozlamalari")]
    public float fallMultiplier = 2.5f;     // Pastga tushishni tezlashtirish koeffitsiyenti

    [Header("Yerni Tekshirish")]
    public Transform groundCheck;           // Oyoq ostidagi tekshiruvchi nuqta
    public float groundCheckRadius = 0.2f;  // Tekshirish radiusi
    public LayerMask groundLayer;           // Yer qatlami (Ground Layer)

    private float horizontalInput;
    private bool isGrounded;
    private Rigidbody2D rb;
    private Animator anim;                  // Animatsiyalar uchun Animator
    private bool isFacingRight = false;     // Qahramon rasmi aslida chapga qarab turgani uchun buni false qilamiz

    // Double Jump va Dash uchun o'zgaruvchilar
    private bool canDoubleJump;
    private bool isDashing;
    private bool canDash = true;

    void Start()
    {
        // Inspector'dagi dastlabki tezlikni (masalan 100) eslab qolamiz. 
        // Xotiradan faqat Do'kon va Level lardan yig'ilgan bonuslarni hisoblab qo'shamiz:
        int speedCost = PlayerPrefs.GetInt("ShopSpeedCost", 10);
        int shopUpgrades = (speedCost - 10) / 5; // Do'kondan necha marta olingani
        int level = PlayerPrefs.GetInt("PlayerLevel", 1); // Qahramon Leveli

        // Jami tezlik = Asl tezlik (100) + Do'kon (har biriga +2) + Level (har biriga +5)
        moveSpeed = moveSpeed + (shopUpgrades * 2f) + ((level - 1) * 5f);

        // Skript biriktirilgan obyektdan komponentlarni olish
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        // Root Motion ni kod orqali majburlab o'chiramiz (joyida qotib qolish xatosini tuzatish uchun)
        if (anim != null)
        {
            anim.applyRootMotion = false;
        }
    }

    void Update()
    {
        // Agar dash qilinayotgan bo'lsa, qolgan kodlarni o'qimay turamiz (faqat dash bo'ladi)
        if (isDashing)
        {
            return;
        }

        // 1. Kiritishlarni (Input) olish (A/D yoki Chap/O'ng strelkalar)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Sakrash va Double Jump
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                canDoubleJump = true; // Birinchi marta sakraganda double jump ga ruxsat beramiz
                anim.SetTrigger("Jump"); // Sakrash animatsiyasi (agar trigger ishlatsangiz)
            }
            else if (canDoubleJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                canDoubleJump = false; // Havoda ikkinchi marta sakradi, endi ruxsat yo'q
                anim.SetTrigger("DoubleJump"); // Ikkinchi sakrash animatsiyasi
            }
        }

        // 3. Dash (Shift tugmasi bosilganda)
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        // 4. Sakrash tugmasi erta qo'yib yuborilsa, sakrashni qisqartirish
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // 5. Yerga tushishni tezlashtirish (Fall Multiplier)
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        // 6. Harakat yo'nalishiga qarab qahramonni burish
        Flip();

        // ERNI SEZAYOTGANINI KONSOLGA CHIQARISH:
        // Debug.Log("Yerga tegib turibdimi: " + isGrounded);

        // 7. Hujum qilish (Fight animatsiyasi uchun tayyorgarlik)
        if (Input.GetMouseButtonDown(0)) // Sichqonchani chap tugmasi
        {
            anim.SetTrigger("Fight");
            // Hujum qilish logikasi shu yerga yoziladi
        }

        // --- ANIMATSIYALARNI YANGILASH ---
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("YVelocity", rb.velocity.y);
    }

    void FixedUpdate()
    {
        // Agar dash bo'layotgan bo'lsa, gorizontal harakatni o'zgartirmaymiz
        if (isDashing)
        {
            return;
        }

        // Qahramon yerda turganligini tekshirish
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            Debug.LogWarning("DIQQAT: Qahramonda 'Ground Check' joyi belgilanmagan, shuning uchun u sakray olmaydi!");
        }

        // Agar yerda bo'lsa, double jump imkoniyatini tiklaymiz (tepadan yiqilganda ham ishlashi uchun)
        if (isGrounded && rb.velocity.y <= 0)
        {
            canDoubleJump = true;
        }

        // Obyektni gorizontal harakatlantirish
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    // Qahramon yuzini harakat yo'nalishiga qarab o'girish funksiyasi
    private void Flip()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f; // X o'qi bo'yicha masshtabni teskarisiga aylantirish
            transform.localScale = localScale;
        }
    }

    // Dash amali (Coroutine orqali vaqt bilan boshqariladi)
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        anim.SetBool("IsDashing", true); // Dash animatsiyasini yoqish
        
        // Hozirgi tortishish kuchini eslab qolamiz va o'chiramiz (dash havoda to'g'ri chiziq bo'ylab bo'lishi uchun)
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        
        // Qaysi tomonga qarab turgan bo'lsa o'sha tomonga dash qilish
        float dashDirection = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);
        
        // Dash vaqti tugashini kutish
        yield return new WaitForSeconds(dashDuration);
        
        // Hamma narsani joyiga qaytarish
        rb.gravityScale = originalGravity;
        isDashing = false;
        anim.SetBool("IsDashing", false); // Dash animatsiyasini o'chirish
        
        // Keyingi dash uchun cooldown (kutish) vaqti
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // Do'kondan yoki Level Up bo'lganda tezlikni oshirish uchun
    public void UpgradeSpeed(float amount)
    {
        moveSpeed += amount;
    }
}
