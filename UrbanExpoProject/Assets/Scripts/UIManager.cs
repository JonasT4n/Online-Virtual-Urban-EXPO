using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class UIManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected InputHandler inputHandler = null;

        [Header("UI Collections")]
        [SerializeField] protected Slider[] slidersCollection = null;
        [SerializeField] protected Button[] buttonsCollection = null;

        #region Properties
        public InputHandler InputHandler => inputHandler;
        #endregion

        protected KeyCode DetectKeyDown()
        {
            // Check each keycode
            foreach (KeyCode code in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(code)) return code;
            }
            return KeyCode.None;
        }

        public virtual void SetActiveInteractable(bool active)
        {
            // Activate interaction on slider
            foreach (Slider s in slidersCollection)
            {
                if (s != null) s.interactable = active;
            }

            // Activate interaction on buttons
            foreach (Button b in buttonsCollection)
            {
                if (b != null) b.interactable = active;
            }
        }
    }

}