using UnityEngine;

namespace Test2
{
    [RequireComponent(typeof(InputHandler), typeof(Animation))]
    public class Attack : MonoBehaviour
    {
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private Animation animationHandler;
        [SerializeField] private Combo combo;

        private void Awake()
        {
            if (inputHandler == null)
                inputHandler = GetComponent<InputHandler>();

            if (animationHandler == null)
                animationHandler = GetComponent<Animation>();

            if (combo == null)
                combo = GetComponent<Combo>();

            if (combo == null)
                combo = gameObject.AddComponent<Combo>();
        }

        public void HandleInput()
        {
            if (inputHandler == null || animationHandler == null || combo == null)
                return;

            if (!inputHandler.ConsumeLightAttack())
                return;

            if (combo.IsAttacking || animationHandler.CanStartAction)
                combo.OnAttackPressed();
        }
    }
}
