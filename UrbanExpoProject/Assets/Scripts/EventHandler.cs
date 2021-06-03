using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanExpo
{
    public static class EventHandler
    {
        public delegate void OnPlayerTileSelect(OnPlayerTileSelectEventArgs args);
        public delegate void OnEntityStartWalk(OnEntityStartWalkEventArgs args);
        public delegate void OnEntityWalkedIntoGrid(OnEntityWalkedIntoGridEventArgs args);
        public delegate void OnPlayerObjectInteract(OnPlayerObjectInteractEventArgs args);
        public delegate void OnEntityEnterArea(OnEntityEnterAreaEventArgs args);

        public static event OnPlayerTileSelect OnPlayerTileSelectEvent;
        public static event OnEntityStartWalk OnEntityStartWalkEvent;
        public static event OnEntityWalkedIntoGrid OnEntityWalkedIntoGridEvent;
        public static event OnPlayerObjectInteract OnPlayerObjectInteractEvent;
        public static event OnEntityEnterArea OnEntityEnterAreaEvent;

        public static void CallEvent(IEventArguments args)
        {
            if (args is OnPlayerTileSelectEventArgs)
                OnPlayerTileSelectEvent?.Invoke((OnPlayerTileSelectEventArgs)args);
            else if (args is OnEntityStartWalkEventArgs)
                OnEntityStartWalkEvent?.Invoke((OnEntityStartWalkEventArgs)args);
            else if (args is OnEntityWalkedIntoGridEventArgs)
                OnEntityWalkedIntoGridEvent?.Invoke((OnEntityWalkedIntoGridEventArgs)args);
            else if (args is OnPlayerObjectInteractEventArgs)
                OnPlayerObjectInteractEvent?.Invoke((OnPlayerObjectInteractEventArgs)args);
            else if (args is OnEntityEnterAreaEventArgs)
                OnEntityEnterAreaEvent?.Invoke((OnEntityEnterAreaEventArgs)args);
        }
    }

    public class OnPlayerTileSelectEventArgs : IEventArguments
    {
        private PlayerEntity player;
        private Vector3Int targetGrid;

        #region Properties
        public PlayerEntity Player => player;
        public Vector3Int GrisPositionSelected
        {
            set
            {
                if (IslandGrid.singleton.FloorTilemap.GetTile(value) != null)
                    targetGrid = value;
            }
            get => targetGrid;
        }
        #endregion

        public OnPlayerTileSelectEventArgs(PlayerEntity player, Vector3Int targetGrid)
        {
            this.player = player;
            this.targetGrid = targetGrid;
        }
    }

    public class OnEntityStartWalkEventArgs : IEventArguments
    {
        private LivingEntity entity;
        private readonly Vector3Int fromGrid;
        private Vector3Int toGrid;

        #region Properties
        public LivingEntity Entity => entity;
        public Vector3Int OriginGridPosition => fromGrid;
        public Vector3Int TargetGridPosition
        {
            set
            {
                if (IslandGrid.singleton.FloorTilemap.GetTile(value) != null)
                    toGrid = value;
            }
            get => toGrid;
        }
        #endregion

        /// <summary>
        /// Event called when any entity start walking from grid position to target grid position 
        /// </summary>
        /// <param name="entity">Entity who start walking</param>
        /// <param name="fromGrid">From entity grid position</param>
        /// <param name="toGrid">Target grid position</param>
        public OnEntityStartWalkEventArgs(LivingEntity entity, Vector3Int fromGrid, Vector3Int toGrid)
        {
            this.entity = entity;
            this.fromGrid = fromGrid;
            this.toGrid = toGrid;
        }
    }

    public class OnEntityWalkedIntoGridEventArgs : IEventArguments
    {
        private LivingEntity entity;
        private Vector3Int currentGrid;

        #region Properties
        public LivingEntity Entity => entity;
        public Vector3Int CurrentGridPosition => currentGrid;
        #endregion

        public OnEntityWalkedIntoGridEventArgs(LivingEntity entity, Vector3Int currentGrid)
        {
            this.entity = entity;
            this.currentGrid = currentGrid;
        }
    }

    public class OnPlayerObjectInteractEventArgs : IEventArguments, ICancelable
    {
        private PlayerEntity player;
        private IObjectInteractable interactWith;

        #region Properties
        public PlayerEntity Player => player;
        public IObjectInteractable InteractTarget => interactWith;
        public bool IsCancelled { get; set; } = false;
        #endregion

        public OnPlayerObjectInteractEventArgs(PlayerEntity player, IObjectInteractable interactWith)
        {
            this.player = player;
            this.interactWith = interactWith;
        }
    }

    public class OnEntityEnterAreaEventArgs : IEventArguments
    {
        private LivingEntity entity;
        private IslandArea area;

        #region Properties
        public LivingEntity Entity => entity;
        public IslandArea Area => area;
        #endregion

        public OnEntityEnterAreaEventArgs(LivingEntity entity, IslandArea area)
        {
            this.entity = entity;
            this.area = area;
        }
    }
}