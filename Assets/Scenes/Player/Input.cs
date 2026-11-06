using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using NPC;
namespace Test2
{
    public class InputHandler : MonoBehaviour
    {
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public float MouseX;
        public float MouseY;
        public bool Jump;
        public bool Interact;

        public bool Light;
        public bool Heavy;

        PlayerMovement Action;
        Cam cameraHandler;
        Vector2 MovementInput;
        Vector2 CameraInput;
        // private void Awake()
        // {
        //     cameraHandler = Cam.sigleton;
        // }
        // private void FixedUpdate()
        // {
        //     float delta = Time.fixedDeltaTime;
        //     if (cameraHandler != null)
        //     {
        //         cameraHandler.FollowTarget(delta);
        //         cameraHandler.HanldeCameraRotation(delta, MouseX, MouseY);
        //     }
        // }
        private void Start()
        {
            cameraHandler = Cam.sigleton;
        }

        private void LateUpdate()
        {
            if (cameraHandler == null)
                return;

            cameraHandler.HanldeCameraRotation(
                Time.deltaTime,
                MouseX,
                MouseY
            );
        }
        public void OnEnable()
        {
            if (Action == null)
            {
                Action = new PlayerMovement();
                Action.Player.Move.performed += Action => MovementInput = Action.ReadValue<Vector2>();
                Action.Player.Move.canceled += Action => MovementInput = Vector2.zero;
                Action.Player.Look.performed += i => CameraInput = i.ReadValue<Vector2>();
                Action.Player.Look.canceled += i => CameraInput = Vector2.zero;
                Action.Player.Jump.performed += ctx => Jump = true;
                Action.Player.Jump.canceled += ctx => Jump = false;           
                Action.Player.Interact.performed += ctx => Interact = true;
                Action.Player.Interact.canceled += ctx => Interact = false; 
            }
            Action.Enable();
        }

        private void OnDisable()
        {
            Action.Disable();
        }

        public void TickInput(float delta)
        {
            MoveInput(delta);
        }
        private void MoveInput(float delta)
        {
            horizontal = MovementInput.x;
            vertical = MovementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            MouseX = CameraInput.x;
            MouseY = CameraInput.y;
        }



    }
}

