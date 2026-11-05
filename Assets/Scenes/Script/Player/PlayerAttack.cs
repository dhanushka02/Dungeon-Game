
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public PlayerAnimation playerAnimation;
    public PlayerHealth playerHealth;
    public Transform attackPoint;
    public LayerMask enemyLayer;

    public float attackRange = 1f;
    public int attackDamage = 20;
    public float attackCooldown = 0.6f;
    public float damageDelay = 0.3f;
    public float comboResetTime = 1.5f;

    private float lastAttackTime = -10f;
    private int combo = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("LEFT CLICK DETECTED");

            if (playerHealth != null && playerHealth.IsDead())
                return;

            Attack();
        }
    }

    void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        playerHealth = GetComponent<PlayerHealth>();

        if (attackPoint == null)
        {
            GameObject point = new GameObject("AttackPoint");
            point.transform.SetParent(transform);
            point.transform.localPosition = new Vector3(0f, 1f, 1f);
            attackPoint = point.transform;
        }
    }


    void Attack()
    {
        if (playerAnimation == null)
        {
            Debug.LogError("PlayerAnimation is NULL");
            return;
        }

        if (playerAnimation.IsLocked())
        {
            Debug.Log("Attack blocked: Animation locked");
            return;
        }

        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log("Attack blocked: Cooldown");
            return;
        }

        if (Time.time - lastAttackTime > comboResetTime)
            combo = 0;

        combo++;

        if (combo > 3)
            combo = 1;

        string animationName = PlayerAnimation.attack1Animation;

        if (combo == 2)
            animationName = PlayerAnimation.attack2Animation;

        if (combo == 3)
            animationName = PlayerAnimation.attack3Animation;

        Debug.Log("Playing attack: " + animationName);

        playerAnimation.PlayAction(animationName, attackCooldown);

        lastAttackTime = Time.time;

        StartCoroutine(DealDamageAfterDelay());
    }


    IEnumerator DealDamageAfterDelay()
    {
        yield return new WaitForSeconds(damageDelay);

        if (playerHealth != null && playerHealth.IsDead())
            yield break;

        if (attackPoint == null)
        {
            Debug.LogWarning("Attack Point has not been assigned.");
            yield break;
        }

        Collider[] enemies = Physics.OverlapSphere(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        HashSet<GolemHealth> damagedEnemies = new HashSet<GolemHealth>();

        foreach (Collider enemy in enemies)
        {
            GolemHealth golemHealth =
                enemy.GetComponentInParent<GolemHealth>();

            if (golemHealth != null && damagedEnemies.Add(golemHealth))
            {
                golemHealth.TakeDamage(attackDamage);
            }
        }
    }
}
