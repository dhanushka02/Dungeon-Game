
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraTransform;
    public PlayerAnimation playerAnimation;

    public float normalSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -20f;

    private float verticalVelocity;
    private bool isJumping;

    void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        playerAnimation = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        if (controller == null || cameraTransform == null)
            return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * z + right * x).normalized;

        bool running = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = running ? sprintSpeed : normalSpeed;

        if (move != Vector3.zero)
            transform.forward = move;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            isJumping = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;

            if (playerAnimation != null)
                playerAnimation.PlayAction(PlayerAnimation.jumpAnimation, 0.6f);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = move * currentSpeed;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);

        if (playerAnimation == null || playerAnimation.IsLocked())
            return;

        if (isJumping && !controller.isGrounded)
        {
            playerAnimation.ChangeAnimation(PlayerAnimation.jumpAnimation);
        }
        else if (move == Vector3.zero)
        {
            playerAnimation.ChangeAnimation(PlayerAnimation.idleAnimation);
        }
        else if (running)
        {
            playerAnimation.ChangeAnimation(PlayerAnimation.sprintAnimation);
        }
        else
        {
            playerAnimation.ChangeAnimation(PlayerAnimation.walkAnimation);
        }
    }
}
