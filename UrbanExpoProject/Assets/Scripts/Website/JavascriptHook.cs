using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using System.Runtime.InteropServices;
using SimpleJSON;

namespace UrbanExpo
{

    public class JavascriptHook : MonoBehaviour, IEntityCommands
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void OnInit();

        [DllImport("__Internal")]
        private static extern void OnSceneChange(string sceneName);

        [DllImport("__Internal")]
        private static extern void OnGamePause();

        [DllImport("__Internal")]
        private static extern void OnGameResume();

        [DllImport("__Internal")]
        private static extern void OnPlayerPositionChange(int x, int y, int z);

        [DllImport("__Internal")]
        private static extern void OnPlayerAreaChange(string areaName, string playerName);
#endif

        [Header("Game Assets")]
        [Expandable] public SkinChoiceData skinData = null;
        [SerializeField] private SoundManager soundManager = null;
        [SerializeField, ShowAssetPreview(128, 128)] private Texture2D customCursor = null;

        private static bool isGameInit = false;

        #region Properties
        public static int SkinIndexUsed { set; get; } = 0;
        public static SpriteContent[] SkinDataCollection { set; get; } = new SpriteContent[] { };
        #endregion

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Subscribe events
            SceneManager.sceneLoaded += HandleNewSceneLoaded;
            UIGameManager.OnGamePause += HandlePauseGame;
            UIGameManager.OnGameResume += HandleResumeGame;
            EventHandler.OnEntityWalkedIntoGridEvent += HandlePlayerGridPositionChange;
            EventHandler.OnEntityEnterAreaEvent += HandleEntityEnteredArea;
        }

        private void Start()
        {
            // Initialize components and static data
            SetActiveCustomCursor("{active: true}");
            SkinDataCollection = skinData.SkinContents;

            // Only call once in game
            if (!isGameInit)
            {
#if !UNITY_EDITOR && UNITY_WEBGL
                OnInit();
#endif
                isGameInit = true;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe events
            SceneManager.sceneLoaded -= HandleNewSceneLoaded;
            UIGameManager.OnGamePause -= HandlePauseGame;
            UIGameManager.OnGameResume -= HandleResumeGame;
            EventHandler.OnEntityWalkedIntoGridEvent -= HandlePlayerGridPositionChange;
            EventHandler.OnEntityEnterAreaEvent -= HandleEntityEnteredArea;
        }
        #endregion

        #region Event Methods
        /// <summary>
        /// Fungsi dipanggil saat scene baru di load.
        /// </summary>
        /// <param name="arg0">Struktur scene yang di load</param>
        /// <param name="arg1">Mode load</param>
        private void HandleNewSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            string sceneName = arg0 == null ? "None" : arg0.name;
            OnSceneChange(sceneName);
#endif
        }

        /// <summary>
        /// Fungsi dipanggil pada saat game di berhentikan sementara.
        /// </summary>
        private void HandlePauseGame()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            OnGamePause();
#endif
        }

        /// <summary>
        /// Fungsi dipanggil saat kembali ke game dari pause.
        /// </summary>
        private void HandleResumeGame()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            OnGameResume();
#endif
        }

        private void HandlePlayerGridPositionChange(OnEntityWalkedIntoGridEventArgs args)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            Vector3Int currentGrid = args.CurrentGridPosition;
            OnPlayerPositionChange(currentGrid.x, currentGrid.y, currentGrid.z);
#endif
        }

        private void HandleEntityEnteredArea(OnEntityEnterAreaEventArgs args)
        {
            if (args.Entity is PlayerEntity)
            {
#if !UNITY_EDITOR && UNITY_WEBGL
                OnPlayerAreaChange(args.Area == null ? string.Empty : args.Area.areaName, "Player");
#endif
            }
        }
        #endregion

        /// <summary>
        /// Fungsi untuk teleportasi player ke suatu posisi koordinat grid. 
        /// Fungsi ini adalah overload dari fungsi CmdTeleport(Vector3Int gridCoordinate) agar dapat diakses oleh Javascript. 
        /// <para>Masukan data dengan format data JSON. Contoh:</para>
        /// <code>CmdTeleport("{x: 1, y: 2, z: 3}")</code>
        /// </summary>
        /// <param name="xyz">Koordinat target</param>
        public void CmdTeleport(string xyz)
        {
            JSONObject jsonObj = JSON.Parse(xyz).AsObject;
            CmdTeleport(new Vector3Int(jsonObj["x"].AsInt, jsonObj["y"].AsInt, jsonObj["z"].AsInt));
        }

        /// <summary>
        /// Fungsi untuk teleportasi player ke suatu posisi koordinat grid. 
        /// </summary>
        /// <param name="gridCoordinate">Koordinat target</param>
        public void CmdTeleport(Vector3Int gridCoordinate)
        {
            if (GameManager.singleton != null)
                GameManager.singleton.MainLocalPlayer.CmdTeleport(gridCoordinate);
        }

        /// <summary>
        /// Fungsi command untuk menyuruh player pergi ke posisi koordinat grid. 
        /// Fungsi ini adalah overload dari fungsi CmdMoveTo(Vector3Int targetGridCoordinate) agar dapat diakses oleh Javascript.
        /// <para>Masukan data dengan format data JSON. Contoh:</para>
        /// <code>CmdTeleport("{x: 1, y: 2, z: 3}")</code>
        /// </summary>
        /// <param name="xyz">Koordinat target</param>
        public void CmdMoveTo(string xyz)
        {
            JSONObject jsonObj = JSON.Parse(xyz).AsObject;
            CmdMoveTo(new Vector3Int(jsonObj["x"].AsInt, jsonObj["y"].AsInt, jsonObj["z"].AsInt));
        }

        /// <summary>
        /// Fungsi command untuk menyuruh player pergi ke posisi koordinat grid. 
        /// </summary>
        /// <param name="targetGridCoordinate">Koordinat target</param>
        public void CmdMoveTo(Vector3Int targetGridCoordinate)
        {
            if (GameManager.singleton != null)
                GameManager.singleton.MainLocalPlayer.CmdMoveTo(targetGridCoordinate);
        }

        /// <summary>
        /// Fungsi agar player utama berhenti bergerak.
        /// </summary>
        public void CmdStopMoving()
        {
            if (GameManager.singleton != null)
                GameManager.singleton.MainLocalPlayer.CmdStopMoving();
        }

        /// <summary>
        /// Fungsi untuk kembali ke game utama setelah berinteraksi dengan NPC atau objek. 
        /// Fungsi ini dapat diperlukan untuk dipanggil keluar dari masa interaksi, khususnya untuk platform Web.
        /// </summary>
        public void StopInteraction()
        {
            PlayerEntity mainPlayer = GameManager.singleton.MainLocalPlayer;
            if (mainPlayer.CurrentlyInteractWith != null)
            {
                mainPlayer.CurrentlyInteractWith.StopInteract();
                mainPlayer.CurrentlyInteractWith = null;
            }

            GameManager.singleton.uiGameManager.SetActiveIContent(false);
        }

        /// <summary>
        /// Fungsi pause dapat dipanggil secara programmatically. 
        /// Fungsi ini hanya dapat bekerja pada saat user berada di dalam Scene game tersebut, fungsi ini tidak dapat dipanggil di Scene lainnya. 
        /// <para>Masukan data dengan format data JSON. Contoh:</para>
        /// <code>SetGamePause("{pause: true}")</code>
        /// </summary>
        /// <param name="pause"></param>
        public void SetGamePause(string pause)
        {
            if (GameManager.singleton != null)
            {
                UIGameManager uiManager = GameManager.singleton.uiGameManager;

                JSONObject jsonObj = JSON.Parse(pause).AsObject;
                uiManager.IsConfiguring = jsonObj["pause"].AsBool;
            }
        }

        /// <summary>
        /// Fungsi untuk menyalakan dan mematikan suara game, baik itu volume maupun SFX.
        /// <para>Masukan data dengan format data JSON. Contoh:</para>
        /// <code>SetMuteGame("{isMuted: true}")</code>
        /// </summary>
        /// <param name="isMuted"></param>
        public void SetMuteGame(string isMuted)
        {
            JSONObject jsonObj = JSON.Parse(isMuted).AsObject;
            soundManager.ToggleSoundMaster(!jsonObj["isMuted"].AsBool);
        }

        /// <summary>
        /// Fungsi untuk aktifkan custom kursor mouse.
        /// <para>Masukan data dengan format data JSON. Contoh:</para>
        /// <code>SetActiveCustomCursor("{active: true}")</code>
        /// </summary>
        /// <param name="active"></param>
        public void SetActiveCustomCursor(string active)
        {
            JSONObject jsonObj = JSON.Parse(active).AsObject;
            Cursor.SetCursor(jsonObj["active"].AsBool ? customCursor : null, Vector2.zero, CursorMode.Auto);
        }
    }

}