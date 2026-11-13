
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GolemHealth))]
public class BossPhases : MonoBehaviour
{
    [System.Serializable]
    public class Move
    {
        public string label;
        public string animatorState;

        [Range(1, 3)]
        public int unlockPhase = 1;

        public float minDistance = 0f;
        public float maxDistance = 4f;
        public float windup = 0.8f;
        public float recovery = 1.5f;
        public float radius = 2f;

        public int damage = 15;
        public bool areaAttack;
        public bool warning = true;
        public bool jumpAttack;
    }

    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Movement")]
    public float detectionRange = 14f;
    public float moveSpeed = 2.5f;
    public float stoppingDistance = 2.2f;
    public float rotationSpeed = 360f;

    [Header("Attack Timing")]
    public float attackCooldown = 2.5f;
    public float comboCooldown = 4f;
    public float firstAttackDelay = 0.5f;
    public float animationFadeTime = 0.1f;

    [Header("Jump Attack")]
    public float jumpMinDistance = 0f;
    public float jumpMaxDistance = 14f;
    public float jumpHeight = 4f;
    public float jumpDuration = 1.2f;
    public float jumpLandingDistance = 0.5f;
    public float jumpWindup = 0.5f;
    public float jumpCooldown = 7f;

    [Header("Boss Phases")]
    [Range(0f, 1f)]
    public float phaseTwoAt = 0.7f;

    [Range(0f, 1f)]
    public float phaseThreeAt = 0.3f;

    public float phaseTransitionSeconds = 1.3f;
    public string phaseTwoAnimation = "Roar";
    public string phaseThreeAnimation = "Roar";

    [Header("Attacks")]
    public List<Move> moves = new List<Move>();

    private GolemHealth hp;

    private int phase = 1;
    private int lastMoveIndex = -1;

    private bool busy;
    private bool died;
    private bool hasJumpedOnce;
    private bool attackAnimationLocked;

    // Leap movement is controlled only by LateUpdate.
    private bool leapActive;
    private Vector3 leapStartPosition;
    private Vector3 lockedTargetPosition;
    private float leapElapsed;

    private float nextAttackTime;
    private float nextJumpTime;

    private GameObject warningCircle;
    private Material warningMaterial;

    void Reset()
    {
        SetDefaultMoves();
    }

    void Awake()
    {
        hp = GetComponent<GolemHealth>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject obj =
                GameObject.FindGameObjectWithTag("Player");

            if (obj != null)
                player = obj.transform;
        }

        if (moves == null || moves.Count == 0)
            SetDefaultMoves();
    }

    void Start()
    {
        nextAttackTime = Time.time + firstAttackDelay;
        nextJumpTime = Time.time;

        if (animator != null)
            animator.applyRootMotion = false;

        PlayState("Idle");
    }

    void Update()
    {
        if (hp == null || hp.IsDead())
        {
            HandleDeath();
            return;
        }

        // Never interrupt an active attack or leap.
        if (player == null || busy || leapActive)
            return;

        int desiredPhase =
            hp.HealthPercent <= phaseThreeAt ? 3 :
            hp.HealthPercent <= phaseTwoAt ? 2 : 1;

        if (desiredPhase > phase)
        {
            StartCoroutine(Transition(desiredPhase));
            return;
        }

        PlayerHealth playerHP =
            player.GetComponent<PlayerHealth>();

        if (playerHP != null && playerHP.IsDead())
        {
            PlayState("Idle");
            return;
        }

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > detectionRange)
        {
            PlayState("Idle");
            return;
        }

        FacePlayer(direction);

        // First encounter: lock the player and leap.
        // Do not walk toward the player first.
        if (!hasJumpedOnce)
        {
            Move openingJump = FindJumpMove(distance);

            if (openingJump != null)
            {
                if (Time.time >= nextAttackTime)
                    StartCoroutine(Perform(openingJump));
                else
                    PlayState("Idle");

                return;
            }
        }

        // Walk during normal attack cooldown.
        if (Time.time < nextAttackTime)
        {
            if (distance > stoppingDistance)
                MoveTowardsPlayer();
            else
                PlayState("Idle");

            return;
        }

        Move selectedMove = null;

        // Leap again when the player is far away.
        if (distance > stoppingDistance &&
            Time.time >= nextJumpTime)
        {
            selectedMove = FindJumpMove(distance);
        }

        if (selectedMove == null)
            selectedMove = SelectNormalMove(distance);

        if (selectedMove != null)
        {
            StartCoroutine(Perform(selectedMove));
        }
        else if (distance > stoppingDistance)
        {
            MoveTowardsPlayer();
        }
        else
        {
            PlayState("Idle");
        }
    }

    void LateUpdate()
    {
        if (!leapActive || died)
            return;

        leapElapsed += Time.deltaTime;

        float duration = Mathf.Max(0.1f, jumpDuration);
        float t = Mathf.Clamp01(leapElapsed / duration);

        // Move horizontally toward the locked target.
        float moveT = Mathf.SmoothStep(0f, 1f, t);

        Vector3 position = Vector3.Lerp(
            leapStartPosition,
            lockedTargetPosition,
            moveT
        );

        // Rise into the air and then descend.
        position.y =
            Mathf.Lerp(
                leapStartPosition.y,
                lockedTargetPosition.y,
                moveT)
            + Mathf.Sin(t * Mathf.PI) * jumpHeight;

        transform.position = position;

        if (t >= 1f)
        {
            transform.position = lockedTargetPosition;
            leapActive = false;

            Debug.Log(
                "BOSS LANDED AT TARGET: " +
                transform.position);
        }
    }

    void FacePlayer(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
    }

    bool IsJumpMove(Move move)
    {
        return move != null &&
            (move.jumpAttack ||
             move.label == "Jump Attack");
    }

    Move FindJumpMove(float distance)
    {
        if (distance > detectionRange)
            return null;

        // Prioritize the dedicated Jump Attack.
        foreach (Move move in moves)
        {
            if (move.label != "Jump Attack")
                continue;

            if (move.unlockPhase > phase)
                continue;

            if (!HasState(move.animatorState))
                continue;

            return move;
        }

        // Fallback to other leap attacks.
        foreach (Move move in moves)
        {
            if (!IsJumpMove(move))
                continue;

            if (move.unlockPhase > phase)
                continue;

            if (!HasState(move.animatorState))
                continue;

            return move;
        }

        return null;
    }

    Move SelectNormalMove(float distance)
    {
        List<int> available = new List<int>();

        for (int i = 0; i < moves.Count; i++)
        {
            Move move = moves[i];

            if (move.unlockPhase > phase)
                continue;

            if (distance < move.minDistance ||
                distance > move.maxDistance)
                continue;

            if (IsJumpMove(move))
                continue;

            if (!HasState(move.animatorState))
                continue;

            if (i == lastMoveIndex)
                continue;

            available.Add(i);
        }

        if (available.Count == 0)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                Move move = moves[i];

                if (move.unlockPhase <= phase &&
                    distance >= move.minDistance &&
                    distance <= move.maxDistance &&
                    !IsJumpMove(move) &&
                    HasState(move.animatorState))
                {
                    available.Add(i);
                }
            }
        }

        if (available.Count == 0)
            return null;

        int index =
            available[Random.Range(0, available.Count)];

        lastMoveIndex = index;

        return moves[index];
    }

    void MoveTowardsPlayer()
    {
        if (player == null || busy || leapActive)
            return;

        Vector3 targetPosition = new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z);

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime);

        PlayState("Walk");
    }

    
    IEnumerator Transition(int nextPhase)
    {
        busy = true;
        phase = nextPhase;

        string state = phase == 2
            ? phaseTwoAnimation
            : phaseThreeAnimation;

        // Stop moving and play Roar.
        PlayState(state, true);

        // Wait until Roar finishes.
        if (HasState(state))
        {
            yield return StartCoroutine(
                WaitForAttackAnimation(state)
            );
        }
        else
        {
            yield return new WaitForSeconds(
                phaseTransitionSeconds
            );
        }

        // Small pause after Roar.
        yield return new WaitForSeconds(0.5f);

        nextAttackTime = Time.time + 1f;

        busy = false;
        PlayState("Idle", true);
    }


    IEnumerator Perform(Move move)
    {
        busy = true;

        bool shouldJump = IsJumpMove(move);

        Vector3 landingPosition = transform.position;

        if (shouldJump)
        {
            hasJumpedOnce = true;

            // Capture the player's position once.
            Vector3 targetPosition = player.position;

            Vector3 direction =
                targetPosition - transform.position;

            direction.y = 0f;

            // Land near the player's locked position.
            landingPosition = new Vector3(
                targetPosition.x,
                transform.position.y,
                targetPosition.z
            );

            if (direction.sqrMagnitude > 0.001f)
            {
                landingPosition -=
                    direction.normalized *
                    jumpLandingDistance;
            }

            FacePlayer(direction);

            Debug.Log(
                "BOSS TARGET LOCKED: " +
                landingPosition);
        }

        if (move.warning)
        {
            ShowWarning(
                move.radius,
                shouldJump
                    ? landingPosition
                    : transform.position);
        }

        if (shouldJump)
        {
            // Prepare the leap while standing still.
            PlayState("Roar", true);

            yield return new WaitForSeconds(
                jumpWindup);

            if (hp == null || hp.IsDead())
                yield break;

            // Play the leap animation.
            PlayState(move.animatorState, true);
            attackAnimationLocked = true;

            Debug.Log(
                "BOSS LEAP START: " + move.label);

            // Actual movement is handled in LateUpdate.
            yield return StartCoroutine(
                BossJump(landingPosition));

            nextJumpTime = Time.time + jumpCooldown;
        }
        else
        {
            PlayState(move.animatorState, true);
            attackAnimationLocked = true;

            yield return new WaitForSeconds(
                move.windup);
        }

        DestroyWarning();

        if (hp != null && !hp.IsDead())
            DealDamage(move);

        // Do not interrupt the attack animation.
        yield return StartCoroutine(
            WaitForAttackAnimation(move.animatorState));

        yield return new WaitForSeconds(
            move.recovery);

        float cooldown = attackCooldown;

        if (shouldJump ||
            move.label.Contains("Combo") ||
            move.label.Contains("Slam") ||
            move.label.Contains("Ultimate"))
        {
            cooldown = comboCooldown;
        }

        nextAttackTime = Time.time + cooldown;

        attackAnimationLocked = false;
        busy = false;

        PlayState("Idle", true);
    }

    IEnumerator BossJump(Vector3 landingPosition)
    {
        leapStartPosition = transform.position;
        lockedTargetPosition = landingPosition;

        leapElapsed = 0f;
        leapActive = true;

        Debug.Log(
            "BOSS JUMP FROM: " + leapStartPosition +
            " TO: " + lockedTargetPosition);

        while (leapActive)
        {
            if (hp == null || hp.IsDead())
            {
                leapActive = false;
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator WaitForAttackAnimation(string state)
    {
        if (!HasState(state))
            yield break;

        int hash = Animator.StringToHash(
            "Base Layer." + state);

        float timeout = 0f;

        // Wait until the attack state becomes active.
        while (timeout < 1f)
        {
            AnimatorStateInfo info =
                animator.GetCurrentAnimatorStateInfo(0);

            if (info.fullPathHash == hash)
                break;

            timeout += Time.deltaTime;
            yield return null;
        }

        // Wait until the animation finishes.
        timeout = 0f;

        while (timeout < 10f)
        {
            AnimatorStateInfo info =
                animator.GetCurrentAnimatorStateInfo(0);

            if (info.fullPathHash != hash)
                break;

            if (!info.loop &&
                info.normalizedTime >= 1f)
                break;

            timeout += Time.deltaTime;
            yield return null;
        }
    }

    void DealDamage(Move move)
    {
        if (player == null)
            return;

        PlayerHealth playerHP =
            player.GetComponent<PlayerHealth>();

        if (playerHP == null || playerHP.IsDead())
            return;

        Vector3 delta =
            player.position - transform.position;

        delta.y = 0f;

        bool inRange =
            delta.magnitude <= move.radius;

        bool inFront =
            delta.sqrMagnitude < 0.001f ||
            Vector3.Dot(
                transform.forward,
                delta.normalized) >= 0.25f;

        if (inRange &&
            (move.areaAttack || inFront))
        {
            playerHP.TakeDamage(move.damage);
        }
    }

    void HandleDeath()
    {
        if (died)
            return;

        died = true;
        busy = true;

        StopAllCoroutines();

        leapActive = false;
        attackAnimationLocked = false;

        DestroyWarning();
        PlayState("Death", true);
    }

    bool HasState(string state)
    {
        if (animator == null ||
            string.IsNullOrEmpty(state))
            return false;

        return animator.HasState(
            0,
            Animator.StringToHash(
                "Base Layer." + state));
    }

    void PlayState(string state, bool force = false)
    {
        if (animator == null ||
            string.IsNullOrEmpty(state))
            return;

        // Prevent interruption of attack animations.
        if (attackAnimationLocked && !force)
            return;

        int hash = Animator.StringToHash(
            "Base Layer." + state);

        if (!animator.HasState(0, hash))
        {
            Debug.LogWarning(
                "Boss missing animation: " + state);
            return;
        }

        AnimatorStateInfo current =
            animator.GetCurrentAnimatorStateInfo(0);

        if (current.fullPathHash == hash &&
            !animator.IsInTransition(0))
            return;

        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo next =
                animator.GetNextAnimatorStateInfo(0);

            if (next.fullPathHash == hash)
                return;
        }

        animator.CrossFadeInFixedTime(
            hash, animationFadeTime);
    }

    void ShowWarning(float radius, Vector3 position)
    {
        DestroyWarning();

        warningCircle =
            GameObject.CreatePrimitive(
                PrimitiveType.Cylinder);

        warningCircle.name = "Boss Attack Warning";

        Collider col =
            warningCircle.GetComponent<Collider>();

        if (col != null)
            Destroy(col);

        warningCircle.transform.position =
            position + Vector3.up * 0.04f;

        warningCircle.transform.localScale =
            new Vector3(
                radius * 2f,
                0.01f,
                radius * 2f);

        Renderer renderer =
            warningCircle.GetComponent<Renderer>();

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            warningMaterial = new Material(shader);

            warningMaterial.color =
                new Color(1f, 0.12f, 0.12f, 0.5f);

            renderer.material = warningMaterial;
        }
    }

    void DestroyWarning()
    {
        if (warningCircle != null)
            Destroy(warningCircle);

        if (warningMaterial != null)
            Destroy(warningMaterial);
    }

    void OnDisable()
    {
        StopAllCoroutines();

        leapActive = false;
        attackAnimationLocked = false;

        DestroyWarning();
    }

    void SetDefaultMoves()
    {
        moves = new List<Move>
        {
            NewMove("Left Punch", "PunchLeft",
                1, 0f, 2.5f, 15),

            NewMove("Right Swipe", "PunchRight",
                1, 0f, 2.8f, 17),

            NewMove("Jump Attack", "Combo3",
                1, 0f, 14f, 22, true, true),

            NewMove("Ground Slam", "Combo3",
                2, 0f, 4f, 25, true),

            NewMove("Heavy Punch", "PunchLeft",
                2, 0f, 2.5f, 26),

            NewMove("Charge Strike", "Combo3",
                2, 4f, 12f, 24, true, true),

            NewMove("Double Slam", "Combo3",
                2, 0f, 4f, 28, true),

            NewMove("Rage Combo", "PunchRight",
                3, 0f, 3f, 32),

            NewMove("Triple Slam", "Combo3",
                3, 0f, 4.5f, 36, true),

            NewMove("Ultimate Shockwave", "Combo3",
                3, 0f, 6f, 45, true)
        };
    }

    Move NewMove(
        string label,
        string state,
        int unlock,
        float min,
        float max,
        int damage,
        bool area = false,
        bool jump = false)
    {
        return new Move
        {
            label = label,
            animatorState = state,
            unlockPhase = unlock,
            minDistance = min,
            maxDistance = max,
            damage = damage,
            areaAttack = area,
            jumpAttack = jump,
            radius = area ? 3f : 2f,
            windup = area ? 1.2f : 0.8f,
            recovery = area ? 2f : 1.5f,
            warning = true
        };
    }
}
