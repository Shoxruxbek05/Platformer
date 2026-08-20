using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Zarba Sozlamalari")]
    public Transform attackPoint;      // Zarba berish nuqtasi (qilichingiz qayerdan urishi)
    public float attackRange = 0.5f;   // Zarba masofasi radiusi
    public LayerMask enemyLayers;      // Qaysi qatlam(Layer)dagilar dushman hisoblanadi

    public float attackRate = 2f;      // Sekundiga necha marta zarba bera olishi
    private float nextAttackTime = 0f;

    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        // Vaqti kelsa hujum qila olishi uchun
        if (Time.time >= nextAttackTime)
        {
            // Z tugmasi yoki sichqonchaning chap tugmasi bosilganda uradi
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void Attack()
    {
        // TODO: Animatsiyani ishga tushirish qismi keyinchalik qo'shiladi

        // Doira ichidagi barcha dushmanlarni topish
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Agar birorta dushmanga tegmasa ham ekranga yozuv chiqarish (siz ishonch hosil qilish uchun)
        if (hitEnemies.Length == 0)
        {
            Debug.Log("Havoga zarba berildi! (Zarba masofasida dushman yo'q)");
        }

        // Topilgan dushmanlarga zarar (damage) berish
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Qilich tegdi: " + enemy.name + " (" + stats.currentDamage + " damage)");
            
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(stats.currentDamage);
            }
        }
    }

    // Unity muharririda zarba doirasini qizil rangda ko'rsatib turish uchun funksiya
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
