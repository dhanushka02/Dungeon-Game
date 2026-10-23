using UnityEngine;
using UnityEngine.UI;
using NPC;

namespace NPC
{
    class Program : MonoBehaviour
    {
        public Status status;
        public Attack attack;
        public GolemHealth health;
        public Target target;
        void Start()
        {
            Debug.Log("Connect File Succeed, prepare to detect");
            target = GetComponent<Target>();
            attack = GetComponent<Attack>();
            health = GetComponent<GolemHealth>();
        }
        void Update()
        {
            if (health != null && health.CurrentHealth <= 0)
            {
                Debug.Log("Golem is dead");
                return; 
            }
        }
    }
}