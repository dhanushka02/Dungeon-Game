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

    [Header("Dodge")]
    [SerializeField, Min(0f)] float dodgeTransitionTime = 0.1f;

    private static readonly int DodgeState = Animator.StringToHash("Horizontal.Dodge");
    private static readonly int MovementState = Animator.StringToHash("Horizontal.Blend Tree");
    private static readonly int HorizontalParameter = Animator.StringToHash("Horizontal");
    private static readonly int VerticalParameter = Animator.StringToHash("Vertical");
    private Combo combo;
    private bool dodgePending;
    private bool dodgeAfterCombo;
    private bool isDodging;
    private bool backStep;
    private Vector2 dodgeInput;
    private Vector3 dodgeDirection;
    private int dodgeStartFrame;
    private Animator rootMotionAnimator;
    private bool originalApplyRootMotion;

    private bool isGrounded;

    private void OnEnable()
    {
        rootMotionAnimator = GetComponent<Animator>();
        if (rootMotionAnimator != null)
        {
            originalApplyRootMotion = rootMotionAnimator.applyRootMotion;
            rootMotionAnimator.applyRootMotion = true;
        }
    }

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        inputHandler = GetComponent<InputHandler>();
        attackHandler = GetComponent<Attack>();
        combo = GetComponent<Combo>();
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

    private void OnDisable()
    {
        dodgePending = false;
        if (animatorHandler != null)
            animatorHandler.SetDodgePending(false);
        if (isDodging && animatorHandler != null)
            animatorHandler.SetDodging(false);
        isDodging = false;
        if (rootMotionAnimator != null)
            rootMotionAnimator.applyRootMotion = originalApplyRootMotion;
    }

    private void HandleDodgeInput(Vector3 cameraForward, Vector3 cameraRight)
    {
        if (!inputHandler.ConsumeDodge(out Vector2 movement, out bool duringCombo) ||
            isDodging || dodgePending)
            return;

        backStep = movement.sqrMagnitude < 0.01f ||
            (movement.y < -0.1f && Mathf.Abs(movement.x) < 0.1f);
        dodgeInput = Vector2.ClampMagnitude(movement, 1f);
        dodgeDirection = backStep
            ? -Vector3.ProjectOnPlane(myTransform.forward, Vector3.up).normalized
            : (cameraForward * movement.y + cameraRight * movement.x).normalized;

        dodgeAfterCombo = duringCombo;
        if (!dodgeAfterCombo && (!isGrounded || !animatorHandler.CanStartAction))
            return;

        dodgePending = true;
        animatorHandler.SetDodgePending(true);
    }

    private void StartDodge()
    {
        Animator animator = animatorHandler.anim;
        if (animator == null || !animator.HasState(0, DodgeState))
        {
            Debug.LogError("Locomotion: Dodge state is missing from the Animator.", this);
            return;
        }

        if (!backStep && dodgeDirection.sqrMagnitude > 0.0001f)
            myTransform.rotation = Quaternion.LookRotation(dodgeDirection);

        isDodging = true;
        dodgeStartFrame = Time.frameCount;
        animatorHandler.SetDodging(true);
        rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
        animator.SetFloat(HorizontalParameter, dodgeInput.x);
        animator.SetFloat(VerticalParameter, dodgeInput.y);
        animator.CrossFadeInFixedTime(DodgeState, dodgeTransitionTime, 0, 0f);
    }

    private void EndDodge()
    {
        isDodging = false;
        animatorHandler.SetDodging(false);
        animatorHandler.anim.CrossFadeInFixedTime(MovementState, dodgeTransitionTime, 0, 0f);
    }

    private bool UpdateDodge()
    {
        if (!isDodging)
            return false;

        Animator animator = animatorHandler.anim;
        if (Time.frameCount <= dodgeStartFrame)
            return true;

        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
        bool inTransition = animator.IsInTransition(0);
        bool enteringDodge = inTransition &&
            animator.GetNextAnimatorStateInfo(0).fullPathHash == DodgeState;
        bool inDodge = current.fullPathHash == DodgeState;

        if (!enteringDodge && !inDodge)
        {
            if (Time.frameCount <= dodgeStartFrame + 2)
                return true;

            Debug.LogError("Locomotion: Animator did not enter the dodge state.", this);
            EndDodge();
            return false;
        }

        if (inDodge && !inTransition && animator.GetCurrentAnimatorClipInfoCount(0) == 0)
        {
            Debug.LogError("Locomotion: Assign an animation clip to the dodge state.", this);
            EndDodge();
            return false;
        }

        if (inDodge && !inTransition && current.normalizedTime >= 1f)
        {
            EndDodge();
            return false;
        }

        return true;
    }

    private void OnAnimatorMove()
    {
        Animator animator = rootMotionAnimator;
        if (animator == null)
            return;

        if (!isDodging)
        {
            if (originalApplyRootMotion)
                animator.ApplyBuiltinRootMotion();
            return;
        }

        if (Time.deltaTime <= 0f)
            return;

        Vector3 velocity = animator.deltaPosition / Time.deltaTime;
        rigidbody.linearVelocity = new Vector3(velocity.x, rigidbody.linearVelocity.y, velocity.z);
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
        animatorHandler.SetJumping(
            !isGrounded || rigidbody.linearVelocity.y > 0.1f
        );
        HandleDodgeInput(cameraForward, cameraRight);
        if (dodgePending && (combo == null || !combo.IsAttacking) &&
            isGrounded && animatorHandler.CanStartDodge(dodgeAfterCombo))
        {
            dodgePending = false;
            animatorHandler.SetDodgePending(false);
            StartDodge();
        }

        if (isDodging || dodgePending)
        {
            inputHandler.ConsumeLightAttack();
            inputHandler.Jump = false;
            if (UpdateDodge() || dodgePending)
                return;
        }

        HandleJump();
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
