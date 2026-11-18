
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Image healthBar;
    public PlayerAnimation playerAnimation;
    public PlayerMovement playerMovement;

    public float healthBarSpeed = 0.5f;

    private int currentHealth;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.fillAmount = 1f;
    }
    void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (healthBar == null || maxHealth <= 0)
            return;

        float targetHealth = (float)currentHealth / maxHealth;

        healthBar.fillAmount = Mathf.MoveTowards(
            healthBar.fillAmount,
            targetHealth,
            healthBarSpeed * Time.deltaTime
        );
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        if (currentHealth > 0)
        {
            if (playerAnimation != null)
                playerAnimation.PlayHit();
        }
        else
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public bool CanHeal()
    {
        return !isDead && currentHealth < maxHealth;
    }

    void Die()
    {
        isDead = true;

        if (playerAnimation != null)
            playerAnimation.PlayDeath();

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    public bool IsDead()
    {
        return isDead;
    }
}
