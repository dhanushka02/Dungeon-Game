using UnityEngine;

namespace NPC
{
    [RequireComponent(typeof(Collider))]
    public class EnemyAttackHitbox : MonoBehaviour
    {
        private Attack attack;

        private void Awake()
        {
            attack = GetComponentInParent<Attack>();
            if (attack == null)
            {
                Debug.LogError("Enemy attack hitbox requires an Attack component on a parent.", this);
                enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other) => attack.TryHitPlayer(other);
        private void OnTriggerStay(Collider other) => attack.TryHitPlayer(other);
        private void OnCollisionEnter(Collision collision) => attack.TryHitPlayer(collision.collider);
        private void OnCollisionStay(Collision collision) => attack.TryHitPlayer(collision.collider);
    }
}
