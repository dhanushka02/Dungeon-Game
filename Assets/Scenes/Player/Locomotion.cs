using UnityEngine;
using UnityEngine.InputSystem;
using Test2;
public class Locomotion : MonoBehaviour
{
    Transform CameraObject;
    InputHandler inputHandler;
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
        animatorHandler = GetComponent<Animation>();
        CameraObject = Camera.main.transform;
        myTransform = transform;
        animatorHandler.Initialize();
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
        if (inputHandler.Jump && isGrounded)
        {
            rigidbody.linearVelocity = new Vector3(
                rigidbody.linearVelocity.x,
                jumpForce,
                rigidbody.linearVelocity.z
            );

            inputHandler.Jump = false;
        }
    }
    public void Update()
    {
        float delta = Time.deltaTime;
        inputHandler.TickInput(delta);
        MoveDirection = CameraObject.forward * inputHandler.vertical;
        MoveDirection += CameraObject.right * inputHandler.horizontal;
        MoveDirection.Normalize();
        MoveDirection.y = 0;
        CheckGround();
        HandleJump();
        animatorHandler.SetJumping(!isGrounded);

        float speed = movementSpeed;
        MoveDirection *= speed;

        Vector3 projectVelocity = Vector3.ProjectOnPlane(MoveDirection, normalVector);
        // rigidbody.linearVelocity = projectVelocity;
        rigidbody.linearVelocity = new Vector3(projectVelocity.x,rigidbody.linearVelocity.y,projectVelocity.z);
        animatorHandler.UpdateAnimValue(inputHandler.moveAmount, 0);
        if (animatorHandler.canRotate)
        {
            HandleRotation(delta);
        }
        
    }
    #endregion
}
