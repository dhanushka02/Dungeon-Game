using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;

    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -20f;

    private float verticalVelocity;

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
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = move * speed;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);

        animator.SetFloat("Speed", move.magnitude);
        animator.SetFloat("YVelocity", verticalVelocity);
    }
}