using UnityEngine;

public class Trap : MonoBehaviour
{
    public int trapDamage = 15;
    public float knockbackForce = 15f; // Tuzoqqa tekkanda orqaga otilib ketish kuchi

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats stats = collision.gameObject.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(trapDamage);
                Debug.Log("Tuzoqqa tushdingiz! Qolgan jon: " + stats.currentHealth);
                
                // Qahramonni orqaga otib yuborish (fizika orqali)
                Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Tuzoq va qahramon o'rtasidagi yo'nalish
                    Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                    knockbackDirection.y = 0.5f; // Sal tepaga sakratib yuborishi uchun
                    
                    rb.velocity = Vector2.zero; // Avvalgi tezlikni nol qilish
                    rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}
