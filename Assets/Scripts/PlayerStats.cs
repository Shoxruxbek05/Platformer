using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("Asosiy Ko'rsatkichlar")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Level va XP")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Boylik")]
    public int coins = 0;

    [Header("Kuch (Damage)")]
    public int baseDamage = 10;
    public int currentDamage;

    // Hodisalar (UI ni yangilash uchun kelajakda kerak bo'ladi)
    public UnityEvent onHealthChanged;
    public UnityEvent onLevelUp;
    public UnityEvent onCoinsChanged;
    public UnityEvent onPlayerDeath;

    void Start()
    {
        // Saqlangan ma'lumotlarni o'qib olish (Tangalar, Level, XP)
        coins = PlayerPrefs.GetInt("TotalCoins", 0);
        currentLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        currentXP = PlayerPrefs.GetInt("PlayerXP", 0);

        // Jon va Kuchni Inspector dagi yozilganidan kelib chiqib hisoblaymiz:
        // Unga faqat Do'kon va Level dan olingan bonuslar qo'shiladi!
        int healthCost = PlayerPrefs.GetInt("ShopHealthCost", 10);
        int damageCost = PlayerPrefs.GetInt("ShopDamageCost", 10);
        
        int healthUpgrades = (healthCost - 10) / 5;
        int damageUpgrades = (damageCost - 10) / 5;

        // Jami Jon = Asl jon (Inspectordan) + Do'kon (+2 dan) + Level (+5 dan)
        maxHealth = maxHealth + (healthUpgrades * 2) + ((currentLevel - 1) * 5);
        baseDamage = baseDamage + (damageUpgrades * 2) + ((currentLevel - 1) * 5);

        xpToNextLevel = 100 + (currentLevel - 1) * 50; // XP formulasi

        currentHealth = maxHealth;
        currentDamage = baseDamage;
        
        // Boshlang'ich UI ni yangilash
        onCoinsChanged?.Invoke();
        onLevelUp?.Invoke();
        onHealthChanged?.Invoke();
    }

    // Barcha o'zgarishlarni xotiraga yozib qoldirish
    public void SaveStats()
    {
        PlayerPrefs.SetInt("TotalCoins", coins);
        PlayerPrefs.SetInt("PlayerLevel", currentLevel);
        PlayerPrefs.SetInt("PlayerXP", currentXP);
        PlayerPrefs.Save();
    }

    // Jon kamayishi
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        onHealthChanged?.Invoke();
    }

    // Jon to'lishi
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        onHealthChanged?.Invoke();
    }

    // Tangalar yig'ish
    public void AddCoins(int amount)
    {
        coins += amount;
        SaveStats(); // Tangani doimiy saqlash
        onCoinsChanged?.Invoke();
    }

    // Do'konda pul sarflash
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            SaveStats(); // Tangani kamaygandan keyin ham saqlash
            onCoinsChanged?.Invoke();
            return true;
        }
        return false;
    }

    // XP va Level Up tizimi
    public void AddXP(int amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
        SaveStats(); // XP o'zgarganda saqlaymiz
    }

    private void LevelUp()
    {
        currentLevel++;
        xpToNextLevel = 100 + (currentLevel - 1) * 50; // Har levelda 50 tadan oshib boradi
        
        maxHealth += 5; // Hamma xususiyatga 5 qo'shiladi
        currentHealth = maxHealth; // Jon yana to'liq to'ladi
        baseDamage += 5;
        currentDamage = baseDamage;
        
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.UpgradeSpeed(5f); // Tezlik ham oshadi
        }

        SaveStats(); // O'sishni doimiy saqlash
        
        onLevelUp?.Invoke();
        onHealthChanged?.Invoke();
        Debug.Log("Level Oshdi! Yangi Level: " + currentLevel);
    }

    // Do'kon uchun alohida kuchaytirish funksiyalari
    public void UpgradeMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount; // Qo'shilgan joni darhol to'ladi
        onHealthChanged?.Invoke();
    }

    public void UpgradeDamage(int amount)
    {
        baseDamage += amount;
        currentDamage = baseDamage;
    }

    private void Die(bool isFallDeath = false)
    {
        Debug.Log("Qahramon o'ldi! O'yin tugadi.");
        
        // Qahramonni qotirib, o'lish animatsiyasini qo'yish
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Death");
        
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false; // Yurishni to'xtatish
        
        // Agar yiqilib o'layotgan bo'lsa (deatline), havo da qotib qolmasligi uchun bularni o'chirmaymiz
        if (!isFallDeath)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            Collider2D coll = GetComponent<Collider2D>();
            if (coll != null) coll.enabled = false;
        }

        StartCoroutine(DeathRoutine());
    }

    private System.Collections.IEnumerator DeathRoutine()
    {
        // Animatsiya o'ynashi uchun biroz kutish (1.5 soniya)
        yield return new WaitForSeconds(1.5f);
        
        onPlayerDeath?.Invoke();
        
        // GameManager orqali Lose panelini avtomatik chiqarish
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.GameOver();
    }

    // --- Muhit (Deatline, Portal) bilan to'qnashuvlar ---
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckEnvironmentCollision(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckEnvironmentCollision(collision.gameObject);
    }

    private void CheckEnvironmentCollision(GameObject obj)
    {
        // 1. Deatline ga tushib ketsa
        if (obj.layer == LayerMask.NameToLayer("deatline") || obj.name.ToLower().Contains("deatline"))
        {
            if (currentHealth > 0)
            {
                currentHealth = 0;
                onHealthChanged?.Invoke(); // Jon nol bo'lganini UI ga bildirish
                Die(true); // true = pastga tushib ketishda davom etishi uchun qotirib qo'ymaydi
            }
        }

        // 2. Portal ga kirsa (Yutish)
        if (obj.name.ToLower().Contains("portal"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null) gm.WinLevel();
            
            // O'yinchi portalga kirganda g'oyib bo'lishi uchun o'zini o'chirib qo'yadi
            gameObject.SetActive(false); 
        }
    }
}
