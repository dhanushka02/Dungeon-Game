using UnityEngine;

public class GolemAI : MonoBehaviour
{
    public Transform player;

    public float moveSpeed = 2.5f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;

    private Animator animator;
    private float lastAttackTime;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > detectionRange)
        {
            animator.SetFloat("Speed", 0f);
        }
        else if (distance > attackRange)
        {
            ChasePlayer();
        }
        else
        {
            AttackPlayer();
        }
    }

    void ChasePlayer()
    {
        animator.SetFloat("Speed", 1f);

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    void AttackPlayer()
    {
        animator.SetFloat("Speed", 0f);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("Attack");
            lastAttackTime = Time.time;
        }
    }
}