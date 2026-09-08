using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Slider healthBar;
    public Animator animator;
    public PlayerMovement playerMovement;

    private int currentHealth;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
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

        healthBar.value = currentHealth;

        if (currentHealth > 0)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        animator.SetFloat("Speed", 0f);
        animator.SetBool("Running", false);
        animator.SetTrigger("Death");

        playerMovement.enabled = false;
    }

    public bool IsDead()
    {
        return isDead;
    }
}