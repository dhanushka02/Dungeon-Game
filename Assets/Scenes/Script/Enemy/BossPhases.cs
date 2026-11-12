
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
    public float moveSpeed = 2f;
    public float stoppingDistance = 2.3f;
    public float rotationSpeed = 360f;

    [Header("Attack Timing")]
    public float attackCooldown = 2.5f;
    public float comboCooldown = 4f;
    public float firstAttackDelay = 0.3f;

    [Header("Jump Attack")]
    public float jumpMinDistance = 3f;
    public float jumpMaxDistance = 14f;
    public float jumpHeight = 3f;
    public float jumpDuration = 1.5f;
    public float jumpLandingDistance = 0.5f;
    public float jumpWindup = 0.4f;
    public float jumpCooldown = 7f;

    [Header("Boss Phases")]
    [Range(0f, 1f)]
    public float phaseTwoAt = 0.7f;

    [Range(0f, 1f)]
    public float phaseThreeAt = 0.3f;

    public float phaseTransitionSeconds = 2f;
    public string phaseTwoAnimation = "Roar";
    public string phaseThreeAnimation = "Enrage";

    [Header("Attacks")]
    public List<Move> moves = new List<Move>();

    private GolemHealth hp;

    private int phase = 1;
    private int lastMoveIndex = -1;

    private bool busy;
    private bool died;
    private bool hasJumpedOnce;

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

        if (player == null || busy)
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

        // Opening attack: jump before walking.
        if (!hasJumpedOnce &&
            Time.time >= nextAttackTime)
        {
            Move openingJump = FindJumpMove(distance);

            if (openingJump != null)
            {
                StartCoroutine(Perform(openingJump));
                return;
            }
        }

        // Wait for the opening jump if it is available.
        if (!hasJumpedOnce &&
            FindJumpMove(distance) != null)
        {
            PlayState("Idle");
            return;
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

        // Jump again when the player is far enough away.
        if (Time.time >= nextJumpTime)
            selectedMove = FindJumpMove(distance);

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
        if (distance < jumpMinDistance ||
            distance > jumpMaxDistance)
            return null;

        // Prefer the dedicated Jump Attack.
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

        // Fallback to other unlocked leap attacks.
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
        if (player == null)
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

        string state =
            phase == 2
                ? phaseTwoAnimation
                : phaseThreeAnimation;

        PlayState(state);

        yield return new WaitForSeconds(
            phaseTransitionSeconds);

        nextAttackTime = Time.time + 2f;

        busy = false;
        PlayState("Idle");
    }

    IEnumerator Perform(Move move)
    {
        busy = true;

        bool shouldJump = IsJumpMove(move);

        Vector3 landingPosition = transform.position;

        if (shouldJump)
        {
            // Capture the player's position before the leap.
            Vector3 target = player.position;

            Vector3 direction =
                target - transform.position;

            direction.y = 0f;

            landingPosition = target;
            landingPosition.y = transform.position.y;

            if (direction.sqrMagnitude > 0.001f)
            {
                landingPosition -=
                    direction.normalized *
                    jumpLandingDistance;
            }
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
            hasJumpedOnce = true;

            PlayState("Roar");

            yield return new WaitForSeconds(
                jumpWindup);

            PlayState(move.animatorState);

            Debug.Log(
                "BOSS LEAP START: " + move.label);

            yield return StartCoroutine(
                BossJump(landingPosition));

            nextJumpTime = Time.time + jumpCooldown;
        }
        else
        {
            PlayState(move.animatorState);

            yield return new WaitForSeconds(
                move.windup);
        }

        DestroyWarning();

        if (hp != null && !hp.IsDead())
            DealDamage(move);

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

        busy = false;
        PlayState("Idle");
    }

    IEnumerator BossJump(Vector3 landingPosition)
    {
        Vector3 startPosition = transform.position;

        float duration = Mathf.Max(0.1f, jumpDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (hp == null || hp.IsDead())
                yield break;

            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(elapsed / duration);

            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    landingPosition,
                    t);

            position.y +=
                Mathf.Sin(t * Mathf.PI) *
                jumpHeight;

            transform.position = position;

            yield return null;
        }

        transform.position = landingPosition;

        Debug.Log("BOSS LEAP FINISHED");
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
        DestroyWarning();

        PlayState("Death");
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

    void PlayState(string state)
    {
        if (animator == null ||
            string.IsNullOrEmpty(state))
            return;

        int stateHash =
            Animator.StringToHash(
                "Base Layer." + state);

        if (!animator.HasState(0, stateHash))
        {
            Debug.LogWarning(
                "Boss missing animation: " + state);
            return;
        }

        AnimatorStateInfo current =
            animator.GetCurrentAnimatorStateInfo(0);

        if (current.fullPathHash == stateHash &&
            !animator.IsInTransition(0))
        {
            return;
        }

        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo next =
                animator.GetNextAnimatorStateInfo(0);

            if (next.fullPathHash == stateHash)
                return;
        }

        animator.CrossFadeInFixedTime(
            stateHash, 0.15f);
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
                1, 3f, 14f, 22, true, true),

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
