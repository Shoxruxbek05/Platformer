using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panellar va O'yin")]
    public GameObject startMenuPanel; // Boshlang'ich menyu paneli
    public GameObject inGamePanel;    // O'yin ichidagi HUD paneli (Tangalar, jon)
    public GameObject gameZone;       // O'yin jarayoni joylashgan asosiy obyekt (Game zone)

    void Start()
    {
        // O'yin endi yonganda vaqtni to'xtatib turamiz (shunda dushmanlar yurib ketmaydi)
        Time.timeScale = 0f; 
        
        // Menyu panelini ko'rsatish va o'yin ekranini yashirib turish
        if (startMenuPanel != null) startMenuPanel.SetActive(true);
        if (inGamePanel != null) inGamePanel.SetActive(false);
    }

    // Boshlash tugmasi bosilganda ishlaydigan funksiya
    public void StartGame()
    {
        // Menyuni yashirish
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        
        // O'yin HUD oynasini faollashtirish
        if (inGamePanel != null) inGamePanel.SetActive(true);
        
        // O'yin obyektlarini faollashtirish (agar o'chiq bo'lsa)
        if (gameZone != null) gameZone.SetActive(true);

        // Vaqtni o'z holiga qaytarish (o'yin boshlanadi)
        Time.timeScale = 1f;
    }

    // Chiqish tugmasi uchun
    public void ExitGame()
    {
        Debug.Log("O'yindan chiqildi!");
        Application.Quit(); // O'yin yopiladi (Faqat build qilingandan keyin ishlaydi, Unity muharririda shunchaki Log chiqadi)
    }
}
