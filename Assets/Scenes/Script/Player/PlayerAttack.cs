using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator;
    public Transform attackPoint;
    public LayerMask enemyLayer;

    public float attackRange = 1f;
    public int attackDamage = 20;
    public float attackCooldown = 1f;
    public float damageDelay = 0.5f;

    private float lastAttackTime;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) &&
            Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;

        Invoke("DealDamage", damageDelay);
    }

    void DealDamage()
    {
        Collider[] enemies = Physics.OverlapSphere(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider enemy in enemies)
        {
            GolemHealth golemHealth =
                enemy.GetComponentInParent<GolemHealth>();

            if (golemHealth != null)
            {
                golemHealth.TakeDamage(attackDamage);
            }
        }
    }
}