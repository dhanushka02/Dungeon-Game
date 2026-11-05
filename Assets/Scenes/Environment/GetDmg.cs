using UnityEngine;
namespace Damage
{
    public class Damage : MonoBehaviour
    {
        public int dmg = 10;
        private void OnCollisionEnter(Collision collision)
        {
            Test2.PlayerHealth playerHealth =
                collision.collider.GetComponentInParent<Test2.PlayerHealth>();

            if (playerHealth != null && !playerHealth.IsDead())
            {
                playerHealth.TakeDamage(dmg);
            }
        }
    }
}
