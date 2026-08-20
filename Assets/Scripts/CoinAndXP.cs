using UnityEngine;

public class CoinAndXP : MonoBehaviour
{
    [Header("Sozlamalar")]
    public bool isCoin = true;  // Agar belgilansa Tanga bo'ladi, belgilanmasa XP bo'ladi
    public int amount = 10;     // Qancha tanga yoki XP berishi

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Tegib ketgan obyekt "Player" tegiga (Tag) ega ekanligini tekshirish
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null)
            {
                if (isCoin)
                {
                    stats.AddCoins(amount);
                    Debug.Log("Tanga olindi: " + amount + ". Jami Tangalar: " + stats.coins);
                }
                else
                {
                    stats.AddXP(amount);
                    Debug.Log("XP olindi: " + amount + ". Jami XP: " + stats.currentXP);
                }
            }
            
            // Obyektni (tangani) o'yindan yo'q qilish
            Destroy(gameObject);
        }
    }
}
