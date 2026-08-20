using UnityEngine;
using TMPro; // TextMeshPro (TMP) bilan ishlash uchun kutubxona

public class UIManager : MonoBehaviour
{
    [Header("Qahramon")]
    public PlayerStats playerStats; // Qahramondagi PlayerStats skriptini shu yerga ulaymiz

    [Header("UI Yozuvlar (Text TMP)")]
    public TextMeshProUGUI levelText; // Levelni ko'rsatuvchi yozuv
    public TextMeshProUGUI coinsText; // Tangalarni ko'rsatuvchi yozuv
    public TextMeshProUGUI xpText;    // XP ni ko'rsatuvchi yozuv
    public TextMeshProUGUI healthText; // Jonni ko'rsatuvchi yozuv

    void Update()
    {
        // Agar qahramon ulangan bo'lsa, har freymda ekrandagi raqamlarni yangilaymiz
        if (playerStats != null)
        {
            if (levelText != null)
            {
                levelText.text = playerStats.currentLevel.ToString();
            }
            
            if (coinsText != null)
            {
                coinsText.text = playerStats.coins.ToString();
            }
                
            if (xpText != null)
            {
                xpText.text = playerStats.currentXP.ToString() + " / " + playerStats.xpToNextLevel.ToString();
            }

            if (healthText != null)
            {
                healthText.text = playerStats.currentHealth.ToString() + " / " + playerStats.maxHealth.ToString();
            }
        }
    }
}
