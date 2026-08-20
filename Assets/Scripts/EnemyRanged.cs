using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyRanged : MonoBehaviour
{
    [Header("Patrul (Tinimsiz yurish)")]
    public float patrolDistance = 5f;  // Qancha masofaga yurishi
    public float speed = 2f;           // Yurish tezligi
    
    [Header("Otish Sozlamalari")]
    public float detectionRange = 10f; // Qahramonni ko'rish masofasi
    public float fireRate = 1.5f;      // Sekundiga necha marta otishi
    private float nextFireTime = 0f;

    [Header("O'q Sozlamalari")]
    public GameObject projectilePrefab; // Otiladigan o'q (Prefab)
    public Transform firePoint;         // O'q chiqadigan joy

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    
    private float leftEdge;
    private float rightEdge;
    private int patrolDirection = 1; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        // Sahnadan qahramonni topamiz
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Yurish chegaralarini belgilash
        leftEdge = transform.position.x - patrolDistance;
        rightEdge = transform.position.x + patrolDistance;
    }

    void Update()
    {
        bool isShooting = false;

        // 1. Qahramonni ko'rsa to'xtab o'q otish
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= detectionRange)
            {
                isShooting = true;
                rb.velocity = new Vector2(0, rb.velocity.y); // Joyida to'xtaydi
                
                // Yuzini qahramon tomonga burish
                Vector3 scale = transform.localScale;
                scale.x = player.position.x > transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;

                if (Time.time >= nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
        }

        // 2. Qahramon uzoqda bo'lsa, Patrul qilish (O'ng va chapga yurish)
        if (!isShooting)
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

            // Yuzini yurayotgan tomonga burish
            Vector3 scale = transform.localScale;
            scale.x = patrolDirection > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Agar yurayotgan bo'lsa true, to'xtab o'q uzayotgan bo'lsa false bo'ladi
        if (anim != null) anim.SetBool("IsWalking", !isShooting);
    }

    void Shoot()
    {
        if (anim != null) anim.SetTrigger("Shoot");
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
    }

    // Unity muharririda ko'rish maydonini chizish
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
