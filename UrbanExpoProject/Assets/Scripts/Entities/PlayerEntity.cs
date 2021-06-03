using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class PlayerEntity : LivingEntity
    {
        [Header("Area Detect Attribute")]
        [SerializeField] private float interactRadius = 1f;
        [SerializeField] private Vector3 interactPivotOffset = new Vector3(0f, 1f, 0f);

        private List<IObjectInteractable> interactNearby = new List<IObjectInteractable>();
        private IObjectInteractable currentlyInteractWith = null;

        #region Properties
        public IObjectInteractable CurrentlyInteractWith 
        {
            set
            {
                if (value == null && currentlyInteractWith != null)
                {
                    currentlyInteractWith.StopInteract();
                }
                currentlyInteractWith = value;
            }
            get => currentlyInteractWith;
        }
        #endregion

        #region Untiy BuiltIn Methods
        private void Update()
        {
            Vector3 origin = transform.position + interactPivotOffset;
            RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, interactRadius, Vector3.zero);

            // Check object interactable nearby
            HashSet<IObjectInteractable> temp = new HashSet<IObjectInteractable>();
            foreach (RaycastHit2D hit in hits)
            {
                IObjectInteractable obj = IslandGrid.GetInteractable(hit.collider.gameObject);
                if (obj == null) continue;
                temp.Add(obj);
            }

            // Check current nearby object is out of radius
            for (int i = interactNearby.Count - 1; i >= 0; i--)
            {
                IObjectInteractable currentNearby = interactNearby[i];
                if (!temp.Contains(currentNearby))
                {
                    interactNearby.RemoveAt(i);
                }
            }
            
            // Check new nearby interactable
            foreach (IObjectInteractable newNearby in temp)
            {
                if (!interactNearby.Contains(newNearby))
                {
                    interactNearby.Add(newNearby);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 origin = transform.position + interactPivotOffset;
            Gizmos.DrawWireSphere(origin, interactRadius);
        }
        #endregion

        public bool IsNearPlayer(IObjectInteractable interactWith)
        {
            return interactNearby.Contains(interactWith);
        }
    }

}