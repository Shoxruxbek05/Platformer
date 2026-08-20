using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 30;
    private int currentHealth;

    [Header("O'lim Sozlamalari")]
    public float deathDelay = 0f; // Agar 0 bo'lsa darhol yo'qoladi, kattaroq bo'lsa kutadi
    public GameObject coinPrefab; // O'lganda tushib qoladigan tanga
    public GameObject xpPrefab;   // O'lganda tushib qoladigan XP

    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + " ga zarba tegdi! Qolgan jon: " + currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(gameObject.name + " o'ldi!");

        // O'lim animatsiyasini ishga tushirish
        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        // Harakatlanmasligi va havoda qotib qolmasligi uchun
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // O'yinchi endi urilib ketavermasligi uchun kolleyderni o'chiramiz
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null)
        {
            coll.enabled = false; 
        }

        // Tanga yig'ish qismi (Agar prefab bo'lsa)
        if (coinPrefab != null) Instantiate(coinPrefab, transform.position, Quaternion.identity);
        
        // XP yig'ish qismi (Agar prefab bo'lsa uni tashlaydi, lekin eng ishonchlisi avtomatik qo'shishdir)
        if (xpPrefab != null) Instantiate(xpPrefab, transform.position + new Vector3(0.5f, 0, 0), Quaternion.identity);

        // O'yinchiga to'g'ridan to'g'ri XP berish (Sahnadan tashqariga chiqib ketmasligi yoki yo'qolib qolmasligi uchun)
        PlayerStats pStats = FindObjectOfType<PlayerStats>();
        if (pStats != null)
        {
            if (gameObject.name.ToLower().Contains("boss")) pStats.AddXP(30);
            else pStats.AddXP(5);
        }

        // O'yindan o'chirishni animatsiyaga qarab kechiktirish
        Destroy(gameObject, deathDelay);
    }
}
