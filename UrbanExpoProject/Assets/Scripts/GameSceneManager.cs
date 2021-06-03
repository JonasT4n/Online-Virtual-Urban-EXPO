using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class GameSceneManager : MonoBehaviour
    {
        [Header("Scenes Data Attribute")]
        [SerializeField, Scene] private string mainMenuScene = string.Empty;

        [Header("Additional UI")]
        [SerializeField] private RectTransform loadingScenePanel = null;
        [SerializeField, ShowIf("IsLoadingScenPanelSet"), Foldout("Loading Screen Attributes")] private CanvasGroup loadingCanvas = null;
        [SerializeField, ShowIf("IsLoadingScenPanelSet"), Foldout("Loading Screen Attributes")] private Slider loadingBar = null;
        [SerializeField, ShowIf("IsLoadingScenPanelSet"), Foldout("Loading Screen Attributes")] private TextMeshProUGUI loadingProgressText = null;

        #region Properties
        private bool IsLoadingScenPanelSet => loadingScenePanel != null;
        #endregion

        #region Unity BuiltIn Methods
        private void Start()
        {
            if (loadingScenePanel != null)
                loadingScenePanel.gameObject.SetActive(false);
        }
        #endregion

        public void LoadToMainMenuScene()
        {
            LoadToScene(mainMenuScene);
        }

        public void LoadToScene(string sceneName)
        {
            // Check main menu state
            if (UIMainMenuManager.singleton != null)
            {
                UIMainMenuManager.singleton.SetInteractable(false);
            }

            StartCoroutine(LoadToSceneAsync(sceneName));
        }

        private IEnumerator LoadToSceneAsync(string sceneName)
        {
            // Fading to loading screen
            if (loadingScenePanel != null)
            {
                loadingScenePanel.gameObject.SetActive(true);
                if (loadingCanvas != null)
                {
                    loadingCanvas.alpha = 0;
                    float secondsFadeIn = 0.5f;
                    float tempSeconds = secondsFadeIn;

                    while (loadingCanvas.alpha < 1f)
                    {
                        tempSeconds -= Time.deltaTime;
                        loadingCanvas.alpha = (secondsFadeIn - tempSeconds) / secondsFadeIn;
                        yield return null;
                    }
                }
            }

            // Process loading scene
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            string progressText = loadingProgressText != null ? loadingProgressText.text : string.Empty;
            while (!op.isDone)
            {
                // Set loading bar value
                if (loadingBar != null)
                {
                    float normalValue = (op.progress / 0.9f - loadingBar.minValue) / (loadingBar.maxValue - loadingBar.minValue);
                    loadingBar.value = normalValue;
                }

                // Set text value
                if (loadingProgressText != null)
                {
                    loadingProgressText.text = $"{progressText} {op.progress / 0.9f * 100}%";
                }

                yield return null;
            }
        }
    }

}