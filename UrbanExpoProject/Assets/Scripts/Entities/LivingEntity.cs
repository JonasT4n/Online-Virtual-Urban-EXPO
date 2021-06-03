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
        [SerializeField] protected SpriteRenderer spriteRenderer = null;
        [SerializeField] protected Animator animator = null;

        [BoxGroup("ABSTRACT DEBUG"), ReadOnly] public IslandArea inCurrentArea = null;

        #region Properties
        public bool IsMoving => controller.IsWalking;
        public SpriteContent Content
        {
            set
            {
                if (string.IsNullOrEmpty(value.name) || value.sprite == null)
                    return;

                spriteRenderer.sprite = value.sprite;
            }
        }
        #endregion

        #region Unity BuiltIn Methods
        protected virtual void Awake()
        {
            // Subscribe events
            EventHandler.OnEntityWalkedIntoGridEvent += EntityWalkedIntoGrid;
        }

        protected virtual void OnDestroy()
        {
            // Unsubscribe events
            EventHandler.OnEntityWalkedIntoGridEvent -= EntityWalkedIntoGrid;
        }
        #endregion

        #region Event Methods
        private void EntityWalkedIntoGrid(OnEntityWalkedIntoGridEventArgs args)
        {
            if (args.Entity.Equals(this))
            {
                IslandArea area = IslandGrid.singleton.GetAreaByGridPosition(args.CurrentGridPosition);
                if (area != null)
                {
                    if (area.Equals(inCurrentArea)) return;
                    inCurrentArea = area;

                    OnEntityEnterAreaEventArgs arg = new OnEntityEnterAreaEventArgs(this, area);
                    EventHandler.CallEvent(arg);
                }
                else
                {
                    if (inCurrentArea == null) return;
                    inCurrentArea = null;

                    OnEntityEnterAreaEventArgs arg = new OnEntityEnterAreaEventArgs(this, area);
                    EventHandler.CallEvent(arg);
                }
            }
        }
        #endregion

        public virtual void CmdTeleport(Vector3Int gridCoordinate)
        {
            if (IslandGrid.singleton.FloorTilemap.GetTile(gridCoordinate) == null)
                return;

            // Set position in world
            transform.position = IslandGrid.singleton.GridToWorldPosition(gridCoordinate) + IslandGrid.singleton.WorldPivotOffset3D;
            controller.MoveToGridPosition(gridCoordinate);
        }

        public void CmdMoveTo(Vector3Int targetGridCoordinate)
        {
            controller.MoveToGridPosition(targetGridCoordinate);
        }

        public void CmdStopMoving()
        {
            controller.StopMoveControl();
        }
    }

}