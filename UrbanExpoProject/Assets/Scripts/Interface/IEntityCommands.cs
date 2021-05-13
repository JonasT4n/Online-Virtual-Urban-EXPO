using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UrbanExpo
{
    public interface IEntityCommands
    {
        void Teleport(Vector3Int gridCoordinate);
        void MoveTo(Vector3Int targetGridCoordinate);
        void StopMoving();
    }
}