using System;
using UnityEngine;
using NPC;

namespace Test2
{
    [RequireComponent(typeof(Collider))]
    public class DealDmg : MonoBehaviour
    {
        public static event Action<EnemyHealth> EnemyHit;

        [SerializeField] private int dmg = 10;
        [SerializeField] private Combo combo;
        private bool weaponHitbox;

        private void Awake()
        {
            if (combo == null)
                combo = GetComponentInParent<Combo>();
        }

        public void Configure(bool isWeapon)
        {
            weaponHitbox = isWeapon;
        }

        private void OnCollisionEnter(Collision collision) => TryDamage(collision.collider);
        private void OnCollisionStay(Collision collision) => TryDamage(collision.collider);
        private void OnTriggerEnter(Collider other) => TryDamage(other);
        private void OnTriggerStay(Collider other) => TryDamage(other);

        private void TryDamage(Collider other)
        {
            if (combo == null)
                combo = GetComponentInParent<Combo>();

            if (combo == null || !combo.IsAttacking || !(other is CapsuleCollider))
                return;

            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
            if (enemy == null || enemy.IsDead() || dmg <= 0 ||
                !combo.TryRegisterHit(enemy, weaponHitbox))
                return;

            enemy.TakeDamage(dmg);
            EnemyHit?.Invoke(enemy);
        }
    }
}
