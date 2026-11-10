using System.Collections;
using UnityEngine;

namespace Test2
{
public class Animation : MonoBehaviour
{
    private static readonly int PickupTrigger =
        Animator.StringToHash("Pickup");
    private static readonly int LightAttackTrigger =
        Animator.StringToHash("LightAttack");
    private static readonly int JumpState =
        Animator.StringToHash("Jump");
    private static readonly int PickupState =
        Animator.StringToHash("Pick Item");
    private static readonly int LightAttackState =
        Animator.StringToHash("Light Attack");

    [Header("Equipment State")]
    [SerializeField] private Invent.ItemData equippedWeapon;

    public Animator anim;
    int vertical;
    int horizontal;
    public bool canRotate;
    int isJumping;
    private bool jumping;
    private Invent.Inventory inventory;

    public bool HasEquippedWeapon => equippedWeapon != null;
    public Invent.ItemData EquippedWeapon => equippedWeapon;
    public bool CanStartAction
    {
        get
        {
            EnsureAnimator();
            return !jumping &&
                !IsStateActive(JumpState) &&
                !IsStateActive(PickupState) &&
                !IsStateActive(LightAttackState);
        }
    }

    private void OnEnable()
    {
        inventory = GetComponent<Invent.Inventory>();
        if (inventory == null)
            return;

        inventory.OnItemAdded += HandleItemAdded;
        inventory.OnInventoryChanged += ValidateEquippedWeapon;
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnItemAdded -= HandleItemAdded;
            inventory.OnInventoryChanged -= ValidateEquippedWeapon;
        }

        inventory = null;
    }

    private void HandleItemAdded(Invent.ItemData item)
    {
        if (item != null && item.itemType == Invent.ItemType.Weapon)
            equippedWeapon = item;
    }

    private void ValidateEquippedWeapon()
    {
        if (equippedWeapon == null || inventory == null)
            return;

        foreach (Invent.InventorySlot slot in inventory.slots)
        {
            if (!slot.IsEmpty && slot.item == equippedWeapon)
                return;
        }

        equippedWeapon = null;
    }

    public void Initialize()
    {
        anim = GetComponent<Animator>();
        vertical = Animator.StringToHash("Vertical");
        horizontal = Animator.StringToHash("Horizontal");
        isJumping = Animator.StringToHash("IsJumping");

    }
    public bool PlayPickup()
    {
        EnsureAnimator();

        if (!CanStartAction || anim == null)
            return false;

        anim.SetTrigger(PickupTrigger);
        return true;
    }

    public bool PlayLightAttack()
    {
        EnsureAnimator();

        if (!CanStartAction || anim == null)
            return false;

        anim.SetTrigger(LightAttackTrigger);
        return true;
    }

    public IEnumerator WaitForPickupAnimation()
    {
        EnsureAnimator();
        if (anim == null || anim.runtimeAnimatorController == null)
            yield break;

        while (!IsStateActive(PickupState))
            yield return null;

        while (IsStateActive(PickupState))
            yield return null;
    }

    public void SetJumping(bool jumping)
    {
        this.jumping = jumping;
        EnsureAnimator();
        if (anim != null)
            anim.SetBool(isJumping, jumping);
    }

    private void EnsureAnimator()
    {
        if (anim == null)
            anim = GetComponent<Animator>();
    }

    private bool IsStateActive(int stateHash)
    {
        if (anim == null || anim.runtimeAnimatorController == null)
            return false;

        AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);
        if (currentState.shortNameHash == stateHash)
            return true;

        return anim.IsInTransition(0) &&
            anim.GetNextAnimatorStateInfo(0).shortNameHash == stateHash;
    }
    public void UpdateAnimValue(float VerticalMove,float HorizontalMove)
    {
        #region Vertical
        float v = 0;
        if (VerticalMove > 0 && VerticalMove < 0.55f)
        {
            v = 0.5f;
        }
        else if(VerticalMove > 0.55f)
        {
            v = 1;
        }
        else if (VerticalMove < 0 && VerticalMove > -0.55f)
        {
            v = -0.5f;
        }
        else if (VerticalMove < -0.55f)
        {
            v = -1;
        }
        else v = 0;
        #endregion

        #region Horizontal
        float h = 0;
        if (HorizontalMove > 0 && HorizontalMove < 0.55f)
        {
            h = 0.5f;
        }
        else if(HorizontalMove > 0.55f)
        {
            h = 1;
        }
        else if (HorizontalMove < 0 && HorizontalMove > -0.55f)
        {
            h = -0.5f;
        }
        else if (HorizontalMove < -0.55f)
        {
            h = -1;
        }
        else h = 0;
        #endregion

        anim.SetFloat(vertical, v, 0.1f, Time.deltaTime);
        anim.SetFloat(horizontal,h, 0.1f,Time.deltaTime);

    }
    public void CanRotate()
    {
        canRotate = true;
    }
    public void StopRotate()
    {
        canRotate = false;
    }
}
}
