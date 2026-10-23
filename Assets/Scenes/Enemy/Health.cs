using UnityEngine;
using UnityEngine.UI;
using NPC;
namespace NPC
{
    public class GolemHealth : Status
{        
    public EnemiesStatus stat;
    protected int currentHealth;
    public int CurrentHealth
    {
        get { return currentHealth; }
    }
    void Start()
    {
        currentHealth = stat.maxHealth;
        animator = GetComponent<Animator>();
        golemAI = GetComponent<Target>();

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
                (float)currentHealth / stat.maxHealth;
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
}