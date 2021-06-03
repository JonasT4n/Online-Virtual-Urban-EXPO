using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class IslandGrid : MonoBehaviour, IEntitySpawner
    {
        public static IslandGrid singleton;

        [Header("Island Settings")]
        [Required("Due to using grid system, this must be filled")] 
        public Grid grid2D = null;
        [Required("Floor Tilemap with collider for player to detect movement control")]
        [SerializeField] private Tilemap tileFloorWithCollider = null;
        [SerializeField] private Vector3 initialPlayerSpawnPosition = Vector3.zero;
        [SerializeField] private Vector2 worldOffsetOnGrid = Vector2.zero;

        [Header("Area & Buildings")]
        [SerializeField] private IslandArea[] islandAreas = null;
        [SerializeField] private SpawnPoint[] spawnPoints = null;

        public IslandGridMap map;

        #region Properties
        public Tilemap FloorTilemap => tileFloorWithCollider;
        public Vector3 PlayerSpawnPosition => initialPlayerSpawnPosition;

        /// <summary>
        /// From world position zero to Grid position Coordinate
        /// </summary>
        public Vector2Int GridPivotOffset
        {
            get
            {
                Vector3Int inGridoffset = WorldToGridPosition(Vector3.zero);
                return new Vector2Int(inGridoffset.x, inGridoffset.y);
            }
        }

        /// <summary>
        /// From grid position coordinate zero to world position
        /// </summary>
        public Vector2 WorldPivotOffset2D
        {
            get
            {
                Vector3 inWorldPos = GridToWorldPosition(Vector3Int.zero);
                return new Vector2(inWorldPos.x, inWorldPos.y);
            }
        }

        /// <summary>
        /// From grid position coordinate zero to world position
        /// </summary>
        public Vector3 WorldPivotOffset3D => new Vector3(WorldPivotOffset2D.x, WorldPivotOffset2D.y);
        #endregion

        #region Unity BuiltIn Methods
        private void Awake()
        {
            if (singleton != null)
            {
                Debug.Log($"Automatically deleted extra object of type {singleton.GetType().Name}: {name}");
                Destroy(this);
                return;
            }

            singleton = this;
            map = new IslandGridMap(this);

            // Fix min max of each points
            foreach (IslandArea area in islandAreas)
            {
                area.FixMinMaxPoint();
            }

            //Debug.Log($"Grid offset from world pos zero: {GridPivotOffset}; World Pivot Offset: {WorldPivotOffset}\n" +
            //    $"Actual result in World Pos: {GridToWorldPosition(GridPivotOffset)};" +
            //    $"\nCenter: {map.WorldCenter}");

            //Vector3 pos = new Vector3(-3.1f, -0.55f, 0f);
            //Vector3Int gridPos = WorldToGridPosition(pos);
            //Vector3 lastPos = GridToWorldPosition(gridPos) + new Vector3(WorldPivotOffset.x, WorldPivotOffset.y, 0f);
            //Debug.Log($"Sample pos: {pos}; In Grid Pos: {gridPos}; Last Pos after grid: {lastPos}");
        }

        private void Update()
        {
            foreach (IslandArea area in islandAreas)
            {
                area.VisualizeAreaDebug(map);
            }
        }

        private void OnDestroy()
        {
            singleton = null;
        }
        #endregion

        #region Debugger
        public void DrawSquareGrid(Vector3Int gridCoordinate)
        {
#if UNITY_EDITOR
            Color detectColor = IsFloorTileExists(gridCoordinate) ? Color.blue : Color.red;
            Vector3 gridOffset = GridToWorldPosition(new Vector3Int(GridPivotOffset.x, GridPivotOffset.y, 0));
            Vector3 worldPos = GridToWorldPosition(gridCoordinate) - gridOffset;
            Debug.DrawLine(new Vector3(worldPos.x - 0.5f, worldPos.y), new Vector3(worldPos.x, worldPos.y + 0.25f), detectColor); // Left to Up
            Debug.DrawLine(new Vector3(worldPos.x - 0.5f, worldPos.y), new Vector3(worldPos.x, worldPos.y - 0.25f), detectColor); // Left to Down
            Debug.DrawLine(new Vector3(worldPos.x, worldPos.y + 0.25f), new Vector3(worldPos.x + 0.5f, worldPos.y), detectColor); // Up to Right
            Debug.DrawLine(new Vector3(worldPos.x, worldPos.y - 0.25f), new Vector3(worldPos.x + 0.5f, worldPos.y), detectColor); // Down to Right
#endif
        }
        #endregion

        public IslandArea GetAreaByGridPosition(Vector3Int gridPos)
        {
            foreach (IslandArea area in islandAreas)
            {
                if (area.IsCoordinateInArea(gridPos)) return area;
            }

            return null;
        }

        public LivingEntity Spawn(LivingEntity entityPrefab)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            return spawnPoints[spawnIndex].Spawn(entityPrefab);
        }

        public bool IsFloorTileExists(Vector3Int gridCoordinate)
        {
            return tileFloorWithCollider.GetTile(gridCoordinate) != null;
        }

        public Vector3Int WorldToGridPosition(Vector2 worldPos)
        {
            Vector3 offset = new Vector3(worldOffsetOnGrid.x, worldOffsetOnGrid.y, 0f);
            return grid2D.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0f) + offset);
        }

        public Vector3Int WorldToGridPosition(Vector3 worldPos)
        {
            Vector3 offset = new Vector3(worldOffsetOnGrid.x, worldOffsetOnGrid.y, 0f);
            return grid2D.WorldToCell(worldPos + offset);
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            Vector3 offset = new Vector3(worldOffsetOnGrid.x, worldOffsetOnGrid.y, 0f);
            return grid2D.CellToWorld(new Vector3Int(gridPos.x, gridPos.y, 0)) - offset;
        }

        public Vector3 GridToWorldPosition(Vector3Int gridPos)
        {
            Vector3 offset = new Vector3(worldOffsetOnGrid.x, worldOffsetOnGrid.y, 0f);
            return grid2D.CellToWorld(gridPos) - offset;
        }

        #region Static Methods & Utilities
        public static float ModulusFloatingPoint(float a, float b)
        {
            float aAbs = a < 0 ? -a : a;
            float bAbs = b < 0 ? -b : b;

            while (aAbs > bAbs)
            {
                aAbs -= bAbs;
            }

            return aAbs < 0 ? -aAbs : aAbs;
        }

        public static IObjectInteractable GetInteractable(GameObject obj)
        {
            if (obj.GetComponent<InteractableBuilding>()) return obj.GetComponent<InteractableBuilding>();
            else if (obj.GetComponent<NPCEntity>()) return obj.GetComponent<NPCEntity>();
            return null;
        }
        #endregion
    }

    public static class IslandGridToolExtension
    {
        public static IslandGridNormalDirection GetOpposite(this IslandGridNormalDirection direction)
        {
            switch (direction)
            {
                case IslandGridNormalDirection.Up:
                    return IslandGridNormalDirection.Down;

                case IslandGridNormalDirection.Down:
                    return IslandGridNormalDirection.Up;

                case IslandGridNormalDirection.Right:
                    return IslandGridNormalDirection.Left;

                case IslandGridNormalDirection.Left:
                    return IslandGridNormalDirection.Right;

                case IslandGridNormalDirection.UpperRight:
                    return IslandGridNormalDirection.LowerLeft;

                case IslandGridNormalDirection.UpperLeft:
                    return IslandGridNormalDirection.LowerRight;

                case IslandGridNormalDirection.LowerRight:
                    return IslandGridNormalDirection.UpperLeft;

                case IslandGridNormalDirection.LowerLeft:
                    return IslandGridNormalDirection.UpperRight;

                default:
                    return IslandGridNormalDirection.Zero;
            }
        }

        public static bool IsDiagonalDirection(this IslandGridNormalDirection direction)
        {
            switch (direction)
            {
                case IslandGridNormalDirection.Zero:
                case IslandGridNormalDirection.Down:
                case IslandGridNormalDirection.Left:
                case IslandGridNormalDirection.Right:
                case IslandGridNormalDirection.Up:
                    return false;

                default:
                    return true;
            }
        }
    }

    public enum IslandGridNormalDirection { Zero = 0, Up, Down, Right, Left, UpperRight, UpperLeft, LowerRight, LowerLeft }

    public class IslandGridMap
    {
        private static readonly Vector3Int GRID_RIGHT_DIRECTION = new Vector3Int(1, 0, 0);
        private static readonly Vector3Int GRID_LEFT_DIRECTION = new Vector3Int(-1, 0, 0);
        private static readonly Vector3Int GRID_UP_DIRECTION = new Vector3Int(0, 1, 0);
        private static readonly Vector3Int GRID_DOWN_DIRECTION = new Vector3Int(0, -1, 0);
        private static readonly Vector3Int GRID_FRONT_DIRECTION = new Vector3Int(0, 0, 1);
        private static readonly Vector3Int GRID_BACK_DIRECTION = new Vector3Int(0, 0, -1);

        public IslandGrid world;

        private Vector2 centerWorld; // World center

        private Vector2 rightDir; // Right
        private Vector2 upDir; // Up
        private Vector2 leftDir; // Left
        private Vector2 downDir; // Down

        private Vector2 upRightDir; // Upper Right
        private Vector2 upLeftDir; // Upper Left
        private Vector2 downRightDir; // Lower Right
        private Vector2 downLeftDir; // Lower Left

        #region Properties
        public Vector2 WorldCenter2D => centerWorld;
        public Vector3 WorldCenter3D => centerWorld;
        #endregion

        public IslandGridMap(IslandGrid world)
        {
            this.world = world;

            Vector3 pivot = world.GridToWorldPosition(Vector3Int.zero);
            centerWorld = new Vector2(pivot.x, pivot.y);

            rightDir = world.GridToWorldPosition(GRID_RIGHT_DIRECTION) - pivot;
            upDir = world.GridToWorldPosition(GRID_UP_DIRECTION) - pivot;
            leftDir = world.GridToWorldPosition(GRID_LEFT_DIRECTION) - pivot;
            downDir = world.GridToWorldPosition(GRID_DOWN_DIRECTION) - pivot;

            upRightDir = world.GridToWorldPosition(GRID_UP_DIRECTION + GRID_RIGHT_DIRECTION) - pivot;
            upLeftDir = world.GridToWorldPosition(GRID_UP_DIRECTION + GRID_LEFT_DIRECTION) - pivot;
            downRightDir = world.GridToWorldPosition(GRID_DOWN_DIRECTION + GRID_RIGHT_DIRECTION) - pivot;
            downLeftDir = world.GridToWorldPosition(GRID_DOWN_DIRECTION + GRID_LEFT_DIRECTION) - pivot;
        }

        public void PrintDirectionInformation()
        {
#if UNITY_EDITOR
            Debug.Log("World Mapping information (Direction in Grid): \n" +
                $"{IslandGridNormalDirection.Right} = {rightDir}\n" +
                $"{IslandGridNormalDirection.Up} = {upDir}\n" +
                $"{IslandGridNormalDirection.Left} = {leftDir}\n" +
                $"{IslandGridNormalDirection.Down} = {downDir}\n" +
                $"{IslandGridNormalDirection.UpperRight} = {upRightDir}\n" +
                $"{IslandGridNormalDirection.UpperLeft} = {upLeftDir}\n" +
                $"{IslandGridNormalDirection.LowerRight} = {downRightDir}\n" +
                $"{IslandGridNormalDirection.LowerLeft} = {downLeftDir}");
#endif
        }

        public Vector3Int GetGridDirection(IslandGridNormalDirection direction)
        {
            switch (direction)
            {
                case IslandGridNormalDirection.Up:
                    return GRID_UP_DIRECTION;

                case IslandGridNormalDirection.Down:
                    return GRID_DOWN_DIRECTION;

                case IslandGridNormalDirection.Left:
                    return GRID_LEFT_DIRECTION;

                case IslandGridNormalDirection.Right:
                    return GRID_RIGHT_DIRECTION;

                case IslandGridNormalDirection.UpperRight:
                    return GRID_UP_DIRECTION + GRID_RIGHT_DIRECTION;

                case IslandGridNormalDirection.UpperLeft:
                    return GRID_UP_DIRECTION + GRID_LEFT_DIRECTION;

                case IslandGridNormalDirection.LowerRight:
                    return GRID_DOWN_DIRECTION + GRID_RIGHT_DIRECTION;

                case IslandGridNormalDirection.LowerLeft:
                    return GRID_DOWN_DIRECTION + GRID_LEFT_DIRECTION;

                default:
                    return Vector3Int.zero;
            }
        }

        public Vector2 GetDirection(IslandGridNormalDirection direction)
        {
            switch (direction)
            {
                case IslandGridNormalDirection.Up:
                    return upDir;

                case IslandGridNormalDirection.Down:
                    return downDir;

                case IslandGridNormalDirection.Left:
                    return leftDir;

                case IslandGridNormalDirection.Right:
                    return rightDir;

                case IslandGridNormalDirection.UpperRight:
                    return upRightDir;

                case IslandGridNormalDirection.UpperLeft:
                    return upLeftDir;

                case IslandGridNormalDirection.LowerRight:
                    return downRightDir;

                case IslandGridNormalDirection.LowerLeft:
                    return downLeftDir;

                default:
                    return Vector2.zero;
            }
        }
    }

}