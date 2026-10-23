using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Test2;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    public void MaxHealth (int MHP)
    {
        slider.value = MHP;
        slider.maxValue = MHP;
    }
    public void CurrentHealth(int CHP)
    {
        slider.value = CHP;

    }

}