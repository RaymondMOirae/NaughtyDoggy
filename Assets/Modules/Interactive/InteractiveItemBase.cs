using UnityEngine;

namespace NaughtyDoggy.Interactive
{
    public class InteractiveItemBase : MonoBehaviour
    {
        [SerializeField] private GameObject _interactiveSign;
        
        protected virtual void Start()
        {
            _interactiveSign = transform.Find("Sign").gameObject;
        }

        private void DeactivateSign()
        {
            //Debug.Log("Deactivate");
            _interactiveSign.SetActive(false);
        }
        
        private void ActivateSign()
        {
            //Debug.Log("Avtivate");
            _interactiveSign.SetActive(true);
        }

        public virtual void HandleInteraction()
        {
            Debug.Log("Interact! " + gameObject.name);
        }
    }
}