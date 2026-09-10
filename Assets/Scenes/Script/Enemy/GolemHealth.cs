using UnityEngine;
using UnityEngine.UI;

public class GolemHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Image healthFill;

    private int currentHealth;
    private Animator animator;
    private GolemAI golemAI;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        golemAI = GetComponent<GolemAI>();

        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHealthBar();

        Debug.Log("Golem Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }

    void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount =
                (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        isDead = true;

        animator.SetFloat("Speed", 0f);
        animator.SetTrigger("Death");

        if (golemAI != null)
        {
            golemAI.enabled = false;
        }
    }

    public bool IsDead()
    {
        return isDead;
    }
}