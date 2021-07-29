using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyDoggy.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NaughtyDoggy.Input
{
    public class PlayerInputs : SingletonHelper<PlayerInputs>
    {
        private PlayerInputActions _inputAction;
        public static PlayerInputActions GetInstance => Instance._inputAction;
        
        protected override void Awake()
        {
            base.Awake();
            _inputAction = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _inputAction.Enable();
        }

        private void OnDisable()
        {
            _inputAction.Disable();
        }
    }
    
}

