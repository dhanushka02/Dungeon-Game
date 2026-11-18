using UnityEngine;
using UnityEngine.UI;

namespace NPC
{
    public class Status : MonoBehaviour
    {
        protected Animator animator;
        protected Transform movementRoot;
        [SerializeField] private float facingYawOffset = 180f;
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

        protected void FaceDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                movementRoot.rotation =
                    Quaternion.LookRotation(direction) *
                    Quaternion.Euler(0f, facingYawOffset, 0f);
            }
        }
    }
}