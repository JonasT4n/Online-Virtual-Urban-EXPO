using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanExpo
{
    public static class EventHandler
    {
        public delegate void EntityStartWalking(EntityStartWalkingEventArgs args);
        public delegate void EntityWalkedIntoGrid(EntityWalkedIntoGridEventArgs args);

        public static event EntityStartWalking EntityStartWalkingEvent;
        public static event EntityWalkedIntoGrid EntityWalkedIntoGridEvent;

        public static void CallEvent(IEventArguments args)
        {
            if (args is EntityStartWalkingEventArgs)
                EntityStartWalkingEvent?.Invoke((EntityStartWalkingEventArgs)args);
            else if (args is EntityWalkedIntoGridEventArgs)
                EntityWalkedIntoGridEvent?.Invoke((EntityWalkedIntoGridEventArgs)args);

        }
    }

    public class EntityStartWalkingEventArgs : IEventArguments
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
        public EntityStartWalkingEventArgs(LivingEntity entity, Vector3Int fromGrid, Vector3Int toGrid)
        {
            this.entity = entity;
            this.fromGrid = fromGrid;
            this.toGrid = toGrid;
        }
    }

    public class EntityWalkedIntoGridEventArgs : IEventArguments
    {
        private LivingEntity entity;
        private Vector3Int currentGrid;

        #region Properties
        public LivingEntity Entity => entity;
        public Vector3Int CurrentGridPosition => currentGrid;
        #endregion

        public EntityWalkedIntoGridEventArgs(LivingEntity entity, Vector3Int currentGrid)
        {
            this.entity = entity;
            this.currentGrid = currentGrid;
        }
    }
}