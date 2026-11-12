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
        [Range(1, 3)] public int unlockPhase = 1;
        public float minDistance = 0f;
        public float maxDistance = 4f;
        public float windup = 0.6f;
        public float recovery = 1.1f;
        public float radius = 1.7f;
        public int damage = 15;
        public bool areaAttack;
        public bool warning = true;
    }

    public Transform player;
    public Animator animator;
    public float detectionRange = 14f;
    public float moveSpeed = 2.5f;
    public float phaseTwoAt = 0.7f;
    public float phaseThreeAt = 0.3f;
    public float phaseTransitionSeconds = 1.3f;
    public string phaseTwoAnimation = "Roar";
    public string phaseThreeAnimation = "Roar";
    public List<Move> moves = new List<Move>();

    GolemHealth hp;
    int phase = 1;
    bool busy;
    bool died;
    GameObject warningCircle;
    Material warningMaterial;

    void Reset()
    {
        moves = new List<Move> {
            NewMove("Left Punch", "PunchLeft", 1, 0, 2.2f, 15),
            NewMove("Right Hook", "PunchRight", 1, 0, 2.2f, 16),
            NewMove("Three Hit Combo", "Combo3", 1, 0, 2.4f, 22),
            NewMove("Ground Slam", "GroundSlam", 2, 0, 4f, 25, true),
            NewMove("Heavy Punch", "HeavyPunch", 2, 0, 2.5f, 26),
            NewMove("Charge Strike", "ChargeStrike", 2, 2.5f, 7f, 22),
            NewMove("Double Slam", "DoubleSlam", 2, 0, 4f, 28, true),
            NewMove("Rage Combo", "RageCombo", 3, 0, 2.8f, 35),
            NewMove("Triple Slam", "TripleSlam", 3, 0, 4.5f, 36, true),
            NewMove("Ultimate Shockwave", "Ultimate", 3, 0, 6f, 45, true)
        };
    }

    Move NewMove(string label, string state, int unlock, float min, float max, int damage, bool area = false)
    {
        return new Move { label=label, animatorState=state, unlockPhase=unlock,
            minDistance=min, maxDistance=max, damage=damage, areaAttack=area,
            radius=area ? max : 1.7f, windup=area ? 1.1f : 0.5f,
            recovery=area ? 1.5f : 0.9f };
    }

    void Awake()
    {
        hp = GetComponent<GolemHealth>();
        if (animator == null) animator = GetComponent<Animator>();
        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
        }
    }

    void Update()
    {
        if (hp == null || hp.IsDead())
        {
            if (!died) { died = true; StopAllCoroutines(); DestroyWarning(); }
            return;
        }
        int desired = hp.HealthPercent <= phaseThreeAt ? 3 : hp.HealthPercent <= phaseTwoAt ? 2 : 1;
        if (desired > phase && !busy) { StartCoroutine(Transition(desired)); return; }
        if (busy || player == null) return;
        PlayerHealth playerHP = player.GetComponent<PlayerHealth>();
        if (playerHP != null && playerHP.IsDead()) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        float distance = toPlayer.magnitude;
        if (distance > detectionRange) return;
        if (toPlayer.sqrMagnitude > 0.001f) transform.rotation = Quaternion.RotateTowards(
            transform.rotation, Quaternion.LookRotation(toPlayer), 540f * Time.deltaTime);

        var available = new List<Move>();
        foreach (Move move in moves)
            if (move.unlockPhase <= phase && distance >= move.minDistance && distance <= move.maxDistance && HasState(move.animatorState))
                available.Add(move);
        if (available.Count > 0) StartCoroutine(Perform(available[Random.Range(0, available.Count)]));
        else if (distance > 1.6f)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(player.position.x, transform.position.y, player.position.z), moveSpeed * Time.deltaTime);
            if (animator != null) animator.SetFloat("Speed", 1f);
        }
    }

    IEnumerator Transition(int nextPhase)
    {
        busy = true;
        phase = nextPhase;
        if (animator != null) animator.SetFloat("Speed", 0);
        PlayState(nextPhase == 2 ? phaseTwoAnimation : phaseThreeAnimation);
        yield return new WaitForSeconds(phaseTransitionSeconds);
        busy = false;
    }

    IEnumerator Perform(Move move)
    {
        busy = true;
        if (animator != null) animator.SetFloat("Speed", 0);
        PlayState(move.animatorState);
        if (move.warning) ShowWarning(move.radius, move.windup);
        yield return new WaitForSeconds(move.windup);
        DestroyWarning();
        if (hp != null && !hp.IsDead() && player != null)
        {
            var playerHP = player.GetComponent<PlayerHealth>();
            if (playerHP != null && !playerHP.IsDead())
            {
                Vector3 delta = player.position - transform.position;
                delta.y = 0;
                bool inRange = delta.magnitude <= move.radius;
                bool inFront = Vector3.Dot(transform.forward, delta.normalized) >= 0.25f;
                if (inRange && (move.areaAttack || inFront)) playerHP.TakeDamage(move.damage);
            }
        }
        yield return new WaitForSeconds(move.recovery);
        busy = false;
    }

    bool HasState(string state) => animator != null &&
        animator.HasState(0, Animator.StringToHash("Base Layer." + state));

    void PlayState(string state)
    {
        if (HasState(state)) animator.CrossFadeInFixedTime("Base Layer." + state, 0.12f);
        else Debug.LogWarning("Boss Animator state missing: " + state, this);
    }

    void ShowWarning(float radius, float duration)
    {
        DestroyWarning();
        warningCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        warningCircle.name = "Boss Attack Warning (temporary)";
        Destroy(warningCircle.GetComponent<Collider>());
        warningCircle.transform.position = transform.position + Vector3.up * 0.04f;
        warningCircle.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
        var renderer = warningCircle.GetComponent<Renderer>();
        // Uses a built-in color shader when supported; replace material for URP/HDRP.
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null)
        {
            warningMaterial = new Material(shader);
            warningMaterial.color = new Color(1f, 0.12f, 0.12f, 0.45f);
            renderer.material = warningMaterial;
        }
    }

    void DestroyWarning()
    {
        if (warningCircle != null) Destroy(warningCircle);
        if (warningMaterial != null) Destroy(warningMaterial);
    }

    void OnDisable() => DestroyWarning();
}
