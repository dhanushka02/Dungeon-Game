using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;

    public float normalSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -20f;

    private float verticalVelocity;

    private int combo = 0;
    private float lastAttackTime = 0f;

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * z + right * x;
        move = move.normalized;

        bool running = Input.GetKey(KeyCode.LeftShift);

        float currentSpeed = normalSpeed;

        if (running)
        {
            currentSpeed = sprintSpeed;
        }

        if (move != Vector3.zero)
        {
            transform.forward = move;
        }

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );

            animator.SetTrigger("Jump");
        }

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        if (Time.time - lastAttackTime > 1.5f)
        {
            combo = 0;
            animator.SetInteger("Combo", 0);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = move * currentSpeed;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);

        animator.SetFloat("Speed", move.magnitude);
        animator.SetBool("Running", running);
        animator.SetFloat("YVelocity", verticalVelocity);
    }

    void Attack()
    {
        if (Time.time - lastAttackTime > 1.5f)
        {
            combo = 0;
        }

        combo++;

        if (combo > 3)
        {
            combo = 1;
        }

        animator.SetInteger("Combo", combo);

        if (combo == 1)
        {
            animator.SetTrigger("Attack");
        }

        lastAttackTime = Time.time;
    }
}