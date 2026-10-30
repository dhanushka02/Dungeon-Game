using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Test2;

public class DamagePlayer : MonoBehaviour
{
    public int dmg = 5;
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth PlayerHealth = other.GetComponent<PlayerHealth>();
        if (PlayerHealth != null)
        {
            PlayerHealth.TakeDamage(dmg);
        }
    }
}