
using UnityEngine;
using UnityEngine.UI;

public class GolemHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    [Header("Health UI")]
    public Image healthFill;
    public BossHealthBar bossHealthBar;
    public float bossHealthShowRange = 10f;

    [Header("Smooth Health Bar")]
    public float healthBarSpeed = 0.5f;

    private int currentHealth;
    private float displayedHealth;

    public int CurrentHealth => currentHealth;

    public float HealthPercent =>
        maxHealth > 0
            ? (float)currentHealth / maxHealth
            : 0f;

    private Animator animator;
    private GolemAI golemAI;
    private Transform player;

    private bool isDead = false;

    void Start()
    {
        currentHealth = Mathf.Max(0, maxHealth);
        displayedHealth = currentHealth;

        animator = GetComponent<Animator>();
        golemAI = GetComponent<GolemAI>();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        // Set the initial health value.
        UpdateHealthBar();

        if (bossHealthBar != null)
        {
            bossHealthBar.HideBossHealth();
        }
    }

    void Update()
    {
        // Smoothly animate the displayed health.
        float targetHealth = currentHealth;

        float speed = Mathf.Max(0f, healthBarSpeed)
            * Mathf.Max(1, maxHealth);

        displayedHealth = Mathf.MoveTowards(
            displayedHealth,
            targetHealth,
            speed * Time.deltaTime
        );

        UpdateHealthBar();

        // Show Boss Health Bar when Player is nearby.
        if (bossHealthBar != null && player != null)
        {
            if (isDead)
            {
                bossHealthBar.HideBossHealth();
            }
            else
            {
                float distance = Vector3.Distance(
                    transform.position,
                    player.position
                );

                if (distance <= bossHealthShowRange)
                {
                    bossHealthBar.ShowBossHealth();
                }
                else
                {
                    bossHealthBar.HideBossHealth();
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Do not update displayedHealth here.
        // Update() will animate the health bar smoothly.

        Debug.Log("Golem Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (animator != null)
        {
            // BossPhases controls the attack animations.
            // Only trigger Hit if the Animator has this parameter.
            if (HasTrigger("Hit"))
            {
                animator.SetTrigger("Hit");
            }
        }
    }

    void UpdateHealthBar()
    {
        if (maxHealth <= 0)
            return;

        float fill = Mathf.Clamp01(
            displayedHealth / maxHealth
        );

        // Update the direct health fill image.
        if (healthFill != null)
        {
            healthFill.fillAmount = fill;
        }

        // Update the Boss Health Bar UI.
        if (bossHealthBar != null)
        {
            int visibleHealth = Mathf.Clamp(
                Mathf.RoundToInt(displayedHealth),
                0,
                maxHealth
            );

            bossHealthBar.UpdateHealth(
                visibleHealth,
                maxHealth
            );
        }
    }

    bool HasTrigger(string parameterName)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter
            in animator.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type ==
                AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (golemAI != null)
        {
            golemAI.enabled = false;
        }

        // BossPhases handles the Death animation.
        BossPhases bossPhases =
            GetComponent<BossPhases>();

        if (bossPhases == null && animator != null)
        {
            if (HasTrigger("Death"))
            {
                animator.SetTrigger("Death");
            }
        }

        if (bossHealthBar != null)
        {
            bossHealthBar.HideBossHealth();
        }
    }

    public bool IsDead()
    {
        return isDead;
    }
}
