using UnityEngine;
using NPC;

namespace NPC
{
        public class Target : Status
    {
        public EnemiesStatus stat;
        void Start()
        {
            animator = GetComponent<Animator>();

            startPosition = transform.position;
            patrolDirection = transform.forward;
            endPosition = startPosition + patrolDirection * stat.patrolDistance;
        }

        void Update()
        {
            if (isAttacking)
            {
                animator.SetFloat("Speed", 0f);
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
                animator.SetFloat("Speed", 0f);
                return;
            }

            float distanceToPlayer =
                Vector3.Distance(transform.position, player.position);

            // if (distanceToPlayer <= attackRange)
            // {
            //     AttackPlayer();
            // } else
            if (distanceToPlayer <= stat.detectionRange)
            {
                ChasePlayer();
            }
            else
            {
                Patrol();
            }
        }

        void Patrol()
        {
            if (isWaiting)
            {
                WaitAtPoint();
                return;
            }

            animator.SetFloat("Speed", 1f);

            Vector3 targetPosition;

            if (movingToEnd)
            {
                targetPosition = endPosition;
            }
            else
            {
                targetPosition = startPosition;
            }

            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                transform.forward = direction.normalized;
            }

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

        void WaitAtPoint()
        {
            animator.SetFloat("Speed", 0f);

            waitTimer += Time.deltaTime;

            if (waitTimer >= stat.waitTime)
            {
                movingToEnd = !movingToEnd;
                isWaiting = false;
                waitTimer = 0f;
            }
        }

        void ChasePlayer()
        {
            isWaiting = false;

            animator.SetFloat("Speed", 1f);

            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                transform.forward = direction.normalized;
            }

            transform.position +=
                transform.forward * stat.moveSpeed * Time.deltaTime;
        }

       
    }

}