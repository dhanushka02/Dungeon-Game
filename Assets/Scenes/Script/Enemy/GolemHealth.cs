
using UnityEngine;
using UnityEngine.UI;

public class GolemHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Image healthFill;
    public BossHealthBar bossHealthBar;
    public float bossHealthShowRange = 10f;

    private int currentHealth;
    private Animator animator;
    private GolemAI golemAI;
    private Transform player;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        golemAI = GetComponent<GolemAI>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        UpdateHealthBar();

        if (bossHealthBar != null)
        {
            bossHealthBar.UpdateHealth(currentHealth, maxHealth);
            bossHealthBar.HideBossHealth();
        }
    }

    void Update()
    {
        if (bossHealthBar == null || player == null || isDead)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= bossHealthShowRange)
            bossHealthBar.ShowBossHealth();
        else
            bossHealthBar.HideBossHealth();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        UpdateHealthBar();

        if (bossHealthBar != null)
            bossHealthBar.UpdateHealth(currentHealth, maxHealth);

        Debug.Log("Golem Health: " + currentHealth);

        if (currentHealth <= 0)
            Die();
        else if (animator != null)
            animator.SetTrigger("Hit");
    }

    void UpdateHealthBar()
    {
        if (healthFill != null && maxHealth > 0)
            healthFill.fillAmount = (float)currentHealth / maxHealth;
    }

    void Die()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger("Death");
        }

        if (golemAI != null)
            golemAI.enabled = false;

        if (bossHealthBar != null)
            bossHealthBar.HideBossHealth();
    }

    public bool IsDead()
    {
        return isDead;
    }
}
