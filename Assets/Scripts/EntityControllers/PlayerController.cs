using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyDoggy.Input;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyDoggy.Helper;

namespace NaughtyDoggy.EntityControllers 
{
    public class PlayerController : MonoBehaviour
    {
        private const float LeftStickDeadZone = 0.125f;
        private const float TurningThresholdAngle = 120.0f;
        
        public PlayerInputActions _input;
        private Transform _compassTrans;
        private Camera _mainCam;
        private Animator _animator;
        private bool _noInputTrigger = true;
        private Vector2 _lastRawInput;

        private void Awake()
        {
            _input = new PlayerInputActions();
        }

        // Start is called before the first frame update
        void Start()
        {
            SetupInputs();
            _animator = GetComponent<Animator>();
            _mainCam = Camera.main;
            _compassTrans = _mainCam.GetComponentInChildren<Transform>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_noInputTrigger)
            {
                Vector2 animInput = ResolveAnimatorInput(_lastRawInput);
                
                _animator.SetFloat("DirInputX", animInput.x);
                _animator.SetFloat("DirInputY", animInput.y);
            }
            else
            {
                _noInputTrigger = true;
            }
        }
        
        public void HandleDirectionInput(InputAction.CallbackContext cxt)
        {
            Vector2 rawAxis = cxt.ReadValue<Vector2>();
            _lastRawInput = rawAxis;
            
            Vector2 animInput = ResolveAnimatorInput(rawAxis);
            
            _animator.SetFloat("DirInputX", animInput.x);
            _animator.SetFloat("DirInputY", animInput.y);
        }

        
        public Vector2 ResolveAnimatorInput(Vector2 rawAxisInput)
        {
            _noInputTrigger = false;
            Vector2 animInput = Vector2.zero;
            Vector2 animState = new Vector2(_animator.GetFloat("DirInputX"), _animator.GetFloat("DirInputY"));
            
            if (rawAxisInput.magnitude < LeftStickDeadZone)
            {
                return animInput;
            }
            
            Vector2 desiredForward= MathHelper.Vec3XZ(_compassTrans.forward * rawAxisInput.y + _compassTrans.right * rawAxisInput.x);
            float sighedAngle = -1.0f * Vector2.SignedAngle(MathHelper.Vec3XZ(transform.forward), desiredForward);
            
            // means the animator is already in turning state, and should wait for the turning
            bool alreadyTurning = animState.x != 0.0f;
            bool walkForward = Mathf.Abs(sighedAngle) <= TurningThresholdAngle;
            
            if (walkForward) // when the angle between controller's input and player's forward is small
            {
                animInput.x = sighedAngle / TurningThresholdAngle;
                animInput.y = 1.0f - Mathf.Abs(animInput.x);
                animInput = Vector2.Lerp(animState, animInput, 0.2f);
            }
            else if(alreadyTurning) // already turning in some direction
            {
                animInput.x = Mathf.Sign(animState.x);
                animInput.y = 0.0f;
            }
            else // force turning clockwise
            {
                animInput.x = 1.0f;
                animInput.y = 0.0f;
            }

            return animInput;
        }

        private void SetupInputs()
        {
            _input.PlayerController_Map.PlayerActions.performed += context => HandleDirectionInput(context);
            _input.PlayerController_Map.PlayerActions.started   += context => HandleDirectionInput(context);
            _input.PlayerController_Map.PlayerActions.canceled  += context => HandleDirectionInput(context);
        }

        private void OnEnable()
        {
            _input.Enable();
        }

        private void OnDisable()
        {
            _input.Disable();
        }
    }
}

