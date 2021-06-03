using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class InteractableBuilding : Building, IObjectInteractable
    {
        [Header("Required Components")]
        [SerializeField] private Collider2D colliderInteractor = null;

        [BoxGroup("DBEUG"), SerializeField, ReadOnly] private bool isBeingInteract = false;

        public bool IsBeingInteract
        {
            set => isBeingInteract = value;
            get => isBeingInteract;
        }

        public void SetInteractable(bool interactable)
        {
            isBeingInteract = false;
            colliderInteractor.enabled = interactable;
        }

        public void Interact()
        {
            isBeingInteract = true;
            GameManager.singleton.uiGameManager.SetActiveIContent(true);
        }

        public void StopInteract()
        {
            isBeingInteract = false;
        }

        public Vector3Int GetCoordinatePosition()
        {
            return IslandGrid.singleton.WorldToGridPosition(transform.localPosition);
        }
    }

}