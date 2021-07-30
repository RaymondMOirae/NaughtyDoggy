


namespace NaughtyDoggy.PlayerControls
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

