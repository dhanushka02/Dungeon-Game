using UnityEngine;

public class GolemAI : MonoBehaviour
{
    public Transform player;
    public Transform pointA;
    public Transform pointB;

    public float moveSpeed = 2.5f;
    public float detectionRange = 1.5f;
    public float attackRange = 1f;
    public float attackCooldown = 2f;

    public float waitTime = 2f;

    private Animator animator;
    private Transform targetPoint;
    private float lastAttackTime;

    private bool isWaiting = false;
    private float waitTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        targetPoint = pointA;
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

        transform.LookAt(targetPoint);

        transform.position +=
            transform.forward * moveSpeed * Time.deltaTime;

        float distanceToPoint =
            Vector3.Distance(transform.position, targetPoint.position);

        if (distanceToPoint < 0.5f)
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
            ChangeTarget();

            isWaiting = false;
        }
    }

    void ChangeTarget()
    {
        if (targetPoint == pointA)
        {
            targetPoint = pointB;
        }
        else
        {
            targetPoint = pointA;
        }
    }

    void ChasePlayer()
    {
        isWaiting = false;

        animator.SetFloat("Speed", 1f);

        transform.LookAt(player);

        transform.position +=
            transform.forward * moveSpeed * Time.deltaTime;
    }

    void AttackPlayer()
    {
        isWaiting = false;

        animator.SetFloat("Speed", 0f);

        transform.LookAt(player);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("Attack");

            lastAttackTime = Time.time;
        }
    }
}
