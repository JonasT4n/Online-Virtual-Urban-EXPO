using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;

namespace UrbanExpo
{
    public class UIMainMenuManager : UIManager
    {
        public static UIMainMenuManager singleton;

        [Header("Requirement Data Path")]
        public SoundManager soundManager = null;
        public GameSceneManager sceneManager = null;
        public JavascriptHook webHook = null;

        [Header("Additional Settings in Main Menu")]
        public RectTransform customSkinSettingPanel = null;
        [SerializeField, ShowIf("IsCustomSkinPanelExists"), Foldout("Custom Skin UI Attributes")] private UIHorizontalInfiniteScroller csScroller = null;
        [SerializeField, ShowIf("IsCustomSkinPanelExists"), Foldout("Custom Skin UI Attributes")] private Image csBackground = null;
        [SerializeField, ShowIf("IsCustomSkinPanelExists"), Foldout("Custom Skin UI Attributes")] private Image csInner = null;

        #region Properties
        private bool IsCustomSkinPanelExists => customSkinSettingPanel != null;
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
            // Check if custom skin panel was already active
            if (customSkinSettingPanel.gameObject.activeSelf) OpenCustomSkinSettings(true);
        }

        private void OnDestroy()
        {
            singleton = null;
        }
        #endregion

        public void EnterGame()
        {
            
        }

        public void OpenCustomSkinSettings(bool active)
        {
            // Check panel master exists
            if (customSkinSettingPanel == null)
                return;

            // Animate UI movement whenever activate or deactivate
            if (active)
            {
                csBackground.gameObject.SetActive(true);
                if (csBackground != null)
                    csBackground.DOColor(new Color(0f, 0f, 0f, 0.5f), 0.35f);
                if (csInner != null)
                    csInner.rectTransform.DOAnchorPos(Vector2.zero, 0.35f).SetEase(Ease.OutCirc);
                csScroller.SetContents(webHook.skinData.SkinContents, JavascriptHook.SkinIndexUsed);
            }
            else
            {
                if (csBackground != null)
                    csBackground.DOColor(new Color(0f, 0f, 0f, 0f), 0.35f);
                if (csInner != null)
                    csInner.rectTransform.DOAnchorPos(new Vector2(0f, -1200f), 0.35f).SetEase(Ease.InCirc);
                StartCoroutine(SetActiveAfterTween(csBackground.gameObject, 0.35f, false));
            }
        }

        public void ConfirmSkin()
        {
            JavascriptHook.SkinIndexUsed = csScroller.ContentIndex;
        }

        public IEnumerator SetActiveAfterTween(GameObject obj, float seconds, bool active)
        {
            yield return new WaitForSeconds(seconds);
            obj.SetActive(active);
        }
    }

}