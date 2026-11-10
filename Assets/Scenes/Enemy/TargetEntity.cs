using UnityEngine;
using NPC;
using Test2;

namespace NPC
{
    public class Target : Status
    {
        private static readonly int SpeedParameter =
            Animator.StringToHash("Speed");

        public EnemiesStatus stat;
        [SerializeField] private Attack attackHandler;

        private void Start()
        {
            animator = GetComponentInChildren<Animator>();
            if (attackHandler == null)
                attackHandler = GetComponent<Attack>();
            ResolvePlayerTarget();

            if (attackHandler != null)
            {
                attackHandler.player = player;
                attackHandler.stat = stat;
            }

            startPosition = transform.position;
            patrolDirection = transform.forward;
            endPosition = startPosition + patrolDirection * stat.patrolDistance;
        }

        private void Update()
        {
            if (isAttacking)
            {
                SetMovementAnimation(0f);
                return;
            }

            if (attackHandler != null && attackHandler.IsAttacking)
            {
                SetMovementAnimation(0f);
                return;
            }

            if (player == null)
            {
                Patrol();
                return;
            }

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerHealth.IsDead())
            {
                SetMovementAnimation(0f);
                return;
            }

            float distanceToPlayer = GetPlanarDistance(player.position);

            if (distanceToPlayer <= stat.detectionRange)
            {
                if (attackHandler != null && attackHandler.TryAttack())
                {
                    SetMovementAnimation(0f);
                    return;
                }

                ChasePlayer();
            }
            else
                Patrol();
        }

        private void Patrol()
        {
            if (isWaiting)
            {
                WaitAtPoint();
                return;
            }

            SetMovementAnimation(1f);

            Vector3 targetPosition = movingToEnd ? endPosition : startPosition;
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
                transform.forward = direction.normalized;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                stat.moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isWaiting = true;
                waitTimer = 0f;
            }
        }

        private void WaitAtPoint()
        {
            SetMovementAnimation(0f);
            waitTimer += Time.deltaTime;

            if (waitTimer >= stat.waitTime)
            {
                movingToEnd = !movingToEnd;
                isWaiting = false;
                waitTimer = 0f;
            }
        }

        private void ChasePlayer()
        {
            isWaiting = false;
            SetMovementAnimation(1f);

            Vector3 targetPosition = player.position;
            targetPosition.y = transform.position.y;
            Vector3 direction = targetPosition - transform.position;

            if (direction != Vector3.zero)
            {
                Vector3 planarDirection = direction.normalized;
                transform.rotation = Quaternion.LookRotation(planarDirection);
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    stat.moveSpeed * Time.deltaTime
                );
            }
        }

        private void ResolvePlayerTarget()
        {
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                    player = playerObject.transform;
            }

            if (player == null)
                return;

            PlayerHealth playerHealth = player.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<PlayerHealth>();

            if (playerHealth != null)
                player = playerHealth.transform;
        }

        private float GetPlanarDistance(Vector3 targetPosition)
        {
            Vector3 offset = targetPosition - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void SetMovementAnimation(float speed)
        {
            animator.SetFloat(SpeedParameter, speed);
        }
    }
}