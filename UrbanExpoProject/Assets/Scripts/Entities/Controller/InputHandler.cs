using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class InputHandler : MonoBehaviour
    {
        [Header("Control Map")]
        [SerializeField]
        private InputMap inputKeyMap = new InputMap
        {
            ClickKey = KeyCode.Mouse0,
            OpenSettingsKey = KeyCode.Escape
        };

        [Header("Additional Data Requirement")]
        [SerializeField] private LayerMask interactableLayer = ~0;

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private InputData data = new InputData();
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private int lastTouchCount = 0;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isTouchEnabled = true;

        #region Properties
        public InputMap Map
        {
            set => inputKeyMap = value;
            get => inputKeyMap;
        }
        public InputData Data => data;
        #endregion

        #region Unity BuiltIn Methods
        private void OnEnable()
        {
            data.ResetInputData();
        }

        private void Update()
        {
            // Prioritize touch control
            if (Input.touchCount > 0 && isTouchEnabled)
            {
                HandleTouchInput();
                return;
            }

            // Other inputs if there's currently no touch input
            if (lastTouchCount > 0 && Input.touchCount == 0) lastTouchCount = 0;
            HandlePCInput();
        }
        #endregion

        private void HandleTouchInput()
        {
            // Check new touch input or touch released
            bool touchClick = false, touchReleased = false;
            if (lastTouchCount != Input.touchCount)
            {
                touchClick = Input.touchCount > lastTouchCount;
                touchReleased = Input.touchCount < lastTouchCount;
                lastTouchCount = Input.touchCount;
            }
            if (touchClick && lastTouchCount == 1)
            {
                data.click = touchClick;
            }

            // Check each touch to be translate
            Touch[] touches = Input.touches;
            for (int i = 0; i < touches.Length; i++)
            {
                // Get finger touch by index
                Touch touch = touches[i];

                // Check priority touch
                switch (i)
                {
                    case 0: // Primary touch
                        data.mousePosition = touch.position;
                        data.mousePosInWorld = Camera.main.ScreenToWorldPoint(data.mousePosition);
                        break;

                    case 1: // Secondary touch
                        break;
                }
            }
        }

        private void HandlePCInput()
        {
            // Insert inputs
            data.click = Input.GetKeyDown(inputKeyMap.ClickKey);
            data.openSettings = Input.GetKeyDown(inputKeyMap.OpenSettingsKey);
            data.zoomingControlValue = Input.GetAxis("Mouse ScrollWheel");
            data.mousePosition = Input.mousePosition;
            data.mousePosInWorld = Camera.main.ScreenToWorldPoint(data.mousePosition);

            // Detect object raycast
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit2D hit = Physics2D.Raycast(data.mousePosInWorld, Vector2.zero, 0f, interactableLayer);
                data.objOnMousePosition = hit.collider == null ? null : hit.collider.gameObject;
            }
            else
            {
                data.objOnMousePosition = null;
            }
        }

        public void SetTouchEnable(bool isTouchEnabled)
        {
            this.isTouchEnabled = isTouchEnabled;
        }
    }

    [System.Serializable]
    public struct InputMap
    {
        [SerializeField] private KeyCode clickKey;
        [SerializeField] private KeyCode openSettingsKey;

        public KeyCode ClickKey
        {
            set => clickKey = value;
            get => clickKey;
        }

        public KeyCode OpenSettingsKey
        {
            set => openSettingsKey = value;
            get => openSettingsKey;
        }
    }

    [System.Serializable]
    public class InputData : ITemporaryData
    {
        public bool click;
        public bool openSettings;
        public float zoomingControlValue;
        public Vector2 mousePosition;
        public Vector3 mousePosInWorld;
        public GameObject objOnMousePosition;

        public void ResetInputData()
        {
            click = false;
            openSettings = false;
            zoomingControlValue = 0f;
            mousePosition = Vector2.zero;
            mousePosInWorld = Vector3.zero;
            objOnMousePosition = null;
        }
    }
}