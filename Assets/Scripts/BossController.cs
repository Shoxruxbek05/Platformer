using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyMelee : MonoBehaviour
{
    [Header("Sozlamalar")]
    public float speed = 3f;
    public float detectionRange = 8f; // Qahramonni ko'rish masofasi
    public float fireRange = 4f;      // Olov purkash masofasi (O'rta masofa)
    public float attackRange = 1.5f;  // Urish masofasi (Yaqin masofa)
    public float verticalAttackRange = 3f; // Vertikal masofa
    public int damage = 10;
    public float attackRate = 1.5f;
    public LayerMask groundLayer; // Jarlikni aniqlash uchun
    public float patrolDistance = 5f; // <--- Masofani belgilash

    [Header("Boss UI Sozlamalari")]
    public GameObject healthPanel; // UI qutichasi (Canvas ichidagi panel)
    public UnityEngine.UI.Slider healthSlider; // Jonni ko'rsatuvchi Slider

    [Header("Boss Enrage Sozlamalari")]
    public float enrageSpeedMultiplier = 1.5f;
    public float enrageAttackRateMultiplier = 2f;
    private bool isEnraged = false;
    private EnemyHealth healthScript;

    private float nextAttackTime = 0f;
    private Transform player;
    private Rigidbody2D rb;
    private Collider2D coll;
    private Animator anim;
    
    private int patrolDirection = 1; // 1 = o'ngga, -1 = chapga
    private float leftEdge;
    private float rightEdge;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        healthScript = GetComponent<EnemyHealth>();
        
        leftEdge = transform.position.x - patrolDistance;
        rightEdge = transform.position.x + patrolDistance;

        FindPlayer();
    }

    void FindPlayer()
    {
        // Tag orqali emas, aniq PlayerStats bor obyekti topish
        PlayerStats p = FindObjectOfType<PlayerStats>();
        if (p != null) player = p.transform;
    }

    void Update()
    {
        CheckEnrage();
        UpdateUI();

        // Agar o'yinchi topilmagan bo'lsa, qidirib ko'ramiz
        if (player == null)
        {
            FindPlayer();
            Patrol(); // Ungacha pultrulyatsiya qilaversin
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        float distanceX = Mathf.Abs(transform.position.x - player.position.x);
        float distanceY = Mathf.Abs(transform.position.y - player.position.y);

        if (distance <= detectionRange)
        {
            bool canAttackVertical = distanceY <= verticalAttackRange;

            if (distanceX <= attackRange && canAttackVertical)
            {
                // Dushman to'xtamasdan o'yinchi tomonga yurishda davom etadi va qisqa masofadan uradi
                float direction = player.position.x > transform.position.x ? 1f : -1f;
                rb.velocity = new Vector2(direction * speed, rb.velocity.y);
                patrolDirection = (int)direction;
                Flip(direction);
                
                if (anim != null) anim.SetBool("IsWalking", true);

                if (Time.time >= nextAttackTime)
                {
                    AttackPlayer();
                    nextAttackTime = Time.time + 1f / attackRate;
                }
            }
            else if (distanceX <= fireRange && canAttackVertical)
            {
                // O'yinchi tomonga burilib to'xtaydi va olov purkaydi
                float direction = player.position.x > transform.position.x ? 1f : -1f;
                rb.velocity = new Vector2(0, rb.velocity.y);
                patrolDirection = (int)direction;
                Flip(direction);
                
                if (anim != null) anim.SetBool("IsWalking", false);

                if (Time.time >= nextAttackTime)
                {
                    FireAttackPlayer();
                    // Olov purkash ancha kuchli bo'lgani uchun uni sal sekinlashtirish mumkin
                    nextAttackTime = Time.time + 1.5f / attackRate; 
                }
            }
            else
            {
                // Agar vertikal yoki gorizontal masofa mos kelmasa, quvishda davom etadi
                ChasePlayer();
            }
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        // Belgilangan masofaga yetganda orqaga qaytadi
        if (transform.position.x >= rightEdge)
        {
            patrolDirection = -1;
        }
        else if (transform.position.x <= leftEdge)
        {
            patrolDirection = 1;
        }

        rb.velocity = new Vector2(patrolDirection * speed, rb.velocity.y);
        if (anim != null) anim.SetBool("IsWalking", true);
        Flip((float)patrolDirection);
    }

    void ChasePlayer()
    {
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        
        Vector2 checkPos = transform.position;
        checkPos.x += direction * (coll.bounds.extents.x + 0.1f);
        checkPos.y -= coll.bounds.extents.y + 0.1f;

        RaycastHit2D groundInfo = Physics2D.Raycast(checkPos, Vector2.down, 1.5f, groundLayer);

        bool hasGround = groundInfo.collider != null && groundInfo.collider.gameObject != gameObject;

        // Jarlik bo'lmasa qahramonga yuguradi
        if (hasGround)
        {
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
            patrolDirection = (int)direction;
            if (anim != null) anim.SetBool("IsWalking", true);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            if (anim != null) anim.SetBool("IsWalking", false);
        }

        Flip(direction);
    }

    void Flip(float dir)
    {
        Vector3 scale = transform.localScale;
        scale.x = dir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void UpdateUI()
    {
        if (healthPanel != null && healthSlider != null && healthScript != null)
        {
            float distance = player != null ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;
            
            // Qahramon ko'rish masofasiga kirsa UIni ko'rsatamiz
            if (distance <= detectionRange)
            {
                healthPanel.SetActive(true);
                healthSlider.maxValue = healthScript.maxHealth;
                healthSlider.value = healthScript.GetCurrentHealth();
                
                // Agar boss o'lsa, UI yo'qoladi
                if (healthScript.GetCurrentHealth() <= 0)
                {
                    healthPanel.SetActive(false);
                }
            }
            else
            {
                healthPanel.SetActive(false);
            }
        }
    }

    void CheckEnrage()
    {
        if (!isEnraged && healthScript != null)
        {
            // Joni yarmidan kamayganda quturadi
            if (healthScript.GetCurrentHealth() <= healthScript.maxHealth / 2)
            {
                isEnraged = true;
                speed *= enrageSpeedMultiplier;
                attackRate *= enrageAttackRateMultiplier;
                Debug.Log(gameObject.name + " QUTURDI! Tezligi va urish kuchi oshdi!");
                
                // Qizarib ketishi
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 0.5f, 0.5f);
            }
        }
    }

    void FireAttackPlayer()
    {
        if (anim != null)
        {
            anim.SetTrigger("FireAttack");
        }
        
        Debug.Log(gameObject.name + " sizga OLOV purkadi!");
        if (player != null)
        {
            PlayerStats pStats = player.GetComponent<PlayerStats>();
            if (pStats != null)
            {
                pStats.TakeDamage(damage + 5); 
            }
        }
    }

    void AttackPlayer()
    {
        if (anim != null)
        {
            int randomAttack = Random.Range(0, 2);
            anim.SetInteger("AttackIndex", randomAttack);
            anim.SetTrigger("Attack");
        }
        
        Debug.Log(gameObject.name + " sizga zarba berdi!");
        if (player != null)
        {
            PlayerStats pStats = player.GetComponent<PlayerStats>();
            if (pStats != null)
            {
                pStats.TakeDamage(damage);
            }
        }
    }
}
