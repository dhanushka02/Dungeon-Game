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

    private float x;
    private float z;

    private bool isMoving;
    private bool isRunning;
    private bool isJumping;
    private bool isAttacking;
    private bool isHit;
    private bool isDead;

    private int attackCombo = 0;

    private string currentAnimation = "";

    void Update()
    {
        if (isDead)
        {
            return;
        }

        MovePlayer();
        CheckAttack();
        CheckAnimation();
    }

    void MovePlayer()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * z + right * x;
        move = move.normalized;

        isMoving = move != Vector3.zero;

        isRunning =
            Input.GetKey(KeyCode.LeftShift) &&
            isMoving;

        float currentSpeed = normalSpeed;

        if (isRunning)
        {
            currentSpeed = sprintSpeed;
        }

        if (isAttacking || isHit)
        {
            currentSpeed = 0f;
        }

        if (move != Vector3.zero && !isAttacking && !isHit)
        {
            transform.forward = move;
        }

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            isJumping = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) &&
            controller.isGrounded &&
            !isAttacking &&
            !isHit)
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);

            isJumping = true;

            ChangeAnimation("Jump");

            Invoke("JumpUp", 0.2f);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = move * currentSpeed;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }

    void CheckAttack()
    {
        if (Input.GetMouseButtonDown(0) &&
            !isAttacking &&
            !isHit &&
            !isJumping)
        {
            attackCombo++;

            if (attackCombo > 3)
            {
                attackCombo = 1;
            }

            isAttacking = true;

            if (attackCombo == 1)
            {
                ChangeAnimation("attack1");
            }
            else if (attackCombo == 2)
            {
                ChangeAnimation("attack2");
            }
            else if (attackCombo == 3)
            {
                ChangeAnimation("attack3");
            }

            Invoke("FinishAttack", 0.8f);
        }
    }

    void CheckAnimation()
    {
        if (isDead)
        {
            return;
        }

        if (isHit)
        {
            return;
        }

        if (isAttacking)
        {
            return;
        }

        if (isJumping)
        {
            if (verticalVelocity < 0f)
            {
                ChangeAnimation("Jump_Down");
            }

            return;
        }

        if (isRunning)
        {
            ChangeAnimation("Sprint");
            return;
        }

        if (z > 0.1f)
        {
            ChangeAnimation("RunForward");
            return;
        }

        if (x < -0.1f)
        {
            ChangeAnimation("RunLeft");
            return;
        }

        if (x > 0.1f)
        {
            ChangeAnimation("RunRight");
            return;
        }

        if (z < -0.1f)
        {
            ChangeAnimation("RunForward");
            return;
        }

        ChangeAnimation("Idle");
    }

    void ChangeAnimation(string animationName)
    {
        if (currentAnimation == animationName)
        {
            return;
        }

        animator.CrossFade(animationName, 0.15f);

        currentAnimation = animationName;
    }

    void JumpUp()
    {
        if (isJumping && !isDead)
        {
            ChangeAnimation("Jump_Up");
        }
    }

    void FinishAttack()
    {
        isAttacking = false;
    }

    public void PlayHit()
    {
        if (isDead)
        {
            return;
        }

        isHit = true;

        int randomHit = Random.Range(1, 3);

        if (randomHit == 1)
        {
            ChangeAnimation("behit1");
        }
        else
        {
            ChangeAnimation("behit2");
        }

        Invoke("FinishHit", 0.6f);
    }

    void FinishHit()
    {
        isHit = false;
    }

    public void PlayDeath()
    {
        isDead = true;
        isAttacking = false;
        isHit = false;
        isMoving = false;
        isRunning = false;

        ChangeAnimation("Death");
    }
}