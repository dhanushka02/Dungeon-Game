using UnityEngine;

namespace Test2
{
    [RequireComponent(typeof(InputHandler), typeof(Animation))]
    public class Attack : MonoBehaviour
    {
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private Animation animationHandler;

        private void Awake()
        {
            if (inputHandler == null)
                inputHandler = GetComponent<InputHandler>();

            if (animationHandler == null)
                animationHandler = GetComponent<Animation>();
        }

        public void HandleInput()
        {
            if (inputHandler == null || animationHandler == null)
                return;

            if (!inputHandler.ConsumeLightAttack())
                return;

            animationHandler.PlayLightAttack();
        }
    }
}