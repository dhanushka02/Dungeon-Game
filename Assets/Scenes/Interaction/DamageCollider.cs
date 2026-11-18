using System.Collections.Generic;
using UnityEngine;

namespace Damage
{
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }

    public class DamageCollider : MonoBehaviour
    {
        private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
        private bool attackActive;
        private int damage;
        private LayerMask targetLayers;

        private void Awake()
        {
            if (GetComponent<Collider>() == null)
            {
                Debug.LogError("DamageCollider requires a Collider on the weapon object.", this);
            }
        }

        public void BeginAttack(int attackDamage, LayerMask layers)
        {
            damage = attackDamage;
            targetLayers = layers;
            hitTargets.Clear();
            attackActive = true;
        }

        public void EndAttack()
        {
            attackActive = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            TryDamage(collision.collider);
        }

        private void OnCollisionStay(Collision collision)
        {
            TryDamage(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryDamage(other);
        }

        private void OnDisable()
        {
            attackActive = false;
        }

        private void TryDamage(Collider other)
        {
            if (!attackActive || (targetLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target != null && hitTargets.Add(target))
            {
                target.TakeDamage(damage);
            }
        }
    }
}
