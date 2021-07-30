// GENERATED AUTOMATICALLY FROM 'Assets/Resources/PlayerInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace NaughtyDoggy.Input
{
    public class @PlayerInputActions : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
        public @PlayerInputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""PlayerController_Map"",
            ""id"": ""8719d7e2-ddd3-48ba-9a76-c4117a063f47"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Value"",
                    ""id"": ""9ac0c7d4-0c38-4c58-bac3-0ef2712a0c6d"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": ""StickDeadzone"",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Interact"",
                    ""type"": ""Button"",
                    ""id"": ""8f2d6766-dcb7-4d4b-bc3a-d1f6f93e1f35"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD_Move"",
                    ""id"": ""841887d3-02d0-4a23-afca-4248698f02e5"",
                    ""path"": ""2DVector(mode=1)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8a0f1a3b-c033-4135-8526-26f029a617fb"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""9a067c12-1a81-4954-a145-112e6e2461ad"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""6abcca7d-9aa9-4512-abfd-93f717887499"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""3cd96371-19f4-4e21-8543-94ac6605bd3d"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""ed021844-7df2-4829-acf5-eaf6c3cc9744"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": ""StickDeadzone"",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ef0bcf7e-506c-405d-821a-1475957c462b"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c4006605-5de6-49aa-8dd7-62fba4552bd1"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // PlayerController_Map
            m_PlayerController_Map = asset.FindActionMap("PlayerController_Map", throwIfNotFound: true);
            m_PlayerController_Map_Movement = m_PlayerController_Map.FindAction("Movement", throwIfNotFound: true);
            m_PlayerController_Map_Interact = m_PlayerController_Map.FindAction("Interact", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        // PlayerController_Map
        private readonly InputActionMap m_PlayerController_Map;
        private IPlayerController_MapActions m_PlayerController_MapActionsCallbackInterface;
        private readonly InputAction m_PlayerController_Map_Movement;
        private readonly InputAction m_PlayerController_Map_Interact;
        public struct PlayerController_MapActions
        {
            private @PlayerInputActions m_Wrapper;
            public PlayerController_MapActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @Movement => m_Wrapper.m_PlayerController_Map_Movement;
            public InputAction @Interact => m_Wrapper.m_PlayerController_Map_Interact;
            public InputActionMap Get() { return m_Wrapper.m_PlayerController_Map; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(PlayerController_MapActions set) { return set.Get(); }
            public void SetCallbacks(IPlayerController_MapActions instance)
            {
                if (m_Wrapper.m_PlayerController_MapActionsCallbackInterface != null)
                {
                    @Movement.started -= m_Wrapper.m_PlayerController_MapActionsCallbackInterface.OnMovement;
                    @Movement.performed -= m_Wrapper.m_PlayerController_MapActionsCallbackInterface.OnMovement;
                    @Movement.canceled -= m_Wrapper.m_PlayerController_MapActionsCallbackInterface.OnMovement;
                    @Interact.started -= m_Wrapper.m_PlayerController_MapActionsCallbackInterface.OnInteract;
                    @Interact.performed -= m_Wrapper.m_PlayerController_MapActionsCallbackInterface.OnInteract;
                    @Interact.canceled -= m_Wrapper.m_PlayerController_MapActionsCallbackInterface.OnInteract;
                }
                m_Wrapper.m_PlayerController_MapActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Movement.started += instance.OnMovement;
                    @Movement.performed += instance.OnMovement;
                    @Movement.canceled += instance.OnMovement;
                    @Interact.started += instance.OnInteract;
                    @Interact.performed += instance.OnInteract;
                    @Interact.canceled += instance.OnInteract;
                }
            }
        }
        public PlayerController_MapActions @PlayerController_Map => new PlayerController_MapActions(this);
        public interface IPlayerController_MapActions
        {
            void OnMovement(InputAction.CallbackContext context);
            void OnInteract(InputAction.CallbackContext context);
        }
    }
}
