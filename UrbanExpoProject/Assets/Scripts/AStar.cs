using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanExpo
{
    public class AStar
    {
        private IslandGrid island;
        private System.Func<Vector3> getPlayerPositionFunc;

        /// <summary>
        /// Path finding A* algorithm
        /// </summary>
        public AStar(IslandGrid island, System.Func<Vector3> getPlayerPositionFunc)
        {
            this.island = island;
            this.getPlayerPositionFunc = getPlayerPositionFunc;
        }

        /// <summary>
        /// Search path in world grid position
        /// </summary>
        public List<Vector3> SearchPath(Vector3Int targetGridPos)
        {
            // If function to get player position not exists then return empty next paths
            if (getPlayerPositionFunc == null)
                return new List<Vector3>();

            // Define all information about player position and target position
            Vector2 startWorldPos = getPlayerPositionFunc();
            Vector3Int startGridCoord = island.WorldToGridPosition(startWorldPos);
            Vector3 targetWorldPos = island.GridToWorldPosition(targetGridPos) + island.WorldPivotOffset3D;
            float distanceStartEnd = Vector3.Distance(startWorldPos, targetWorldPos);

            // Check if player already on the target position
            if (startGridCoord == targetGridPos)
                return new List<Vector3>() { targetWorldPos };

            // Create root node
            Dictionary<string, AStarPathNode> registeredNodes = new Dictionary<string, AStarPathNode>();
            Dictionary<string, AStarPathNode> openNodes = new Dictionary<string, AStarPathNode>();
            AStarPathNode rootNode = new AStarPathNode(null, startGridCoord, distanceStartEnd, 0f);
            registeredNodes.Add($"({rootNode.GridCoordinate.x}, {rootNode.GridCoordinate.y})", rootNode);

            // Start path finding
            AStarPathNode pickedPath = rootNode;
            while (pickedPath.GridCoordinate != targetGridPos)
            {
                // Close the current picked path first
                pickedPath.isOpen = false;
                openNodes.Remove($"({pickedPath.GridCoordinate.x}, {pickedPath.GridCoordinate.y})");

                // Check each direction and add it into registered dictionary and open dictionary
                AStarPathNode upWayNode = AddRegisterNode(IslandGridNormalDirection.Up, pickedPath, targetWorldPos, ref registeredNodes);
                if (upWayNode != null)
                {
                    string gridAskey = $"({upWayNode.GridCoordinate.x}, {upWayNode.GridCoordinate.y})";
                    if (upWayNode.isOpen && !openNodes.ContainsKey(gridAskey))
                        openNodes.Add(gridAskey, upWayNode);
                }

                AStarPathNode downWayNode = AddRegisterNode(IslandGridNormalDirection.Down, pickedPath, targetWorldPos, ref registeredNodes);
                if (downWayNode != null)
                {
                    string gridAsKey = $"({downWayNode.GridCoordinate.x}, {downWayNode.GridCoordinate.y})";
                    if (downWayNode.isOpen && !openNodes.ContainsKey(gridAsKey))
                        openNodes.Add(gridAsKey, downWayNode);
                }

                AStarPathNode leftWayNode = AddRegisterNode(IslandGridNormalDirection.Left, pickedPath, targetWorldPos, ref registeredNodes);
                if (leftWayNode != null)
                {
                    string gridAsKey = $"({leftWayNode.GridCoordinate.x}, {leftWayNode.GridCoordinate.y})";
                    if (leftWayNode.isOpen && !openNodes.ContainsKey(gridAsKey))
                        openNodes.Add(gridAsKey, leftWayNode);
                }

                AStarPathNode rightWayNode = AddRegisterNode(IslandGridNormalDirection.Right, pickedPath, targetWorldPos, ref registeredNodes);
                if (rightWayNode != null)
                {
                    string gridAsKey = $"({rightWayNode.GridCoordinate.x}, {rightWayNode.GridCoordinate.y})";
                    if (rightWayNode.isOpen && !openNodes.ContainsKey(gridAsKey))
                        openNodes.Add(gridAsKey, rightWayNode);
                }

                AStarPathNode upperRightWayNode = AddRegisterNode(IslandGridNormalDirection.UpperRight, pickedPath, targetWorldPos, ref registeredNodes);
                if (upperRightWayNode != null)
                {
                    string gridAsKey = $"({upperRightWayNode.GridCoordinate.x}, {upperRightWayNode.GridCoordinate.y})";
                    if (upperRightWayNode.isOpen && !openNodes.ContainsKey(gridAsKey))
                        openNodes.Add(gridAsKey, upperRightWayNode);
                }

                AStarPathNode upperLeftWayNode = AddRegisterNode(IslandGridNormalDirection.UpperLeft, pickedPath, targetWorldPos, ref registeredNodes);
                if (upperLeftWayNode != null)
                {
                    string gridAsKey = $"({upperLeftWayNode.GridCoordinate.x}, {upperLeftWayNode.GridCoordinate.y})";
                    if (upperLeftWayNode.isOpen && !openNodes.ContainsKey(gridAsKey))
                        openNodes.Add(gridAsKey, upperLeftWayNode);
                }

                AStarPathNode lowerRightWayNode = AddRegisterNode(IslandGridNormalDirection.LowerRight, pickedPath, targetWorldPos, ref registeredNodes);
                if (lowerRightWayNode != null)
                {
                    string gridAsKey = $"({lowerRightWayNode.GridCoordinate.x}, {lowerRightWayNode.GridCoordinate.y})";
                    if (lowerRightWayNode.isOpen && !openNodes.ContainsKey(gridAsKey))
                        openNodes.Add(gridAsKey, lowerRightWayNode);
                }

                AStarPathNode lowerLeftWayNode = AddRegisterNode(IslandGridNormalDirection.LowerLeft, pickedPath, targetWorldPos, ref registeredNodes);
                if (lowerLeftWayNode != null)
                {
                    string gridAsKey = $"({lowerLeftWayNode.GridCoordinate.x}, {lowerLeftWayNode.GridCoordinate.y})";
                    if (lowerLeftWayNode.isOpen && !openNodes.ContainsKey(gridAsKey))
                        openNodes.Add(gridAsKey, lowerLeftWayNode);
                }

                // Compare the lowest cost to pick between open nodes
                AStarPathNode lowestCostNode = null;
                foreach (KeyValuePair<string, AStarPathNode> pair in openNodes)
                {
                    if (lowestCostNode == null)
                    {
                        lowestCostNode = pair.Value;
                        continue;
                    }

                    if (lowestCostNode.Cost > pair.Value.Cost)
                    {
                        lowestCostNode = pair.Value;
                    }
                }

                // Check path to target not exists, set target to start position itself
                if (lowestCostNode == null)
                {
                    pickedPath = rootNode;
                    break;
                }

                // Pick the lowest and change the current pick path
                pickedPath = lowestCostNode;
            }

            // Convert nodes into target positions, reverse process
            List<Vector3> targetPositions = new List<Vector3>() { targetWorldPos };
            IslandGridNormalDirection lastDirection = pickedPath.FromDirection;
            float timesBy = 1f;
            pickedPath = pickedPath.PreviousNode;
            while (pickedPath != null)
            {
                if (lastDirection != pickedPath.FromDirection)
                {
                    // Convert direction to target position
                    Vector2 dir = island.map.GetDirection(lastDirection);
                    targetPositions.Add(targetPositions[targetPositions.Count - 1] - new Vector3(dir.x, dir.y) * timesBy);

                    // Different direction
                    lastDirection = pickedPath.FromDirection;
                    timesBy = 1f;
                }
                else
                {
                    timesBy++;
                    if (pickedPath.PreviousNode == null)
                        targetPositions.Add(startWorldPos);
                }

                pickedPath = pickedPath.PreviousNode;
            }
            targetPositions.Reverse();

            return targetPositions;
        }

        private AStarPathNode AddRegisterNode(IslandGridNormalDirection gridDir, AStarPathNode current, Vector3 targetWorldPos,
            ref Dictionary<string, AStarPathNode> registeredNode)
        {
            // Chedk tile exists
            Vector3Int gridAfterMove = current.GridCoordinate + island.map.GetGridDirection(gridDir);
            if (island.FloorTilemap.GetTile(gridAfterMove) == null)
                return null;

            // Check diagonal movement valid
            bool isMovingDiagonal = gridDir.IsDiagonalDirection();
            if (isMovingDiagonal)
            {
                IslandGridNormalDirection firstDir = IslandGridNormalDirection.Zero, secondDir = IslandGridNormalDirection.Zero;
                switch (gridDir)
                {
                    case IslandGridNormalDirection.UpperRight:
                        firstDir = IslandGridNormalDirection.Up;
                        secondDir = IslandGridNormalDirection.Right;
                        break;

                    case IslandGridNormalDirection.UpperLeft:
                        firstDir = IslandGridNormalDirection.Up;
                        secondDir = IslandGridNormalDirection.Left;
                        break;

                    case IslandGridNormalDirection.LowerRight:
                        firstDir = IslandGridNormalDirection.Down;
                        secondDir = IslandGridNormalDirection.Right;
                        break;

                    case IslandGridNormalDirection.LowerLeft:
                        firstDir = IslandGridNormalDirection.Down;
                        secondDir = IslandGridNormalDirection.Left;
                        break;
                }

                Vector3Int firstGridAfterMove = current.GridCoordinate + island.map.GetGridDirection(firstDir);
                Vector3Int secondGridAfterMove = current.GridCoordinate + island.map.GetGridDirection(secondDir);
                if (island.FloorTilemap.GetTile(firstGridAfterMove) == null || island.FloorTilemap.GetTile(secondGridAfterMove) == null)
                    return null;
            }

            // Check node already registered
            string gridAsKey = $"({gridAfterMove.x}, {gridAfterMove.y})";
            if (registeredNode.ContainsKey(gridAsKey))
                return registeredNode[gridAsKey];

            // Create new node
            Vector3 worldPosAfterMove = island.GridToWorldPosition(gridAfterMove) + island.WorldPivotOffset3D;
            float distanceAfterMove = Vector3.Distance(targetWorldPos, worldPosAfterMove);
            AStarPathNode node = new AStarPathNode(current, gridAfterMove, distanceAfterMove, current.MoveDistance + (isMovingDiagonal ? Mathf.Sqrt(2f) : 1f));
            registeredNode.Add(gridAsKey, node);
            node.FromDirection = gridDir;
            return node;
        }
    }

    /// <summary>
    /// Path node to implement A* Algorithm for path finding problem
    /// </summary>
    public class AStarPathNode
    {
        private AStarPathNode fromNode;

        private Vector3Int gridCoord;
        private IslandGridNormalDirection fromDirection = IslandGridNormalDirection.Zero;
        private float targetDistance;
        private float moveDistance;
        public bool isOpen = true;

        #region Properties
        public AStarPathNode PreviousNode
        {
            set => fromNode = value;
            get => fromNode;
        }
        public float MoveDistance => moveDistance;
        public Vector3Int GridCoordinate => gridCoord;
        public IslandGridNormalDirection FromDirection
        {
            set => fromDirection = value;
            get => fromDirection;
        }
        public float Cost => targetDistance + moveDistance;
        #endregion

        public AStarPathNode(AStarPathNode fromNode, Vector3Int gridCoord, float targetDistance, float moveDistance)
        {
            this.fromNode = fromNode;
            this.gridCoord = gridCoord;
            this.targetDistance = targetDistance;
            this.moveDistance = moveDistance;
        }
    }
}