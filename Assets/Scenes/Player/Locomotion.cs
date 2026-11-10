using UnityEngine;
using UnityEngine.InputSystem;

namespace Test2
{
public class Locomotion : MonoBehaviour
{
    Transform CameraObject;
    [SerializeField] private Transform cameraReference;
    InputHandler inputHandler;
    private Attack attackHandler;
    Vector3 MoveDirection;
    [HideInInspector]
    public Transform myTransform;
    [HideInInspector] public Animation animatorHandler;
    public new Rigidbody rigidbody;
    public GameObject normalCamera;

    [Header("Stats")]
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float rotationSpeed = 10;
    // [SerializeField] float JumpHeigh = 5;
    // private float verticalVeloc;
    [Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] LayerMask groundLayer;

    private bool isGrounded;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        inputHandler = GetComponent<InputHandler>();
        attackHandler = GetComponent<Attack>();
        animatorHandler = GetComponent<Animation>();
        // CameraObject = Camera.main.transform;
        myTransform = transform;
        animatorHandler.Initialize();
        CameraObject = cameraReference;

        if (CameraObject == null)
        {
            Debug.LogError("Locomotion: Camera Reference Not Setup!");
            enabled = false;
            return;
        }
    }
    #region  Movement
    Vector3 normalVector;
    Vector3 targetPosition;
    private void HandleRotation(float delta)
    {
        Vector3 targetDir = Vector3.zero;
        float moveOverride = inputHandler.moveAmount;

        targetDir = CameraObject.forward * inputHandler.vertical;
        targetDir += CameraObject.right * inputHandler.horizontal;

        targetDir.Normalize();
        if(targetDir == Vector3.zero)
        {
            targetDir = myTransform.forward;
        }
        float rs = rotationSpeed;
        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation,tr,rs*delta);

        myTransform.rotation = targetRotation;
    }
    private void CheckGround()
    {
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );
    }
    private void HandleJump()
    {
        if (!inputHandler.Jump)
            return;

        inputHandler.Jump = false;
        if (!isGrounded || !animatorHandler.CanStartAction)
            return;

        rigidbody.linearVelocity = new Vector3(
            rigidbody.linearVelocity.x,
            jumpForce,
            rigidbody.linearVelocity.z
        );

        animatorHandler.SetJumping(true);
    }
    public void Update()
    {
        float delta = Time.deltaTime;
        inputHandler.TickInput(delta);

        Vector3 cameraForward = CameraObject.forward;
        Vector3 cameraRight = CameraObject.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        MoveDirection = cameraForward * inputHandler.vertical;
        MoveDirection += cameraRight * inputHandler.horizontal;

        MoveDirection = Vector3.ClampMagnitude(MoveDirection, 1f);

        CheckGround();
        HandleJump();
        animatorHandler.SetJumping(
            !isGrounded || rigidbody.linearVelocity.y > 0.1f
        );
        attackHandler?.HandleInput();

        float speed = movementSpeed;
        MoveDirection *= speed;

        Vector3 projectVelocity =
            Vector3.ProjectOnPlane(MoveDirection, normalVector);

        rigidbody.linearVelocity = new Vector3(
            projectVelocity.x,
            rigidbody.linearVelocity.y,
            projectVelocity.z
        );

        animatorHandler.UpdateAnimValue(inputHandler.moveAmount, 0);

        if (animatorHandler.canRotate)
        {
            HandleRotation(delta);
        }
    }
        #endregion
        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log(
                "Player collided with: " +
                collision.gameObject.name +
                " | Layer: " +
                LayerMask.LayerToName(collision.gameObject.layer)
            );
        }
}
}
