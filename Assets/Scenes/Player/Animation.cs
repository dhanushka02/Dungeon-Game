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
        Animator.StringToHash("Unarmed Attack");
    private static readonly int LongSwordAttackState =
        Animator.StringToHash("LongSword Attack");

    [Header("Equipment State")]
    [SerializeField] private Invent.ItemData equippedWeapon;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private Vector3 weaponSocketPosition = new Vector3(-0.08881251f, -0.022271466f, 0.03837269f);
    [SerializeField] private Vector3 weaponSocketRotation = new Vector3(68.2227f, 114.4689f, 92.3270f);

    public Animator anim;
    int vertical;
    int horizontal;
    public bool canRotate;
    int isJumping;
    private bool jumping;
    private bool dodging;
    private bool dodgePending;
    private Invent.Inventory inventory;
    private GameObject weaponVisual;
    private bool forearmHitboxesConfigured;

    public bool HasEquippedWeapon => equippedWeapon != null;
    public Invent.ItemData EquippedWeapon => equippedWeapon;
    public bool CanStartAction => !dodgePending && CanStartDodge(false);

    public bool HasWeaponInHand()
    {
        EnsureAnimator();
        if (anim == null || !anim.isHuman)
            return false;

        Transform hand = anim.GetBoneTransform(HumanBodyBones.RightHand);
        if (hand == null)
            return false;

        foreach (Transform child in hand.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.activeInHierarchy && child.CompareTag("Weapon"))
                return true;
        }

        return false;
    }

    public bool CanStartDodge(bool afterCombo)
    {
        EnsureAnimator();
        return !jumping && !dodging &&
            !IsStateActive(JumpState) &&
            !IsStateActive(PickupState) &&
            (afterCombo ||
                (!IsStateActive(LightAttackState) && !IsStateActive(LongSwordAttackState)));
    }

    private void OnEnable()
    {
        inventory = GetComponent<Invent.Inventory>();
        if (inventory == null)
            return;

        inventory.OnInventoryChanged += ValidateEquippedWeapon;
        ValidateEquippedWeapon();
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= ValidateEquippedWeapon;
        }

        inventory = null;
        ClearWeaponVisual();
    }

    private void ValidateEquippedWeapon()
    {
        Invent.ItemData weapon = inventory != null ? inventory.EquippedWeapon : null;
        if (equippedWeapon == weapon && (weapon == null || weaponVisual != null))
            return;

        equippedWeapon = weapon;
        ClearWeaponVisual();
        if (weapon == null)
            return;

        if (weapon.worldPrefab == null)
        {
            Debug.LogError("Equipped weapon has no world prefab: " + weapon.name, weapon);
            return;
        }

        if (weaponSocket == null)
        {
            EnsureAnimator();
            if (anim == null || !anim.isHuman)
            {
                Debug.LogError("Cannot create a weapon socket without a humanoid Animator.", this);
                return;
            }

            Transform hand = anim.GetBoneTransform(HumanBodyBones.RightHand);
            if (hand == null)
            {
                Debug.LogError("Cannot find the right hand bone for the weapon socket.", this);
                return;
            }

            weaponSocket = new GameObject("RightHandWeaponSocket").transform;
            weaponSocket.SetParent(hand, false);
            weaponSocket.localPosition = weaponSocketPosition;
            weaponSocket.localRotation = Quaternion.Euler(weaponSocketRotation);
        }

        weaponVisual = Instantiate(weapon.worldPrefab, weaponSocket, false);
        weaponVisual.tag = "Weapon";
        foreach (Invent.ItemPickup pickup in weaponVisual.GetComponentsInChildren<Invent.ItemPickup>(true))
            pickup.enabled = false;
        foreach (Collider collider in weaponVisual.GetComponentsInChildren<Collider>(true))
            collider.enabled = false;
        foreach (Rigidbody body in weaponVisual.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.useGravity = false;
        }

        MeshFilter[] meshes = weaponVisual.GetComponentsInChildren<MeshFilter>();
        if (meshes.Length == 0)
            Debug.LogError("Equipped weapon has no mesh for its attack hitbox.", weaponVisual);

        foreach (MeshFilter mesh in meshes)
        {
            if (mesh.sharedMesh == null)
            {
                Debug.LogError("Equipped weapon attack hitbox requires a mesh.", mesh);
                continue;
            }

            MeshCollider hitbox = mesh.gameObject.AddComponent<MeshCollider>();
            hitbox.sharedMesh = mesh.sharedMesh;
            hitbox.convex = true;
            hitbox.isTrigger = true;
            mesh.gameObject.AddComponent<DealDmg>().Configure(true);
        }
    }

    private void ClearWeaponVisual()
    {
        if (weaponVisual == null)
            return;

        weaponVisual.SetActive(false);
        Destroy(weaponVisual);
        weaponVisual = null;
    }

    public void Initialize()
    {
        anim = GetComponent<Animator>();
        vertical = Animator.StringToHash("Vertical");
        horizontal = Animator.StringToHash("Horizontal");
        isJumping = Animator.StringToHash("IsJumping");

        if (!forearmHitboxesConfigured)
        {
            if (anim == null || !anim.isHuman)
            {
                Debug.LogError("Player attack hitboxes require a humanoid Animator.", this);
                return;
            }

            ConfigureForearmHitbox(HumanBodyBones.LeftLowerArm);
            ConfigureForearmHitbox(HumanBodyBones.RightLowerArm);
            forearmHitboxesConfigured = true;
        }
    }

    private void ConfigureForearmHitbox(HumanBodyBones bone)
    {
        Transform forearm = anim.GetBoneTransform(bone);
        if (forearm == null)
        {
            Debug.LogError("Player attack hitbox cannot find bone " + bone, this);
            return;
        }

        CapsuleCollider collider = forearm.GetComponent<CapsuleCollider>();
        if (collider == null || !collider.enabled || !collider.isTrigger)
        {
            Debug.LogError("Player forearm requires an enabled trigger Capsule Collider.", forearm);
            return;
        }

        DealDmg hitbox = forearm.GetComponent<DealDmg>();
        if (hitbox == null)
            hitbox = forearm.gameObject.AddComponent<DealDmg>();
        hitbox.Configure(false);
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

    public void SetDodging(bool dodging)
    {
        this.dodging = dodging;
    }

    public void SetDodgePending(bool pending)
    {
        dodgePending = pending;
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
