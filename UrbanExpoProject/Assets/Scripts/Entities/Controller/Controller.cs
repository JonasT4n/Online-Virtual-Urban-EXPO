using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace UrbanExpo
{
    public abstract class Controller : MonoBehaviour
    {
        [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] protected LivingEntity entityControlled = null;
        [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] protected Vector3Int currentGridPos;
        [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private bool isWalking = false;

        #region Properties
        public bool IsWalking { protected set => isWalking = value; get => isWalking; }
        #endregion

        #region Unity BuiltIn Methods
        protected virtual void Start()
        {
            if (entityControlled == null) entityControlled = GetComponent<LivingEntity>();
            currentGridPos = IslandGrid.singleton.WorldToGridPosition(transform.position);
        }

        protected virtual void FixedUpdate()
        {
            // Check player grid position change
            Vector3Int gridPos = IslandGrid.singleton.WorldToGridPosition(transform.position);
            if (currentGridPos != gridPos)
            {
                EntityWalkedIntoGridEventArgs eventArg = new EntityWalkedIntoGridEventArgs(entityControlled, gridPos);
                EventHandler.CallEvent(eventArg);
            }

            // Set current grid position
            currentGridPos = gridPos;
        }
        #endregion

        public abstract void MoveToGridPosition(Vector3Int targetGridCoordinate);
        public abstract void StopMoveControl();
    }

}