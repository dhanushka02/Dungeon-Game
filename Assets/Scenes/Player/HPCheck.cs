using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Test2
{
    public class PlayerHealth: MonoBehaviour
    {
        public int healthLevel = 10;
        public int maxHP;
        public int currentHP;

        public HealthBar healthBar;
        void Start()
        {
            maxHP = SetScalingMaxHP();
            currentHP = maxHP;
            healthBar.MaxHealth(maxHP);
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

                currentHP = Mathf.Clamp(
                    currentHP - dmg,
                    0,
                    maxHP
                );

                healthBar.CurrentHealth(currentHP);
            }
        public bool IsDead()
        {
            if (currentHP == 0)
            return true; else return false;
        }
        
    }
}