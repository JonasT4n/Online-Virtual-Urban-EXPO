using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace UrbanExpo
{
    public abstract class LivingEntity : MonoBehaviour, IEntityCommands
    {
        [Required("Every entities has their own controller to be able to do something")]
        [SerializeField] protected Controller controller = null;
        [SerializeField] protected SpriteRenderer renderer = null;
        [SerializeField] protected Animator animator = null;

        #region Properties
        public bool IsMoving => controller.IsWalking;
        public SpriteContent Content
        {
            set
            {
                if (string.IsNullOrEmpty(value.name) || value.sprite == null)
                    return;

                renderer.sprite = value.sprite;
            }
        }
        #endregion

        public virtual void Teleport(Vector3Int gridCoordinate)
        {
            if (IslandGrid.singleton.FloorTilemap.GetTile(gridCoordinate))
                return;

            // Set position in world
        }

        public void MoveTo(Vector3Int targetGridCoordinate)
        {
            controller.MoveToGridPosition(targetGridCoordinate);
        }

        public void StopMoving()
        {
            controller.StopMoveControl();
        }
    }

}