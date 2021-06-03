using System;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanExpo
{
    [Serializable]
    public class IslandArea
    {
        public string areaName = string.Empty;
        [SerializeField] private Vector3Int minGridPoint = Vector3Int.zero;
        [SerializeField] private Vector3Int maxGridPoint = Vector3Int.zero;

        public IslandArea(string areaName, Vector3Int pivotGridPosition, int radius)
        {
            this.areaName = areaName;
            minGridPoint = pivotGridPosition - Vector3Int.one * Mathf.FloorToInt(radius / 2f);
            maxGridPoint = pivotGridPosition + Vector3Int.one * Mathf.CeilToInt(radius / 2f);
        }

        public IslandArea(string areaName, Vector3Int pivotGridPosition, int radX, int radY, int radZ = 0)
        {
            this.areaName = areaName;
            int xMin = -Mathf.FloorToInt(radX / 2f), yMin = -Mathf.FloorToInt(radY / 2f), zMin = -Mathf.FloorToInt(radZ / 2f);
            int xMax = Mathf.CeilToInt(radX / 2f), yMax = Mathf.CeilToInt(radY / 2f), zMax = Mathf.CeilToInt(radZ / 2f);
            minGridPoint = pivotGridPosition + new Vector3Int(xMin, yMin, zMin);
            maxGridPoint = pivotGridPosition + new Vector3Int(xMax, yMax, zMax);
        }

        public IslandArea(string areaName, Vector3Int minGridPoint, Vector3Int maxGridPoint)
        {
            this.areaName = areaName;
            this.minGridPoint = minGridPoint;
            this.maxGridPoint = maxGridPoint;
            FixMinMaxPoint();
        }

        public bool IsCoordinateInArea(Vector3Int gridCoord)
        {
            return gridCoord.x >= minGridPoint.x && gridCoord.x <= maxGridPoint.x &&
                gridCoord.y >= minGridPoint.y && gridCoord.y <= maxGridPoint.y &&
                gridCoord.z >= minGridPoint.z && gridCoord.z <= maxGridPoint.z;
        }

        public void FixMinMaxPoint()
        {
            // Temporary variables
            Vector3Int min = minGridPoint;
            Vector3Int max = maxGridPoint;

            // Check X Axis
            if (min.x > max.x)
            {
                min.x = min.x + max.x;
                max.x = min.x - max.x;
                min.x = min.x - max.x;
            }

            // Check Y Axis
            if (min.y > max.y)
            {
                min.y = min.y + max.y;
                max.y = min.y - max.y;
                min.y = min.y - max.y;
            }

            // Check Z Axis
            if (min.z > max.z)
            {
                min.z = min.z + max.z;
                max.z = min.z - max.z;
                min.z = min.z - max.z;
            }

            // Assign back to origin
            minGridPoint = min;
            maxGridPoint = max;
        }

        #region Debugger
        public void VisualizeAreaDebug(IslandGridMap map)
        {
#if UNITY_EDITOR
            int xDistance = maxGridPoint.x - minGridPoint.x + 1, yDistance = maxGridPoint.y - minGridPoint.y + 1;
            Vector3 startMinPos = IslandGrid.singleton.GridToWorldPosition(minGridPoint);
            Vector3 rightWay = map.GetDirection(IslandGridNormalDirection.Right) * xDistance;
            Vector3 upWay = map.GetDirection(IslandGridNormalDirection.Up) * yDistance;
            Debug.DrawLine(startMinPos, startMinPos + rightWay, Color.magenta); // Up to Left in World
            Debug.DrawLine(startMinPos, startMinPos + upWay, Color.magenta); // Left to Down
            Debug.DrawLine(startMinPos + rightWay, startMinPos + rightWay + upWay, Color.magenta); // Up to Right
            Debug.DrawLine(startMinPos + upWay, startMinPos + rightWay + upWay, Color.magenta); // Down to Right
#endif
        }
        #endregion
    }

}