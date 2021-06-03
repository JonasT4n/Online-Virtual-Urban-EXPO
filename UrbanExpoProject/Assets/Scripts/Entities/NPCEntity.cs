using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class NPCEntity : LivingEntity, IObjectInteractable
    {
        [Header("Required Components")]
        [SerializeField] private Collider2D colliderInteractor = null;

        [BoxGroup("DBEUG"), SerializeField, ReadOnly] private bool isInteracting = false;

        public bool IsBeingInteract
        {
            set => isInteracting = value;
            get => isInteracting;
        }

        public void SetInteractable(bool interactable)
        {
            isInteracting = false;
            colliderInteractor.enabled = interactable;
        }

        public void Interact()
        {
            isInteracting = true;
            controller.StopMoveControl();
            GameManager.singleton.uiGameManager.SetActiveIContent(true);
        }

        public void StopInteract()
        {
            isInteracting = false;
        }

        public Vector3Int GetCoordinatePosition()
        {
            return IslandGrid.singleton.WorldToGridPosition(transform.localPosition);
        }
    }

}