using UnityEngine;

public class GolemAI : MonoBehaviour
{
    public Transform player;

    public float moveSpeed = 2.5f;
    public float patrolDistance = 5f;

    public float detectionRange = 1.5f;
    public float attackRange = 1f;
    public float attackCooldown = 2f;

    public float waitTime = 2f;

    private Animator animator;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 patrolDirection;

    private bool movingToEnd = true;

    private float lastAttackTime;

    private bool isWaiting = false;
    private float waitTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Save this Golem's starting position
        startPosition = transform.position;

        // Save the direction the Golem is facing
        patrolDirection = transform.forward;

        // Calculate patrol end position
        endPosition =
            startPosition + patrolDirection * patrolDistance;
    }

    void Update()
    {
        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectionRange)
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

        // Look at patrol target
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }

        // Move
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Reached patrol point
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

        if (waitTimer >= waitTime)
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
            transform.forward * moveSpeed * Time.deltaTime;
    }

    void AttackPlayer()
    {
        isWaiting = false;

        animator.SetFloat("Speed", 0f);

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }

        if (Time.time > lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("Attack");

            lastAttackTime = Time.time;
        }
    }
}