using UnityEngine;
using UnityEngine.UI;
using NPC;
namespace NPC
{
    public class Attack:Status
{
    public EnemiesStatus stat;
    
     void AttackPlayer()
        {
            animator.SetFloat("Speed", 0f);
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                transform.forward = direction.normalized;
            }

            if (Time.time >= lastAttackTime + stat.attackCooldown)
            {
                isAttacking = true;

                animator.SetTrigger("Attack");

                Invoke("DealDamage", stat.damageDelay);
                Invoke("FinishAttack", stat.attackDuration);

                lastAttackTime = Time.time;
            }
        }

        void DealDamage()
        {
            if (player == null)
            {
                return;
            }

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth == null || playerHealth.IsDead())
            {
                return;
            }

            float distance =
                Vector3.Distance(transform.position, player.position);

            if (distance <= stat.attackRange)
            {
                playerHealth.TakeDamage(stat.attackDamage);
            }
        }

        void Update()
    {
            float distanceToPlayer =
                Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= stat.attackRange)
            {
                AttackPlayer();
            }
    }
        void FinishAttack()
        {
            isAttacking = false;
        }
        
}
}