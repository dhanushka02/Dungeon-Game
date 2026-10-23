using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Test2;
//Check Take Dmg - Is Hit and Is death missing

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
        currentHP -= dmg;
    }
    public bool IsDead()
    {
        if (currentHP == 0)
        return true; else return false;
    }
}