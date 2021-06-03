using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class UIGameManager : UIManager
    {
        // Additional UI static events
        public static event System.Action OnGamePause;
        public static event System.Action OnGameResume;

        [Header("Additional UI in Game")]
        [SerializeField] private RectTransform optionSettingPanel = null;
        [SerializeField] private RectTransform gamePanel = null;

        [Header("Button Settings")]
        [SerializeField] private BindingButton[] bindingButtons = null;

        [Header("Others")]
        [InfoBox("This content template used for loading data from any interaction calls")]
        [SerializeField] private UIInteractionContent uiContentTemplate = null;

        [BoxGroup("DEBUG"), SerializeField] private UIGameProfiler profiler = null;
        [Header("Private Debugger Information")]
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Button recentButtonHit = null;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isKeyBinding = false;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isPause = false;

        #region Properties
        public bool IsConfiguring
        {
            set
            {
                //optionSettingPanel.gameObject.SetActive(value);
                //gamePanel.gameObject.SetActive(!value);
                if (value == isPause) return;

                isPause = value;
                PauseGame(value);
            }
            get => isPause; // optionSettingPanel.gameObject.activeSelf;
        }
        #endregion

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Subscribe events
            EventHandler.OnEntityWalkedIntoGridEvent += HandleDisplayCurrentGrid;
            EventHandler.OnEntityEnterAreaEvent += HandleDisplayCurrentArea;
        }

        private void Start()
        {
            if (optionSettingPanel != null) optionSettingPanel.gameObject.SetActive(false);
            if (gamePanel != null) gamePanel.gameObject.SetActive(true);
        }

        private void Update()
        {
            // Check enter and leave the UI configuration settings menu, or the other definition pause the game.
            if (inputHandler.Data.openSettings && !isKeyBinding) IsConfiguring = !IsConfiguring;

            // UI debugger update 
            if (profiler.gameObject.activeSelf)
            {
                Vector3 mouseWorldPos = inputHandler.Data.mousePosInWorld;
                mouseWorldPos.z = 0f;
                profiler.txtMouseGridInfo.text = $"Mouse On Grid Position {IslandGrid.singleton.WorldToGridPosition(mouseWorldPos)}";

                bool highlightInteract = false;
                if (inputHandler.Data.objOnMousePosition != null)
                {
                    IObjectInteractable interact = IslandGrid.GetInteractable(inputHandler.Data.objOnMousePosition);
                    highlightInteract = interact != null ? true : false;
                }
                profiler.txtInteractionHighlightInfo.text = $"Interaction Highlight ({highlightInteract})";
            }

            // Key binding settings when its active
            if (isKeyBinding && Input.anyKeyDown)
            {
                // Set new key bind
                BindingButton bindButton = GetMatchBindingButton(recentButtonHit);
                SetKeyBinding(bindButton);

                // Exit key binding state
                SetInteractable(true);
                isKeyBinding = false;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe events
            EventHandler.OnEntityWalkedIntoGridEvent -= HandleDisplayCurrentGrid;
            EventHandler.OnEntityEnterAreaEvent -= HandleDisplayCurrentArea;
        }
        #endregion

        #region Event Methods
        private void HandleDisplayCurrentGrid(OnEntityWalkedIntoGridEventArgs args)
        {
            if (args.Entity is PlayerEntity)
            {
                PlayerEntity player = (PlayerEntity)args.Entity;
                if (player.tag == "Main Player")
                {
                    profiler.txtPlayerGridInfo.text = $"Main Player Grid Position {args.CurrentGridPosition}";
                }
            }
        }

        private void HandleDisplayCurrentArea(OnEntityEnterAreaEventArgs args)
        {
            if (args.Entity is PlayerEntity)
            {
                PlayerEntity player = (PlayerEntity)args.Entity;
                if (player.tag == "Main Player")
                {
                    string n = args.Area == null ? "None" : args.Area.areaName;
                    profiler.txtPlayerAreaInfo.text = $"Main Player In Area ({n})";
                }
            }
        }
        #endregion

        private void PauseGame(bool pause)
        {
            if (pause) OnGamePause?.Invoke();
            else OnGameResume?.Invoke();
        }

        public void BindButtonHit(Button hit)
        {
            // Set and Get UI manager informations
            recentButtonHit = hit;
            BindingButton bindButton = GetMatchBindingButton(recentButtonHit);
            isKeyBinding = bindButton.Button == null ? false : true;

            // Check if no button reference
            if (isKeyBinding)
            {
                // Disable all UI interaction while key binding
                SetInteractable(false);
            }
        }

        public void SetActiveIContent(bool active)
        {
            uiContentTemplate.ActivateIContent(active);
        }

        private void SetKeyBinding(BindingButton bindButton)
        {
            // Change key map
            InputMap map = inputHandler.Map;
            switch (bindButton.Key)
            {
                case "Click Select":
                    map.ClickKey = DetectKeyDown();
                    break;

                case "Open Setting":
                    map.OpenSettingsKey = DetectKeyDown();
                    break;
            }
            inputHandler.Map = map;

            // Change hint
            TextMeshProUGUI textMesh = bindButton.KeyHintText;
            if (textMesh != null) textMesh.text = $"{map.ClickKey}";
        }

        public override void SetInteractable(bool active)
        {
            base.SetInteractable(active);

            // Activate interaction on binding buttons
            foreach (BindingButton b in bindingButtons)
            {
                if (b.Button != null) b.Button.interactable = active;
            }
        }

        private BindingButton GetMatchBindingButton(Button button)
        {
            // Check button is null
            if (button == null) return new BindingButton();

            foreach (BindingButton b in bindingButtons)
            {
                if (b.Button == null) continue;
                if (b.Button.Equals(button)) return b;
            }
            return new BindingButton();
        }
    }

    [System.Serializable]
    public struct BindingButton
    {
        private static readonly string[] KEY_NAME = { "Click Select", "Open Setting" }; 

        [SerializeField, Dropdown("KEY_NAME")] private string key;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI keyHintText;

        public string Key => key;
        public Button Button
        {
            set => button = value;
            get => button;
        }

        public TextMeshProUGUI KeyHintText => keyHintText;
    }
}