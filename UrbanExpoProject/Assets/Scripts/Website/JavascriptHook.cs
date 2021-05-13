using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using System.Runtime.InteropServices;

namespace UrbanExpo
{

    public class JavascriptHook : MonoBehaviour
    {
        public static JavascriptHook singleton;

        [DllImport("__internal")]
        private static extern void InitMain();

        [DllImport("__internal")]
        private static extern void OnSceneChange(string sceneName);

        [Header("Game Assets")]
        [Expandable] public SkinChoiceData skinData = null;
        [SerializeField, ShowAssetPreview(128, 128)] private Texture2D customCursor = null;

        #region Properties
        public int SkinIndexUsed { set; get; } = 0;
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
            DontDestroyOnLoad(this);

            // Subscribe events
            SceneManager.sceneLoaded += HandleNewSceneLoaded;
        }

        private void Start()
        {
            SetActiveCustomCursor(true);

#if !UNITY_EDITOR && UNITY_WEBGL
            InitMain();
#endif
        }

        private void OnDestroy()
        {
            singleton = null;

            // Unsubscribe events
            SceneManager.sceneLoaded -= HandleNewSceneLoaded;
        }
        #endregion

        private void HandleNewSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            OnSceneChange(arg0.name);
#endif
        }

        public void SetActiveCustomCursor(bool active)
        {
            Cursor.SetCursor(active ? customCursor : null, Vector2.zero, CursorMode.Auto);
        }
    }

}