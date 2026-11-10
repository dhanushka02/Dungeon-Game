using UnityEngine;
using Test2;

namespace NPC
{
    public class Attack : Status
    {
        private static readonly int SpeedParameter =
            Animator.StringToHash("Speed");
        private static readonly int AttackTrigger =
            Animator.StringToHash("Attack");

        public EnemiesStatus stat;
        private bool hasAttacked;

        public bool IsAttacking => isAttacking;

        private void Start()
        {
            animator = GetComponentInChildren<Animator>();

            Target target = GetComponent<Target>();
            if (stat == null && target != null)
                stat = target.stat;

            if (player == null && target != null)
                player = target.player;

            if (animator == null)
            {
                Debug.LogError(
                    "Enemy Attack requires an Animator on this object or its children.",
                    this
                );
                enabled = false;
                return;
            }

            if (stat == null)
            {
                Debug.LogError("Enemy Attack requires an EnemiesStatus asset.", this);
                enabled = false;
                return;
            }

            if (player == null)
            {
                Debug.LogError("Enemy Attack requires a Player Transform.", this);
                enabled = false;
            }
        }

        public bool TryAttack()
        {
            if (!enabled || player == null || stat == null || animator == null)
                return false;

            if (GetPlanarDistance(player.position) > stat.attackRange)
                return false;

            animator.SetFloat(SpeedParameter, 0f);
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
                transform.forward = direction.normalized;

            if (isAttacking ||
                (hasAttacked && Time.time < lastAttackTime + stat.attackCooldown))
                return true;

            isAttacking = true;
            animator.SetTrigger(AttackTrigger);
            Invoke(nameof(DealDamage), stat.damageDelay);
            Invoke(nameof(FinishAttack), stat.attackDuration);
            lastAttackTime = Time.time;
            hasAttacked = true;

            return true;
        }

        private void DealDamage()
        {
            if (player == null || stat == null)
                return;

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null || playerHealth.IsDead())
                return;

            if (GetPlanarDistance(player.position) <= stat.attackRange)
                playerHealth.TakeDamage(stat.attackDamage);
        }

        private float GetPlanarDistance(Vector3 targetPosition)
        {
            Vector3 offset = targetPosition - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void FinishAttack()
        {
            isAttacking = false;
        }

        private void OnDisable()
        {
            CancelInvoke();
            isAttacking = false;
        }
    }
}
