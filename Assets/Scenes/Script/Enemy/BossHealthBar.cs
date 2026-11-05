
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public Image healthBarFill;
    public TMP_Text bossNameText;
    public string bossName = "Fallen Guardian";

    private void Awake()
    {
        if (bossNameText != null)
            bossNameText.text = bossName;

        gameObject.SetActive(false);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthBarFill != null && maxHealth > 0)
        {
            healthBarFill.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }
    }

    public void ShowBossHealth()
    {
        gameObject.SetActive(true);
    }

    public void HideBossHealth()
    {
        gameObject.SetActive(false);
    }
}
