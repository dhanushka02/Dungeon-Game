using UnityEngine;
using UnityEngine.InputSystem;

public class Moving : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private float rotateTime = 0.2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpHeight = 5f;

    private Transform bodyCam;
    private CharacterController characterController;
    private float turnVelocity;
    private float verticalVelocity;
    private Vector2 playerMoveAmount;

    public InputAction PlayerMoveAction;
    public InputAction PlayerJumpAction;
    public InputActionAsset InputActionAsset;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        bodyCam = Camera.main != null ? Camera.main.transform : transform;

        if (InputSystem.actions != null)
        {
            PlayerMoveAction = InputSystem.actions.FindAction("Move", throwIfNotFound: false);
            PlayerJumpAction = InputSystem.actions.FindAction("Jump", throwIfNotFound: false);
        }

        if (InputActionAsset != null)
        {
            if (PlayerMoveAction == null)
                PlayerMoveAction = InputActionAsset.FindAction("Move", throwIfNotFound: false);
            if (PlayerJumpAction == null)
                PlayerJumpAction = InputActionAsset.FindAction("Jump", throwIfNotFound: false);
        }
    }

    private void OnEnable()
    {
        if (InputActionAsset != null)
        {
            var playerMap = InputActionAsset.FindActionMap("Player");
            if (playerMap != null)
                playerMap.Enable();
        }
        else if (PlayerMoveAction != null)
        {
            PlayerMoveAction.Enable();
        }

        if (PlayerJumpAction != null)
            PlayerJumpAction.Enable();
    }

    private void OnDisable()
    {
        if (InputActionAsset != null)
        {
            var playerMap = InputActionAsset.FindActionMap("Player");
            if (playerMap != null)
                playerMap.Disable();
        }
        else if (PlayerMoveAction != null)
        {
            PlayerMoveAction.Disable();
        }

        if (PlayerJumpAction != null)
            PlayerJumpAction.Disable();
    }

    private void MoveandRotate()
    {
        if (characterController == null)
            return;

        Vector3 movement = new Vector3(playerMoveAmount.x, 0f, playerMoveAmount.y).normalized;
        Vector3 verticalMove = new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime;

        if (movement.magnitude >= 0.1f)
        {
            float cameraY = bodyCam != null ? bodyCam.eulerAngles.y : 0f;
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cameraY;
            float smoothTarget = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, rotateTime);

            transform.rotation = Quaternion.Euler(0f, smoothTarget, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            characterController.Move(moveDir.normalized * speed * Time.deltaTime + verticalMove);
        }
        else
        {
            characterController.Move(verticalMove);
        }
    }

    private void Jump()
    {
        if (characterController == null)
            return;

        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;

            if (PlayerJumpAction != null && PlayerJumpAction.WasPressedThisFrame())
            {
                verticalVelocity = jumpHeight;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (characterController.enabled)
        {
            Vector3 gravityMove = new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime;
            characterController.Move(gravityMove);
        }
    }

    private void Update()
    {
        if (PlayerMoveAction != null)
            playerMoveAmount = PlayerMoveAction.ReadValue<Vector2>();
        else
            playerMoveAmount = Vector2.zero;

        MoveandRotate();
        Jump();
    }
}

public class PlayerControllet : Moving
{
}
