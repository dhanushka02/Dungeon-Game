using NPC;
using Test2;
using UnityEngine;

namespace Invent
{
    public class EnemyHealthHUD : MonoBehaviour
    {
        [SerializeField] private BossHealthBar healthBar;
        [SerializeField] private RectTransform playerHealthFrame;

        private Target selectedTarget;

        private void Awake()
        {
            if (healthBar == null || healthBar.healthBarFill == null ||
                healthBar.bossNameText == null || playerHealthFrame == null)
            {
                Debug.LogError("Enemy health HUD requires the boss bar, its fill and name, and the player health frame.", this);
                enabled = false;
                return;
            }

            RectTransform background = healthBar.healthBarFill.transform.parent as RectTransform;
            if (background == null)
            {
                Debug.LogError("Enemy health HUD requires a RectTransform around its fill.", this);
                enabled = false;
                return;
            }

            background.sizeDelta = playerHealthFrame.rect.size / 3f;
        }

        private void OnEnable()
        {
            DealDmg.EnemyHit += OnEnemyHit;
        }

        private void OnDisable()
        {
            DealDmg.EnemyHit -= OnEnemyHit;
            selectedTarget = null;
            if (healthBar != null)
                healthBar.HideBossHealth();
        }

        private void OnEnemyHit(EnemyHealth enemy)
        {
            Target target = enemy.GetComponent<Target>();
            if (IsEligible(target))
                selectedTarget = target;
        }

        private void LateUpdate()
        {
            if (!IsEligible(selectedTarget))
                selectedTarget = FindNearestDetectedTarget();

            if (selectedTarget == null)
            {
                healthBar.HideBossHealth();
                return;
            }

            EnemyHealth health = selectedTarget.GetComponent<EnemyHealth>();
            healthBar.bossNameText.text = health.stat.name;
            healthBar.UpdateHealth(health.CurrentHealth, health.stat.maxHealth);
            healthBar.ShowBossHealth();
        }

        private static bool IsEligible(Target target)
        {
            if (target == null || !target.DetectsPlayer)
                return false;

            EnemyHealth health = target.GetComponent<EnemyHealth>();
            CapsuleCollider capsule = target.GetComponentInChildren<CapsuleCollider>();
            return health != null && health.isActiveAndEnabled && !health.IsDead() &&
                health.stat != null && health.stat.maxHealth > 0 &&
                capsule != null && capsule.enabled;
        }

        private static Target FindNearestDetectedTarget()
        {
            Target nearest = null;
            float nearestDistance = float.PositiveInfinity;
            foreach (Target target in Target.ActiveTargets)
            {
                if (!IsEligible(target))
                    continue;

                Vector3 offset = target.player.position - target.transform.root.position;
                offset.y = 0f;
                float distance = offset.sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = target;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }
    }
}
