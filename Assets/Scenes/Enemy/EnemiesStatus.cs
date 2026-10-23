using UnityEngine;
namespace NPC
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Scenes/Script")]
    public class EnemiesStatus : ScriptableObject
    {
        public int maxHealth;
        public float moveSpeed;
        public float patrolDistance ; 
        public float detectionRange;
        public float attackRange;
        public int attackDamage;
        public float attackCooldown;
        public float attackDuration;
        public float damageDelay;
        public float waitTime ;
        protected string Type;

    }
}