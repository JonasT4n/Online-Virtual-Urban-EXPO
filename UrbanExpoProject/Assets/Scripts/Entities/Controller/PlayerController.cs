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

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector3Int targetGridCoordinate;

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

            // Check enter and leave the UI configuration settings menu
            if (inputHandler.Data.openSettings)
                GameManager.singleton.uiGameManager.IsConfiguring = !GameManager.singleton.uiGameManager.IsConfiguring;

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
            // Initialize starting information
            IslandGrid island = IslandGrid.singleton;
            inputHandler.Data.mousePosInWorld.z = 0;
            Vector3Int gridCoordinate = island.WorldToGridPosition(inputHandler.Data.mousePosInWorld);

            //Debug.Log($"Mouse pos in world: {inputHandler.Data.mousePosInWorld}; Grid coordinate: {gridCoordinate}");

            // Movement selection control
            bool floorTileExists = false;
            if (island.FloorTilemap.gameObject.Equals(inputHandler.Data.objOnMousePosition))
            {
                floorTileExists = island.IsFloorTileExists(gridCoordinate);
                if (floorTileExists && inputHandler.Data.click)
                    MoveToGridPosition(gridCoordinate);
            }

            // Local camera control
            GameManager.singleton.CameraController.Zoom(inputHandler.Data.zoomingControlValue);

#if UNITY_EDITOR
            IslandGrid.singleton.DrawSquareGrid(gridCoordinate);
#endif
        }

        public override void MoveToGridPosition(Vector3Int targetGridCoordinate)
        {
            this.targetGridCoordinate = targetGridCoordinate;
            List<Vector3> pathsToTarget = pathFinder.SearchPath(targetGridCoordinate);
            PlayerStartMove(pathsToTarget);
        }

        public override void StopMoveControl()
        {
            if (moveProcessRoutine != null) StopCoroutine(moveProcessRoutine);
            IsWalking = false;
            targetGridCoordinate = IslandGrid.singleton.WorldToGridPosition(transform.position);
            PlayerStartMove(new List<Vector3>() { targetGridCoordinate });
        }

        private void PlayerStartMove(List<Vector3> paths)
        {
            if (moveProcessRoutine != null) StopCoroutine(moveProcessRoutine);

            moveProcessRoutine = MovementRoutine(paths);
            StartCoroutine(moveProcessRoutine);
        }

        private IEnumerator MovementRoutine(List<Vector3> targetPaths)
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
        }
    }
}