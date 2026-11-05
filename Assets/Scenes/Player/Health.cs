using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Test2
{
    public class HealthBar : MonoBehaviour
    {
        public Slider slider;

        public void MaxHealth (int MHP)
        {
            slider.maxValue = MHP;
            slider.value = MHP;
        }
        public void CurrentHealth(int CHP)
        {
            slider.value = CHP;

        }

    }
}