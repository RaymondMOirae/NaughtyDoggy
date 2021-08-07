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

        protected void DeactivateSign()
        {
            //Debug.Log("Deactivate");
            _interactiveSign.SetActive(false);
        }
        
        protected virtual void ActivateSign()
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