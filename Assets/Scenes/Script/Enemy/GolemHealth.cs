using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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

    void Update()
    {
        // Test only
        if (Keyboard.current != null &&
            Keyboard.current.hKey.wasPressedThisFrame)
        {
            TakeDamage(20);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth = currentHealth - damage;

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
        healthFill.fillAmount =
            (float)currentHealth / maxHealth;
    }

    void Die()
    {
        isDead = true;

        animator.SetFloat("Speed", 0f);
        animator.SetTrigger("Death");

        golemAI.enabled = false;

        Debug.Log("Golem died");
    }
}