using UnityEngine;
using UnityEngine.UI;

namespace NPC
{
    public class Status : MonoBehaviour
    {
        protected Animator animator;
        public Transform player;
        protected Image healthFill;
        protected Target golemAI;
        protected float lastAttackTime;
        protected bool isAttacking;
        protected bool isDead;
        protected Vector3 startPosition;
        protected Vector3 endPosition;
        protected Vector3 patrolDirection;

        protected bool movingToEnd = true;
        protected bool isWaiting;
        protected float waitTimer;
    }
}