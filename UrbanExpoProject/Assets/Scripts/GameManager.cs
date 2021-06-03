using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;

namespace UrbanExpo
{
    public enum GameMode { MainGame, BuildingMap }

    public class GameManager : MonoBehaviour
    {
        public static GameManager singleton;

        [Header("Requirement Data Path")]
        public UIGameManager uiGameManager = null;
        public SoundManager soundManager = null;
        public GameSceneManager sceneManager = null;
        public JavascriptHook webHook = null;

        [Header("Others")]
        [Required("Camera controller required for local player to make camera controlable")]
        [SerializeField] private CameraController cameraController = null;
        [SerializeField] private PlayerEntity mainPlayerPrefab = null;

        private PlayerEntity mainLocalPlayer;

        #region Properties
        public CameraController CameraController => cameraController;
        public PlayerEntity MainLocalPlayer => mainLocalPlayer;
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
        }

        private void Start()
        {
            SetUpGame();
        }

        private void OnDestroy()
        {
            singleton = null;
        }
        #endregion

        private void SetUpGame()
        {
            mainLocalPlayer = (PlayerEntity)IslandGrid.singleton.Spawn(mainPlayerPrefab);
            mainLocalPlayer.Content = webHook.skinData.SkinContents[JavascriptHook.SkinIndexUsed];
            cameraController.SetFollowTarget(mainLocalPlayer.transform);
        }
    }

}