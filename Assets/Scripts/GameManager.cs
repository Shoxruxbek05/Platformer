using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI; // Slayderlar uchun kerak

public class GameManager : MonoBehaviour
{
    [Header("UI Panellar")]
    public GameObject startMenuPanel;
    public GameObject inGamePanel;
    public GameObject pauseMenuPanel;
    public GameObject losePanel;
    public GameObject winPanel;
    public GameObject shopPanel;

    [Header("Do'kon va UI Yozuvlari")]
    public TextMeshProUGUI healthCostText;
    public TextMeshProUGUI speedCostText;
    public TextMeshProUGUI damageCostText;
    public TextMeshProUGUI startMenuCoinsText; // Start menyudagi jami tangalar
    public TextMeshProUGUI shopMenuCoinsText;  // Do'kondagi jami tangalar

    [Header("Do'kon Slayderlari")]
    public Slider healthSlider;
    public Slider speedSlider;
    public Slider damageSlider;

    [Header("Slayder Ranglari")]
    public Color emptyColor = Color.white;
    public Color fullColor = Color.green;

    [Header("Hozirgi Ko'rsatkichlar Yozuvlari")]
    public TextMeshProUGUI currentHealthText;
    public TextMeshProUGUI currentSpeedText;
    public TextMeshProUGUI currentDamageText;

    [Header("Qahramon")]
    public PlayerStats playerStats;
    public PlayerMovement playerMovement; 

    private int healthCost;
    private int speedCost;
    private int damageCost;
    private bool isGameStarted = false;

    void Start()
    {
        // Do'kon narxlarini xotiradan o'qish (yo'q bo'lsa 10 dan boshlanadi)
        healthCost = PlayerPrefs.GetInt("ShopHealthCost", 10);
        speedCost = PlayerPrefs.GetInt("ShopSpeedCost", 10);
        damageCost = PlayerPrefs.GetInt("ShopDamageCost", 10);

        // O'yin boshida barcha panellarni yopib, faqat Start Menu ni ochamiz
        ShowPanel(startMenuPanel);
        Time.timeScale = 0f; // O'yinni orqa fonda qotirib qo'yadi
        isGameStarted = false;
        
        UpdateShopUI();
    }

    void Update()
    {
        // ESC tugmasi orqali Pause ni yoqish va o'chirish
        if (Input.GetKeyDown(KeyCode.Escape) && isGameStarted)
        {
            if (pauseMenuPanel != null && pauseMenuPanel.activeSelf) 
                ResumeGame();
            else 
                PauseGame();
        }
    }

    // --- O'YIN HOLATLARI FUNKSIYALARI ---
    
    public void StartGame()
    {
        isGameStarted = true;
        Time.timeScale = 1f; // O'yin harakatlanishni boshlaydi
        ShowPanel(inGamePanel);
        UpdateShopUI();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        ShowPanel(pauseMenuPanel);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        ShowPanel(inGamePanel);
    }

    public void GameOver()
    {
        Time.timeScale = 0f; // O'lganda orqa fon qotadi
        ShowPanel(losePanel);
    }

    public void WinLevel()
    {
        Time.timeScale = 0f; // Yutganda qotadi
        ShowPanel(winPanel);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        // Sahnani qayta yuklash avtomat tarzda Start menyuni ochadi
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        // Keyingi sahnani yuklash (Build Settings da qo'shilgan bo'lishi kerak)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("O'yindan chiqildi!");
        Application.Quit();
    }

    // --- DO'KON HOLATLARI ---

    public void OpenShop()
    {
        ShowPanel(shopPanel);
        UpdateShopUI();
    }

    public void CloseShop()
    {
        // Start menyusiga qaytarish
        ShowPanel(startMenuPanel);
        UpdateShopUI();
    }

    // --- KUCHAYTIRISH (UPGRADE) FUNKSIYALARI ---
    
    public void BuyHealth()
    {
        Debug.Log("Jon sotib olish tugmasi bosildi!");
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        Debug.Log("Xotiradagi jami tanga: " + totalCoins + " | So'ralayotgan narx: " + healthCost);
        
        if (totalCoins >= healthCost)
        {
            totalCoins -= healthCost;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            
            healthCost += 5;
            PlayerPrefs.SetInt("ShopHealthCost", healthCost);
            PlayerPrefs.Save();
            
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>(true);
            if (playerStats != null)
            {
                playerStats.coins = totalCoins;
                playerStats.UpgradeMaxHealth(2); // Jonni 2 taga oshirish
            }
            UpdateShopUI();
        }
    }

    public void BuySpeed()
    {
        Debug.Log("Tezlik sotib olish tugmasi bosildi!");
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        Debug.Log("Xotiradagi jami tanga: " + totalCoins + " | So'ralayotgan narx: " + speedCost);
        
        if (totalCoins >= speedCost)
        {
            totalCoins -= speedCost;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            
            speedCost += 5;
            PlayerPrefs.SetInt("ShopSpeedCost", speedCost);
            PlayerPrefs.Save();
            
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>(true);
            if (playerStats != null) playerStats.coins = totalCoins;
            
            if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>(true);
            if (playerMovement != null) playerMovement.UpgradeSpeed(2f); // Tezlik +2
            
            UpdateShopUI();
        }
    }

    public void BuyDamage()
    {
        Debug.Log("Kuch sotib olish tugmasi bosildi!");
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        Debug.Log("Xotiradagi jami tanga: " + totalCoins + " | So'ralayotgan narx: " + damageCost);
        
        if (totalCoins >= damageCost)
        {
            totalCoins -= damageCost;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            
            damageCost += 5;
            PlayerPrefs.SetInt("ShopDamageCost", damageCost);
            PlayerPrefs.Save();
            
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>(true);
            if (playerStats != null)
            {
                playerStats.coins = totalCoins;
                playerStats.UpgradeDamage(2); // Kuch +2
            }
            UpdateShopUI();
        }
    }

    public void UpdateShopUI()
    {
        if (healthCostText != null) healthCostText.text = healthCost.ToString();
        if (speedCostText != null) speedCostText.text = speedCost.ToString();
        if (damageCostText != null) damageCostText.text = damageCost.ToString();
        
        // Slayderlarni yangilash (Har bir sotib olish narxni 5 ga oshirganligi uchun)
        // Shuning uchun necha marta sotib olinganini narxdan kelib chiqib hisoblaymiz
        int healthUpgrades = (healthCost - 10) / 5;
        int speedUpgrades = (speedCost - 10) / 5;
        int damageUpgrades = (damageCost - 10) / 5;

        if (healthSlider != null) 
        {
            healthSlider.value = healthUpgrades;
            UpdateSliderColor(healthSlider);
        }
        if (speedSlider != null) 
        {
            speedSlider.value = speedUpgrades;
            UpdateSliderColor(speedSlider);
        }
        if (damageSlider != null) 
        {
            damageSlider.value = damageUpgrades;
            UpdateSliderColor(damageSlider);
        }

        // Jami tangalarni to'g'ridan-to'g'ri xotiradan olamiz (qahramon o'chirilgan bo'lsa ham ishlaydi)
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (startMenuCoinsText != null) startMenuCoinsText.text = totalCoins.ToString();
        if (shopMenuCoinsText != null) shopMenuCoinsText.text = totalCoins.ToString();

        // Hozirgi ko'rsatkichlarni UI ga yozish
        int level = PlayerPrefs.GetInt("PlayerLevel", 1);
        int currentMaxHealth = 100 + (healthUpgrades * 2) + ((level - 1) * 5);
        int currentBaseDamage = 10 + (damageUpgrades * 2) + ((level - 1) * 5);
        
        if (currentHealthText != null) currentHealthText.text = currentMaxHealth.ToString();
        if (currentDamageText != null) currentDamageText.text = currentBaseDamage.ToString();
        
        // Tezlik hisobi
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>(true);
        if (playerMovement != null && currentSpeedText != null)
        {
            currentSpeedText.text = playerMovement.moveSpeed.ToString();
        }
        else if (currentSpeedText != null)
        {
            float speedBonus = (speedUpgrades * 2f) + ((level - 1) * 5f);
            currentSpeedText.text = "+" + speedBonus.ToString(); // Asl tezlikni bilmaganimiz uchun faqat bonus
        }
    }

    private void ShowPanel(GameObject panelToShow)
    {
        // Hamma panellarni yopish
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (inGamePanel != null) inGamePanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);

        // Faqat kerakligini ochish
        if (panelToShow != null) panelToShow.SetActive(true);
    }

    private void UpdateSliderColor(Slider slider)
    {
        if (slider != null && slider.fillRect != null)
        {
            Image fillImage = slider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                // Slayder qancha foiz to'lganini hisoblaymiz (0 dan 1 gacha)
                float percentage = slider.maxValue > 0 ? (slider.value / slider.maxValue) : 0f;
                // Rangni bo'sh va to'la ranglar orasida silliq o'zgartiramiz
                fillImage.color = Color.Lerp(emptyColor, fullColor, percentage);
            }
        }
    }
}
