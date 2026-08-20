using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

    void Awake()
    {
        // Agar sahnada allaqachon musiqa bo'lsa (masalan o'yindan yutqazib qayta boshlaganda), 
        // musiqani boshidan boshlamasligi uchun yangisini o'chirib tashlaymiz.
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Agar yo'q bo'lsa, buni asosiy musiqa qilib belgilaymiz
        instance = this;
        
        // Sahnani qayta yuklaganda ham obyektni va musiqani o'chirib yubormasligini ta'minlaymiz
        DontDestroyOnLoad(gameObject);
    }
}
