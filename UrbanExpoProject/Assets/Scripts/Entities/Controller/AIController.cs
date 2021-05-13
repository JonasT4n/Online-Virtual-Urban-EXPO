using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class AIController : Controller
    {
        [Header("Controller Data Attributes")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float idleTime = 5f;

        [SerializeField] private bool isStaticMovement = false;
        [SerializeField, ShowIf("IsRandomMovement")] private float maxRange = 4f;
        [InfoBox("Make sure it is filled with world position data, otherwise it will automatically make AI movement random")]
        [SerializeField, ShowIf("isStaticMovement")] private Vector3[] staticWorldPosition = null; 

        private AStar pathFinder;
        private IEnumerator moveProcessRoutine;
        private int iterationIndex = 0;

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float tempCurrentIdleTime;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector3 hookedPosition;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector3Int targetGridCoordinate;

        #region Properties
        public bool IsRandomMovement => !isStaticMovement;
        #endregion

        #region Unity BuiltIn Methods
        protected override void Start()
        {
            base.Start();

            // Fix initial attribute values
            pathFinder = new AStar(IslandGrid.singleton, () => transform.position);
            if (maxRange < 0f) maxRange = -maxRange;

            // Initialize temporary values
            hookedPosition = transform.position;
            targetGridCoordinate = IslandGrid.singleton.WorldToGridPosition(transform.position);
            tempCurrentIdleTime = idleTime;

            // Check empty static data
            if (isStaticMovement && staticWorldPosition.Length == 0) isStaticMovement = false;
        }

        protected override void FixedUpdate()
        {
            HandleAIBrain();

#if UNITY_EDITOR
            // Debugging static movement
            if (!isStaticMovement) return;
            for (int i = 0; i < staticWorldPosition.Length; i++)
            {
                Vector3Int gPos = IslandGrid.singleton.WorldToGridPosition(staticWorldPosition[i]);
                IslandGrid.singleton.DrawSquareGrid(gPos);
            }
#endif

            base.FixedUpdate();
        }
        #endregion

        private void HandleAIBrain()
        {
            if (!IsWalking)
            {
                tempCurrentIdleTime -= Time.deltaTime;
                if (tempCurrentIdleTime <= 0)
                {
                    // Check static movement or random movement
                    Vector3Int gridTarget;
                    if ((isStaticMovement && staticWorldPosition.Length == 0) || !isStaticMovement)
                    {
                        gridTarget = RandomGridTarget();
                    }
                    else
                    {
                        gridTarget = IslandGrid.singleton.WorldToGridPosition(staticWorldPosition[iterationIndex]);
                        iterationIndex = iterationIndex + 1 >= staticWorldPosition.Length ? 0 : iterationIndex + 1;
                    }

                    if (!IslandGrid.singleton.IsFloorTileExists(gridTarget))
                        return;

                    MoveToGridPosition(gridTarget);
                    tempCurrentIdleTime = idleTime;
                }
            }
        }

        private Vector3Int RandomGridTarget()
        {
            Vector3 randomVect = hookedPosition + Random.onUnitSphere * Random.Range(0f, maxRange + 1);
            randomVect.z = 0f;
            return IslandGrid.singleton.WorldToGridPosition(randomVect);
        }

        public override void MoveToGridPosition(Vector3Int targetGridCoordinate)
        {
            this.targetGridCoordinate = targetGridCoordinate;
            List<Vector3> pathsToTarget = pathFinder.SearchPath(targetGridCoordinate);
            AIStartMove(pathsToTarget);
        }

        public override void StopMoveControl()
        {
            if (moveProcessRoutine != null) StopCoroutine(moveProcessRoutine);
            IsWalking = false;
            targetGridCoordinate = IslandGrid.singleton.WorldToGridPosition(transform.position);
            AIStartMove(new List<Vector3>() { targetGridCoordinate });
        }

        private void AIStartMove(List<Vector3> paths)
        {
            if (moveProcessRoutine != null) StopCoroutine(moveProcessRoutine);

            moveProcessRoutine = MovementRoutine(paths);
            StartCoroutine(moveProcessRoutine);
        }

        private IEnumerator MovementRoutine(List<Vector3> targetPaths)
        {
            // Initialize states
            IsWalking = true;

            while (targetPaths.Count > 0)
            {

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