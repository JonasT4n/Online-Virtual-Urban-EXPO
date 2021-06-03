using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NaughtyAttributes;

namespace UrbanExpo
{
    [RequireComponent(typeof(PlayerEntity))]
    public class PlayerController : Controller
    {
        [Header("Controller Data Attributes")]
        [SerializeField] private float walkSpeed = 3.5f;

        private AStar pathFinder;
        private PlayerEntity playerEntity;
        private IEnumerator moveProcessRoutine;
        private InputHandler inputHandler;

        #region Unity BuiltIn Methods
        protected override void Start()
        {
            inputHandler = GameManager.singleton.uiGameManager.InputHandler;
            playerEntity = GetComponent<PlayerEntity>();
            entityControlled = playerEntity;
            targetGridCoordinate = IslandGrid.singleton.WorldToGridPosition(transform.position);
            pathFinder = new AStar(IslandGrid.singleton, () => transform.position);
        }

        private void Update()
        {
            // Check player has input handler
            if (inputHandler == null) return;

            // Handle control
            if (!GameManager.singleton.uiGameManager.IsConfiguring) HandlePlayerControl();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        #endregion

        /// <summary>
        /// Translate input into control data
        /// </summary>
        private void HandlePlayerControl()
        {
            // Priority check interaction clicked
            if (inputHandler.Data.objOnMousePosition != null)
            {
                IObjectInteractable interact = IslandGrid.GetInteractable(inputHandler.Data.objOnMousePosition);
                if (interact != null && inputHandler.Data.click)
                {
                    if (playerEntity.IsNearPlayer(interact))
                    {
                        playerEntity.CurrentlyInteractWith = interact;
                        interact.Interact();
                        return;
                    }

                    //if (playerEntity.CurrentlyInteractWith != null)
                    //{
                    //    playerEntity.CurrentlyInteractWith.IsBeingInteract = false;
                    //    playerEntity.CurrentlyInteractWith = null;
                    //}

                    //PlayerGoingInteractWith(interact);
                    //return;
                }
            }

            // Initialize starting information
            IslandGrid island = IslandGrid.singleton;
            inputHandler.Data.mousePosInWorld.z = 0;
            Vector3Int gridCoordinate = island.WorldToGridPosition(inputHandler.Data.mousePosInWorld);

            //Debug.Log($"Mouse pos in world: {inputHandler.Data.mousePosInWorld}; Grid coordinate: {gridCoordinate}");

            if (island.FloorTilemap.gameObject.Equals(inputHandler.Data.objOnMousePosition))
            {
                // Movement selection control
                bool floorTileExists = island.IsFloorTileExists(gridCoordinate);
                if (floorTileExists && inputHandler.Data.click)
                    MoveToGridPosition(gridCoordinate);
            }

            // Local camera control
            GameManager.singleton.CameraController.Zoom(inputHandler.Data.zoomingControlValue);

            // Debug grid
            IslandGrid.singleton.DrawSquareGrid(gridCoordinate);
        }

        public override void MoveToGridPosition(Vector3Int targetGridCoordinate)
        {
            this.targetGridCoordinate = targetGridCoordinate;
            List<Vector3> pathsToTarget = pathFinder.SearchPath(targetGridCoordinate);
            PlayerStartMove(pathsToTarget, null);
        }

        public override void StopMoveControl()
        {
            // Check current task routine is running
            if (moveProcessRoutine != null)
            {
                StopCoroutine(moveProcessRoutine);
                if (playerEntity.CurrentlyInteractWith != null)
                    playerEntity.CurrentlyInteractWith.IsBeingInteract = false;
            }

            // Set this current position and walk snap to the grid coordinate
            IsWalking = false;
            targetGridCoordinate = IslandGrid.singleton.WorldToGridPosition(transform.position);
            Vector3 snappedPos = IslandGrid.singleton.GridToWorldPosition(targetGridCoordinate) + IslandGrid.singleton.WorldPivotOffset3D;
            PlayerStartMove(new List<Vector3>() { snappedPos }, null);
        }

        private void PlayerStartMove(List<Vector3> paths, IObjectInteractable interactWith)
        {
            // Check current task routine is running
            if (moveProcessRoutine != null)
            {
                StopCoroutine(moveProcessRoutine);
                if (playerEntity.CurrentlyInteractWith != null)
                    playerEntity.CurrentlyInteractWith.IsBeingInteract = false;
            }

            // Run a move task routine
            moveProcessRoutine = MovementRoutine(paths, interactWith);
            StartCoroutine(moveProcessRoutine);
        }

        [System.Obsolete]
        private void PlayerGoingInteractWith(IObjectInteractable interactWith)
        {
            // Get object position in coordinate
            Vector3Int gridPos = interactWith.GetCoordinatePosition();
            gridPos.z = 0;

            // Change target grid position if the object not placed on the floor tile or it is living entity
            if (IslandGrid.singleton.FloorTilemap.GetTile(gridPos) != null || interactWith is LivingEntity)
            {
                IslandGrid island = IslandGrid.singleton;
                List<Vector3Int> targetCandidate = new List<Vector3Int>();

                // Check tiles for target coordinates
                Vector3Int targetUpward = gridPos + island.map.GetGridDirection(IslandGridNormalDirection.Up);
                if (island.FloorTilemap.GetTile(targetUpward)) targetCandidate.Add(targetUpward);

                Vector3Int targetDownward = gridPos + island.map.GetGridDirection(IslandGridNormalDirection.Down);
                if (island.FloorTilemap.GetTile(targetDownward)) targetCandidate.Add(targetDownward);

                Vector3Int targetLeftward = gridPos + island.map.GetGridDirection(IslandGridNormalDirection.Left);
                if (island.FloorTilemap.GetTile(targetLeftward)) targetCandidate.Add(targetLeftward);

                Vector3Int targetRightward = gridPos + island.map.GetGridDirection(IslandGridNormalDirection.Right);
                if (island.FloorTilemap.GetTile(targetRightward)) targetCandidate.Add(targetRightward);

                // Check whether there are no way to go to any target coordinate to interact with
                if (targetCandidate.Count == 0) return;

                // Check nearest grid coordinate
                Vector3Int nearest = targetCandidate[0];
                for (int i = 1; i < targetCandidate.Count; i++)
                {
                    float distanceNew = Vector3Int.Distance(currentGridPos, targetCandidate[i]);
                    if (distanceNew < Vector3Int.Distance(currentGridPos, nearest))
                        nearest = targetCandidate[i];
                }
                gridPos = nearest;
            }

            // Check if the object is living entity, make sure the entity stop first
            playerEntity.CurrentlyInteractWith = interactWith;
            interactWith.IsBeingInteract = true;
            if (interactWith is LivingEntity)
            {
                LivingEntity entity = (LivingEntity)interactWith;
                entity.CmdStopMoving();
            }

            // Start move to target
            targetGridCoordinate = gridPos;
            List<Vector3> pathsToTarget = pathFinder.SearchPath(targetGridCoordinate);
            PlayerStartMove(pathsToTarget, interactWith);
        }

        private IEnumerator MovementRoutine(List<Vector3> targetPaths, IObjectInteractable targetInteraction = null)
        {
            // Initialize states
            IsWalking = true;

            while (targetPaths.Count > 0) {

                // Take and remove the first index of path way list
                Vector3 targetPos = targetPaths[0];
                targetPaths.RemoveAt(0);

                // Start move to target by procedure
                while (transform.position != targetPos)
                {
                    #if UNITY_EDITOR
                    Debug.DrawLine(transform.position, targetPos, Color.yellow);
                    for (int i = 0; i < targetPaths.Count; i++)
                    {
                        Vector3 dirDebug;
                        float distanceDebug;
                        if (i == 0)
                        {
                            Debug.DrawLine(targetPos, targetPaths[0], Color.yellow);
                            continue;
                        }

                        dirDebug = (targetPaths[i] - targetPaths[i - 1]).normalized;
                        distanceDebug = Vector3.Distance(targetPaths[i], targetPaths[i - 1]);
                        Debug.DrawRay(targetPaths[i - 1], dirDebug * distanceDebug, Color.yellow);
                    }
                    #endif

                    Vector3 dir = (targetPos - transform.position).normalized;
                    float distance = Vector3.Distance(transform.position, targetPos);
                    if (distance > 0)
                    {
                        Vector3 afterMove = transform.position + dir * walkSpeed * Time.deltaTime;
                        float distanceAfterMove = Vector3.Distance(afterMove, targetPos);
                        if (distanceAfterMove > distance)
                        {
                            afterMove = targetPos;
                        }

                        transform.position = afterMove;
                    }
                    yield return null;
                }
            }

            // Clean states
            moveProcessRoutine = null;
            IsWalking = false;

            //// Check player interaction
            //if (targetInteraction != null)
            //{
            //    OnPlayerObjectInteractEventArgs args = new OnPlayerObjectInteractEventArgs(playerEntity, targetInteraction);
            //    EventHandler.CallEvent(args);

            //    if (!args.IsCancelled)
            //    {
            //        targetInteraction.Interact();
            //    }
            //}
        }
    }
}