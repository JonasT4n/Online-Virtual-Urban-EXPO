using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UrbanExpo
{
    public interface IEntityCommands
    {
        void CmdTeleport(Vector3Int gridCoordinate);
        void CmdMoveTo(Vector3Int targetGridCoordinate);
        void CmdStopMoving();
    }
}