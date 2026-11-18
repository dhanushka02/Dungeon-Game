
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private string currentAnimation;
    private float lockedUntil;
    private bool isDead;

    

    public static string idleAnimation => "Idle";
    public static string walkAnimation => "RunForward";
    public static string sprintAnimation => "Sprint";
    public static string jumpAnimation => "Jump";
    public static string hitAnimation => "Hit";
    public static string deathAnimation => "Death";

    public static string attack1Animation => "AS_1Hand_Up_Attack_B_1_InPlace";
    public static string attack2Animation => "AS_1Hand_Up_Attack_B_2_InPlace";
    public static string attack3Animation => "AS_1Hand_Up_Attack_B_3_InPlace";



    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public bool IsLocked()
    {
        return isDead || Time.time < lockedUntil;
    }

    public void ChangeAnimation(string animationName)
    {
        if (IsLocked())
            return;

        PlayAnimation(animationName);
    }

    void PlayAnimation(string animationName)
    {
        if (animator == null || string.IsNullOrEmpty(animationName))
            return;

        if (currentAnimation == animationName)
            return;

        string fullName = "Base Layer." + animationName;
        int stateHash = Animator.StringToHash(fullName);

        if (!animator.HasState(0, stateHash))
        {
            Debug.LogError("STATE NOT FOUND: " + fullName);
            return;
        }

        animator.CrossFadeInFixedTime(stateHash, 0.1f, 0, 0f);
        currentAnimation = animationName;
    }

    public void PlayAction(string animationName, float duration)
    {
        if (IsLocked())
            return;

        currentAnimation = "";
        PlayAnimation(animationName);
        lockedUntil = Time.time + duration;
    }

    public void PlayHit()
    {
        if (isDead)
            return;

        lockedUntil = 0f;
        PlayAction(hitAnimation, 0.5f);
    }

    public void PlayDeath()
    {
        if (isDead)
            return;

        lockedUntil = 0f;
        currentAnimation = "";
        PlayAnimation(deathAnimation);
        isDead = true;
    }
}
