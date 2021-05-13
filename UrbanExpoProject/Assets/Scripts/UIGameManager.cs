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
        [Header("Additional Settings in Game")]
        [SerializeField] private RectTransform optionSettingPanel = null;
        [SerializeField] private RectTransform gamePanel = null;

        [Header("Button Settings")]
        [SerializeField] private BindingButton[] bindingButtons = null;

        [Header("Public Debugger Information")]
        [BoxGroup("DEBUG"), SerializeField] private TextMeshProUGUI gridDisplay = null;

        [Header("Private Debugger Information")]
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Button recentButtonHit = null;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isKeyBinding = false;

        #region Properties
        public bool IsConfiguring
        {
            set
            {
                optionSettingPanel.gameObject.SetActive(value);
                gamePanel.gameObject.SetActive(!value);
            }
            get => optionSettingPanel.gameObject.activeSelf;
        }
        #endregion

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Subscribe events
            EventHandler.EntityWalkedIntoGridEvent += HandleDisplayCurrentGrid;
        }

        private void Start()
        {
            if (optionSettingPanel != null) optionSettingPanel.gameObject.SetActive(false);
            if (gamePanel != null) gamePanel.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (isKeyBinding && Input.anyKeyDown)
            {
                // Set new key bind
                BindingButton bindButton = GetMatchBindingButton(recentButtonHit);
                SetKeyBinding(bindButton);

                // Exit key binding state
                SetActiveInteractable(true);
                isKeyBinding = false;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe events
            EventHandler.EntityWalkedIntoGridEvent -= HandleDisplayCurrentGrid;
        }
        #endregion

        #region Event Methods
        private void HandleDisplayCurrentGrid(EntityWalkedIntoGridEventArgs args)
        {
            if (args.Entity is PlayerEntity)
                gridDisplay.text = $"Current Grid Position: {args.CurrentGridPosition}";
        }
        #endregion

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
                SetActiveInteractable(false);
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

        public override void SetActiveInteractable(bool active)
        {
            base.SetActiveInteractable(active);

            // Activate interaction on binding buttons
            foreach (BindingButton b in bindingButtons)
            {
                if (b.Button != null) b.Button.interactable = active;
            }
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