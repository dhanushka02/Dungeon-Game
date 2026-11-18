using UnityEngine;
using System.Collections.Generic;
using NPC;
namespace Test2
{
    public class Combo : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private Animation animationHandler;

        [Header("Animator")]
        [SerializeField] private string attackState = "Horizontal.Unarmed Attack";
        [SerializeField] private string weaponAttackState = "Horizontal.LongSword Attack";
        [SerializeField] private string comboStepParam = "ComboStep";
        [SerializeField] private string attackIndexParam = "AttackIndex";

        [Header("Combo Settings")]
        [SerializeField, Min(1)] private int maxSteps = 3;
        [Tooltip("Seconds after an attack ends in which the next click continues the combo")]
        [SerializeField, Min(0.05f)] private float comboWindow = 0.5f;
        [SerializeField] private float transitionTime = 0.1f;
        [SerializeField] private bool lockMovement = true;

        private int attackStateHash;
        private int unarmedAttackStateHash;
        private int weaponAttackStateHash;
        private static readonly int MovementStateHash = Animator.StringToHash("Horizontal.Blend Tree");
        private int comboStepHash;
        private int attackIndexHash;
        private int step;
        private bool nextAttackQueued;
        private bool waitingForNext;
        private float windowEndTime;
        private int startFrame;
        private bool rotationLocked;
        private bool previousCanRotate = true;
        private bool dodgeQueued;
        private bool weaponCombo;
        private readonly HashSet<EnemyHealth> hitTargets = new HashSet<EnemyHealth>();

        public bool IsAttacking => step > 0;
        public int Step => step;

        public bool TryRegisterHit(EnemyHealth enemy, bool weaponHitbox)
        {
            return step > 0 && !waitingForNext && weaponCombo == weaponHitbox &&
                hitTargets.Add(enemy);
        }

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (rigidbody == null)
                rigidbody = GetComponent<Rigidbody>();

            if (animationHandler == null)
                animationHandler = GetComponent<Animation>();

            unarmedAttackStateHash = Animator.StringToHash(attackState);
            weaponAttackStateHash = Animator.StringToHash(weaponAttackState);
            comboStepHash = Animator.StringToHash(comboStepParam);
            attackIndexHash = Animator.StringToHash(attackIndexParam);
        }

        private void OnDisable()
        {
            if (step > 0)
                EndCombo();
        }

        public void OnAttackPressed()
        {
            if (animator == null || dodgeQueued)
                return;

            if (step == 0)
            {
                weaponCombo = animationHandler != null && animationHandler.HasWeaponInHand();
                attackStateHash = weaponCombo ? weaponAttackStateHash : unarmedAttackStateHash;
                if (!animator.HasState(0, attackStateHash))
                {
                    Debug.LogError("Combo: Selected attack state is missing from the Animator.", this);
                    return;
                }
                StartStep(1);
            }
            else if (step < maxSteps)
            {
                if (waitingForNext)
                    StartStep(step + 1);
                else
                    nextAttackQueued = true;
            }
        }

        public void QueueDodge()
        {
            if (step == 0)
                return;

            dodgeQueued = true;
            nextAttackQueued = false;
        }

        private void Update()
        {
            if (step == 0)
                return;

            if (Time.frameCount <= startFrame + 1)
                return;

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            if (animator.IsInTransition(0))
            {
                if (animator.GetNextAnimatorStateInfo(0).fullPathHash != attackStateHash &&
                    state.fullPathHash != attackStateHash)
                    EndCombo();
                return;
            }

            if (state.fullPathHash != attackStateHash)
            {
                EndCombo();
                return;
            }

            if (dodgeQueued && (waitingForNext || state.normalizedTime >= 1f))
            {
                EndCombo(true);
                return;
            }

            if (waitingForNext)
            {
                if (Time.time >= windowEndTime)
                    EndCombo(true);
                return;
            }

            if (state.normalizedTime < 1f)
                return;

            if (nextAttackQueued && step < maxSteps)
            {
                StartStep(step + 1);
                return;
            }

            if (step >= maxSteps)
            {
                EndCombo(true);
                return;
            }

            waitingForNext = true;
            windowEndTime = Time.time + comboWindow;
        }

        private void LateUpdate()
        {
            if (!lockMovement || step == 0)
                return;

            if (rigidbody != null)
            {
                Vector3 velocity = rigidbody.linearVelocity;
                rigidbody.linearVelocity = new Vector3(0f, velocity.y, 0f);
            }

            if (animationHandler != null)
            {
                if (!rotationLocked)
                {
                    rotationLocked = true;
                    previousCanRotate = animationHandler.canRotate;
                }
                animationHandler.canRotate = false;
            }
        }

        private void StartStep(int newStep)
        {
            step = newStep;
            hitTargets.Clear();
            nextAttackQueued = false;
            waitingForNext = false;
            startFrame = Time.frameCount;

            animator.SetInteger(comboStepHash, step);
            animator.SetFloat(
                attackIndexHash,
                maxSteps > 1 ? (step - 1) / (float)(maxSteps - 1) : 0f
            );
            animator.CrossFadeInFixedTime(attackStateHash, transitionTime, 0, 0f);
        }

        private void EndCombo(bool returnToMovement = false)
        {
            step = 0;
            hitTargets.Clear();
            nextAttackQueued = false;
            waitingForNext = false;
            dodgeQueued = false;

            if (rotationLocked)
            {
                rotationLocked = false;
                if (animationHandler != null)
                    animationHandler.canRotate = previousCanRotate;
            }

            animator.SetInteger(comboStepHash, 0);
            if (returnToMovement)
                animator.CrossFadeInFixedTime(MovementStateHash, transitionTime, 0, 0f);
        }
    }
}
