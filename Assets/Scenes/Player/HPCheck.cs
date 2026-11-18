using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Invent;

namespace Test2
{
    public class PlayerHealth: MonoBehaviour
    {
        private const float HealthPotionCooldown = 3f;

        public int healthLevel = 10;
        public int maxHP;
        public int currentHP;
        public int ShieldHitsRemaining { get; private set; }

        public HealthBar healthBar;
        private Inventory inventory;
        private float nextHealthPotionTime;

        void Start()
        {
            maxHP = SetScalingMaxHP();
            currentHP = maxHP;
            healthBar.MaxHealth(maxHP);
            inventory = GetComponent<Inventory>();
        }

        private void Update()
        {
            if (inventory == null ||
                maxHP <= 0 ||
                currentHP <= 0 ||
                currentHP / (float)maxHP >= 0.5f ||
                Time.time < nextHealthPotionTime)
                return;

            for (int i = 0; i < inventory.slots.Count; i++)
            {
                InventorySlot slot = inventory.slots[i];
                if (slot.IsEmpty ||
                    slot.quantity <= 0 ||
                    slot.item.itemType != ItemType.Consumable ||
                    slot.item.consumableType != ConsumableType.HP ||
                    slot.item.amount <= 0)
                    continue;

                Heal(slot.item.amount);
                inventory.RemoveItem(i);
                nextHealthPotionTime = Time.time + HealthPotionCooldown;
                return;
            }
        }

        private int SetScalingMaxHP()
        {
            maxHP = healthLevel * 10;
            return maxHP;
        }
            public void TakeDamage(int dmg)
            {
                if (dmg <= 0 || currentHP <= 0)
                    return;

                if (ShieldHitsRemaining > 0)
                {
                    ShieldHitsRemaining--;
                    return;
                }

                currentHP = Mathf.Clamp(
                    currentHP - dmg,
                    0,
                    maxHP
                );

                healthBar.CurrentHealth(currentHP);
            }
        public bool TryApplyShield(int blockedHits)
        {
            if (blockedHits <= 0 || currentHP <= 0)
                return false;

            ShieldHitsRemaining = blockedHits;
            return true;
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || currentHP <= 0 || maxHP <= 0)
                return;

            currentHP = Mathf.Min(currentHP + amount, maxHP);
            healthBar.CurrentHealth(currentHP);
        }

        public bool IsDead()
        {
            if (currentHP == 0)
            return true; else return false;
        }
        
    }
}