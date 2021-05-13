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

        [Header("Others")]
        [Required("Camera controller required for local player to make camera controlable")]
        [SerializeField] private CameraController cameraController = null;
        [SerializeField] private PlayerEntity mainPlayerPrefab = null;

        private PlayerEntity mainPlayer;

        #region Properties
        public CameraController CameraController => cameraController;
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
            mainPlayer = (PlayerEntity)IslandGrid.singleton.Spawn(mainPlayerPrefab);
            mainPlayer.Content = JavascriptHook.singleton.skinData.SkinContents[JavascriptHook.singleton.SkinIndexUsed];
        }
    }

}