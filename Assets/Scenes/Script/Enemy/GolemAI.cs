using UnityEngine;

public class GolemAI : MonoBehaviour
{
    public Transform player;

    public float moveSpeed = 2.5f;
    public float patrolDistance = 5f;

    public float detectionRange = 5f;
    public float attackRange = 1.5f;

    public int attackDamage = 20;
    public float attackCooldown = 2f;
    public float attackDuration = 1f;
    public float damageDelay = 0.5f;

    public float waitTime = 2f;

    private Animator animator;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 patrolDirection;

    private bool movingToEnd = true;
    private bool isWaiting = false;
    private bool isAttacking = false;

    private float waitTimer = 0f;
    private float lastAttackTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();

        startPosition = transform.position;
        patrolDirection = transform.forward;
        endPosition = startPosition + patrolDirection * patrolDistance;
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

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
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
        animator.SetFloat("Speed", 0f);

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;

            animator.SetTrigger("Attack");

            Invoke("DealDamage", damageDelay);
            Invoke("FinishAttack", attackDuration);

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

        if (distance <= attackRange)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    void FinishAttack()
    {
        isAttacking = false;
    }
}