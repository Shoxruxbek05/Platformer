using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 10;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Qahramon qayerdaligini topib, o'sha tomonga qarab uchish
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Uchish yo'nalishini hisoblash
            Vector2 direction = (player.transform.position - transform.position).normalized;
            rb.velocity = direction * speed;
        }
        
        // Agar o'q hech kimga tegmasa, 3 sekunddan keyin o'z-o'zidan yo'qoladi (xotirani to'ldirmaslik uchun)
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Agar qahramonga tegsa
        if (hitInfo.CompareTag("Player"))
        {
            PlayerStats stats = hitInfo.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage); // Jonini oladi
            }
            Destroy(gameObject); // O'q yo'qoladi
        }
        // Agar devorga yoki yerga tegsa ham yo'qoladi (Enemy qatlamiga tegmasa bo'ldi)
        else if (hitInfo.gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
