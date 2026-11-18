using UnityEngine;
using NPC;
using System.Collections.Generic;

namespace NPC
{
    public class Target : Status
    {
        private static readonly List<Target> activeTargets = new List<Target>();
        public static IReadOnlyList<Target> ActiveTargets => activeTargets;

        private static readonly int SpeedParameter =
            Animator.StringToHash("Speed");

        public EnemiesStatus stat;
        [SerializeField] private Attack attackHandler;

        public bool DetectsPlayer =>
            isActiveAndEnabled && player != null && stat != null &&
            (player.GetComponent<Test2.PlayerHealth>() == null ||
             !player.GetComponent<Test2.PlayerHealth>().IsDead()) &&
            Vector3.Distance(
                new Vector3(transform.root.position.x, 0f, transform.root.position.z),
                new Vector3(player.position.x, 0f, player.position.z)) <= stat.detectionRange;

        private void OnEnable()
        {
            activeTargets.Add(this);
        }

        private void OnDisable()
        {
            activeTargets.Remove(this);
        }

        private void Start()
        {
            movementRoot = transform.root;
            animator = GetComponentInChildren<Animator>();
            if (attackHandler == null)
                attackHandler = GetComponent<Attack>();
            ResolvePlayerTarget();

            if (attackHandler != null)
            {
                attackHandler.player = player;
                attackHandler.stat = stat;
            }

            startPosition = movementRoot.position;
            patrolDirection = movementRoot.forward;
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

            Test2.PlayerHealth playerHealth = player.GetComponent<Test2.PlayerHealth>();
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
            Vector3 direction = targetPosition - movementRoot.position;
            FaceDirection(direction);

            movementRoot.position = Vector3.MoveTowards(
                movementRoot.position,
                targetPosition,
                stat.moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(movementRoot.position, targetPosition) < 0.1f)
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
            targetPosition.y = movementRoot.position.y;
            Vector3 direction = targetPosition - movementRoot.position;

            if (direction != Vector3.zero)
            {
                FaceDirection(direction);
                movementRoot.position = Vector3.MoveTowards(
                    movementRoot.position,
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

            Test2.PlayerHealth playerHealth = player.GetComponentInParent<Test2.PlayerHealth>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<Test2.PlayerHealth>();

            if (playerHealth != null)
                player = playerHealth.transform;
        }

        private float GetPlanarDistance(Vector3 targetPosition)
        {
            Vector3 offset = targetPosition - movementRoot.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void SetMovementAnimation(float speed)
        {
            animator.SetFloat(SpeedParameter, speed);
        }
    }
}