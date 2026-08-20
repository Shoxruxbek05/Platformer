using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BasicMelee : MonoBehaviour
{
    [Header("Sozlamalar")]
    public float speed = 3f;
    public float detectionRange = 5f; 
    public float attackRange = 1f;    
    public float verticalAttackRange = 2f; // Vertikal (tepa-past) masofa tolerance
    public int damage = 10;
    public float attackRate = 1.5f;
    public LayerMask groundLayer; 
    public float patrolDistance = 5f; 

    private float nextAttackTime = 0f;
    private Transform player;
    private Rigidbody2D rb;
    private Collider2D coll;
    private Animator anim;
    
    private int patrolDirection = 1; 
    private float leftEdge;
    private float rightEdge;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        
        // Animatsiya qahramonni joyiga qulflab qo'ymasligi uchun
        if (anim != null)
        {
            anim.applyRootMotion = false;
        }

        leftEdge = transform.position.x - patrolDistance;
        rightEdge = transform.position.x + patrolDistance;

        FindPlayer();
    }

    void FindPlayer()
    {
        // Tag orqali emas, aniq PlayerStats bor obyekti topish (kameralarga adashmasligi uchun)
        PlayerStats p = FindObjectOfType<PlayerStats>();
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            Patrol(); 
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        float distanceX = Mathf.Abs(transform.position.x - player.position.x);
        float distanceY = Mathf.Abs(transform.position.y - player.position.y);

        if (distance <= detectionRange)
        {
            // Agar yonga qarab hujum masofasida bo'lsa VA tepa-pastlik verticalAttackRange dan oshmasa hujum qiladi
            if (distanceX <= attackRange && distanceY <= verticalAttackRange)
            {
                // O'yinchi tomonga burilish
                float direction = player.position.x > transform.position.x ? 1f : -1f;
                rb.velocity = new Vector2(0, rb.velocity.y); // Oddiy dushman urish paytida to'xtaydi
                patrolDirection = (int)direction;
                Flip(direction);
                
                if (anim != null) anim.SetBool("IsWalking", false);

                if (Time.time >= nextAttackTime)
                {
                    AttackPlayer();
                    nextAttackTime = Time.time + 1f / attackRate;
                }
            }
            else
            {
                // Hujumga yeta olmasa quvishda davom etadi
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

    void AttackPlayer()
    {
        if (anim != null) anim.SetTrigger("Attack");
        Debug.Log(gameObject.name + " sizga oddiy zarba berdi!");
        
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
