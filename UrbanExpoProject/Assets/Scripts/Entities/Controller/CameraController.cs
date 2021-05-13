using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera virtualCamera = null;

        [Header("Camera Attributes")]
        [SerializeField, MinMaxSlider(4f, 15f)] private Vector2 cameraSizeRange = new Vector2(5f, 10f);
        [SerializeField] private float cameraScalingSpeed = 1f;

        public void Zoom(float zoomValue)
        {
            float currentSize = virtualCamera.m_Lens.OrthographicSize;
            currentSize -= zoomValue * cameraScalingSpeed;
            currentSize = currentSize < cameraSizeRange.x ? cameraSizeRange.x : currentSize > cameraSizeRange.y ? cameraSizeRange.y : currentSize;
            virtualCamera.m_Lens.OrthographicSize = currentSize;
        }
    }

}