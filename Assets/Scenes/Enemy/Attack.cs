using UnityEngine;

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
        private bool hitPlayer;

        public bool IsAttacking => isAttacking;

        private void Start()
        {
            movementRoot = transform.root;
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
            Vector3 direction = player.position - movementRoot.position;
            if (direction != Vector3.zero)
                FaceDirection(direction);

            if (isAttacking ||
                (hasAttacked && Time.time < lastAttackTime + stat.attackCooldown))
                return true;

            isAttacking = true;
            hitPlayer = false;
            animator.SetTrigger(AttackTrigger);
            Invoke(nameof(FinishAttack), stat.attackDuration);
            lastAttackTime = Time.time;
            hasAttacked = true;

            return true;
        }

        public void TryHitPlayer(Collider other)
        {
            if (!enabled || !isAttacking || hitPlayer || player == null || stat == null)
                return;

            EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.IsDead())
                return;

            Test2.PlayerHealth playerHealth = other.GetComponentInParent<Test2.PlayerHealth>();
            if (playerHealth == null || playerHealth.transform != player ||
                playerHealth.IsDead() || other != playerHealth.GetComponent<CapsuleCollider>() ||
                stat.attackDamage <= 0)
                return;

            hitPlayer = true;
            playerHealth.TakeDamage(stat.attackDamage);
        }

        private float GetPlanarDistance(Vector3 targetPosition)
        {
            Vector3 offset = targetPosition - movementRoot.position;
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
