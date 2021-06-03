using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanExpo
{
    public interface IObjectInteractable : ISettingInteractable
    {
        bool IsBeingInteract { get; set; }

        void Interact();
        void StopInteract();
        Vector3Int GetCoordinatePosition();
    }

}