using System;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyDoggy.Helper;
using NaughtyDoggy.Interactive;
using NaughtyDoggy.UI;

namespace NaughtyDoggy.PlayerControls
{
    public class PlayerController : MonoBehaviour
    {
        private const float LeftStickDeadZone = 0.125f;
        private const float TurningThresholdAngle = 120.0f;
        
        private Transform _compassTrans;
        private Camera _mainCam;
        private Animator _animator;
        private bool _noInputTrigger = true;
        private Vector2 _lastRawInput;

        private InteractionTargetDetector _detector;

        // Start is called before the first frame update
        void Start()
        {
            BindInputs();
            InitComponents();
        }

        private void Update()
        {
            Shader.SetGlobalVector("_PlayerPos", transform.position);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // persisit per frame animator updat
            if (_noInputTrigger)
            {
                UpdateAnimationParameter(_lastRawInput);
            }
            else
            {
                _noInputTrigger = true;
            }
        }

        #region Animation&Movement

        private void UpdateAnimationParameter(Vector2 playerInput)
        {
                Vector2 animInput = ResolveInputToAnimator(playerInput);
                
                _animator.SetFloat("DirInputX", animInput.x);
                _animator.SetFloat("DirInputY", animInput.y);
        }
        
        public void HandleDirectionInput(InputAction.CallbackContext cxt)
        {
            Vector2 rawAxis = cxt.ReadValue<Vector2>();
            _lastRawInput = rawAxis;
            UpdateAnimationParameter(rawAxis);
        }

        public Vector2 ResolveInputToAnimator(Vector2 rawAxisInput)
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

        
        
        #endregion

        #region Inits

        private void BindInputs()
        {
            PlayerInputs.GetInstance.PlayerController_Map.Movement.started   += context => HandleDirectionInput(context);
            PlayerInputs.GetInstance.PlayerController_Map.Movement.performed += context => HandleDirectionInput(context);
            PlayerInputs.GetInstance.PlayerController_Map.Movement.canceled  += context => HandleDirectionInput(context);
            PlayerInputs.GetInstance.PlayerController_Map.Interact.started   += context => LaunchInteraction();
            PlayerInputs.GetInstance.PlayerController_Map.StartMenu.performed += context => GameManager.Instance.RestartGame();
            PlayerInputs.GetInstance.PlayerController_Map.StartMenu.performed += context => GameManager.Instance.RestartGame();
            PlayerInputs.GetInstance.PlayerController_Map.Terminate.performed += context => GameManager.Instance.EndGame();
        }
        
        private void InitComponents()
        {
            _animator = GetComponent<Animator>();
            _mainCam = Camera.main;
            _compassTrans = _mainCam.GetComponentInChildren<Transform>();
            _detector = transform.Find("Detector").GetComponent<InteractionTargetDetector>();
        }
        
        #endregion

        #region InteractiveActions

        private void LaunchInteraction()
        {
            if (_detector.FocusTarget)
            {
                _detector.FocusTarget.HandleInteraction();
            }
        }

        #endregion
    }
}

